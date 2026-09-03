document.addEventListener("DOMContentLoaded", function () {
    const mapElement = document.getElementById("event-location-map");
    const latitudeInput = document.getElementById("latitude-input");
    const longitudeInput = document.getElementById("longitude-input");
    const addressInput = document.getElementById("address-input");
    const resolveAddressBtn = document.getElementById("resolve-address-btn");
    const mapStatus = document.getElementById("map-status");

    if (!mapElement || !latitudeInput || !longitudeInput || !addressInput || typeof L === "undefined") {
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

            const resolvedAddress = data?.display_name?.trim();
            if (resolvedAddress) {
                addressInput.value = resolvedAddress;
                setStatus("Adres bijgewerkt op basis van de pin.");
            } else {
                setStatus("Geen adres gevonden voor deze locatie.", true);
            }
        } catch (error) {
            setStatus("Kon geen adres ophalen voor deze locatie.", true);
        }
    }

    async function forwardGeocode(query) {
        const trimmed = (query || "").trim();
        if (!trimmed) {
            setStatus("Vul eerst een adres in.", true);
            return;
        }

        const requestId = ++activeRequestId;
        setStatus("Adres zoeken...");

        try {
            const url = new URL("https://nominatim.openstreetmap.org/search");
            url.searchParams.set("format", "jsonv2");
            url.searchParams.set("q", trimmed);
            url.searchParams.set("limit", "1");
            url.searchParams.set("addressdetails", "1");

            const response = await fetch(url.toString(), {
                headers: {
                    "Accept": "application/json"
                }
            });

            if (!response.ok) {
                throw new Error(`Geocoding mislukt (${response.status}).`);
            }

            const data = await response.json();

            if (requestId !== activeRequestId) {
                return;
            }

            if (!Array.isArray(data) || data.length === 0) {
                setStatus("Geen locatie gevonden voor dit adres.", true);
                return;
            }

            const bestMatch = data[0];
            const lat = parseFloat(bestMatch.lat);
            const lng = parseFloat(bestMatch.lon);

            if (!Number.isFinite(lat) || !Number.isFinite(lng)) {
                setStatus("Geen geldige coördinaten ontvangen.", true);
                return;
            }

            addressInput.value = bestMatch.display_name || trimmed;
            setMarker(lat, lng, true);
            setStatus("Adres gevonden en pin geplaatst.");
        } catch (error) {
            setStatus("Kon dit adres niet omzetten naar coördinaten.", true);
        }
    }

    if (latitudeInput.value && longitudeInput.value) {
        setMarker(initialLat, initialLng, false);
        if (!addressInput.value.trim()) {
            reverseGeocode(initialLat, initialLng);
        }
    }

    map.on("click", async function (e) {
        setMarker(e.latlng.lat, e.latlng.lng, false);
        await reverseGeocode(e.latlng.lat, e.latlng.lng);
    });

    if (resolveAddressBtn) {
        resolveAddressBtn.addEventListener("click", async function () {
            await forwardGeocode(addressInput.value);
        });
    }

    addressInput.addEventListener("keydown", async function (event) {
        if (event.key === "Enter") {
            event.preventDefault();
            await forwardGeocode(addressInput.value);
        }
    });

    addressInput.addEventListener("blur", async function () {
        const hasAddress = addressInput.value.trim() !== "";
        const hasCoordinates = latitudeInput.value && longitudeInput.value;

        if (hasAddress && !hasCoordinates) {
            await forwardGeocode(addressInput.value);
        }
    });
});