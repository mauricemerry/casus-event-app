document.addEventListener("DOMContentLoaded", function () {
    const mapElement = document.getElementById("event-location-map");
    const latitudeInput = document.getElementById("latitude-input");
    const longitudeInput = document.getElementById("longitude-input");
    const mapStatus = document.getElementById("map-status");
    const firstLocation = document.querySelector('[data-location-panel="0"]');
    const locationLatitudeInput = firstLocation?.querySelector(".location-lat");
    const locationLongitudeInput = firstLocation?.querySelector(".location-lng");
    const streetInput = firstLocation?.querySelector('[name="Input.Locations[0].Street"]');
    const numberInput = firstLocation?.querySelector('[name="Input.Locations[0].Number"]');
    const postalCodeInput = firstLocation?.querySelector('[name="Input.Locations[0].PostalCode"]');
    const cityInput = firstLocation?.querySelector('[name="Input.Locations[0].City"]');

    if (!mapElement || !latitudeInput || !longitudeInput || typeof L === "undefined") {
        return;
    }

    const defaultLat = 50.8882;
    const defaultLng = 5.9795;
    const defaultZoom = 13;

    const initialLat = latitudeInput.value ? parseFloat(latitudeInput.value) : defaultLat;
    const initialLng = longitudeInput.value ? parseFloat(longitudeInput.value) : defaultLng;

    const map = L.map(mapElement).setView([initialLat, initialLng], defaultZoom);

    L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", {
        maxZoom: 19,
        attribution: "&copy; OpenStreetMap"
    }).addTo(map);

    let marker = null;
    let activeRequestId = 0;

    function setStatus(message, isError = false) {
        if (!mapStatus) return;
        mapStatus.textContent = message || "";
        mapStatus.classList.toggle("portal-map-status--error", isError);
    }

    function setCoordinates(lat, lng) {
        latitudeInput.value = lat.toFixed(6).replace('.', ',');
        longitudeInput.value = lng.toFixed(6).replace('.', ',');

        if (locationLatitudeInput) {
            locationLatitudeInput.value = latitudeInput.value;
        }

        if (locationLongitudeInput) {
            locationLongitudeInput.value = longitudeInput.value;
        }
    }

    function setLocationFields(address) {
        if (!address) {
            return;
        }

        if (streetInput) {
            streetInput.value = address.road || address.pedestrian || address.footway || "";
        }

        if (numberInput) {
            numberInput.value = address.house_number || "0";
        }

        if (postalCodeInput) {
            postalCodeInput.value = address.postcode || "";
        }

        if (cityInput) {
            cityInput.value = address.city || address.town || address.village || address.municipality || "";
        }
    }

    function setMarker(lat, lng, shouldCenter = true) {
        setCoordinates(lat, lng);

        if (marker) {
            marker.setLatLng([lat, lng]);
        } else {
            marker = L.marker([lat, lng], { draggable: true }).addTo(map);

            marker.on("dragend", async function (e) {
                const position = e.target.getLatLng();
                setCoordinates(position.lat, position.lng);
                await reverseGeocode(position.lat, position.lng);
            });
        }

        if (shouldCenter) {
            map.setView([lat, lng], Math.max(map.getZoom(), 15));
        }
    }

    async function reverseGeocode(lat, lng) {
        const requestId = ++activeRequestId;
        setStatus("Adres ophalen...");

        try {
            const url = new URL("https://nominatim.openstreetmap.org/reverse");
            url.searchParams.set("format", "jsonv2");
            url.searchParams.set("lat", String(lat));
            url.searchParams.set("lon", String(lng));
            url.searchParams.set("addressdetails", "1");

            const response = await fetch(url.toString(), {
                headers: {
                    "Accept": "application/json"
                }
            });

            if (!response.ok) {
                throw new Error(`Reverse geocoding mislukt (${response.status}).`);
            }

            const data = await response.json();

            if (requestId !== activeRequestId) {
                return;
            }

            setLocationFields(data?.address);
            setStatus(data?.address
                ? "Locatievelden bijgewerkt op basis van de pin."
                : "Geen adres gevonden voor deze locatie.", !data?.address);
        } catch (error) {
            setStatus("Kon geen adres ophalen voor deze locatie.", true);
        }
    }

    if (latitudeInput.value && longitudeInput.value) {
        setMarker(initialLat, initialLng, false);
        setStatus("Bestaande locatie geladen.");
    }

    map.on("click", async function (e) {
        setMarker(e.latlng.lat, e.latlng.lng, false);
        await reverseGeocode(e.latlng.lat, e.latlng.lng);
    });

    window.addEventListener("resize", () => map.invalidateSize());
    window.addEventListener("edit-location-step-visible", () => {
        setTimeout(() => map.invalidateSize(), 50);
    });

});