
document.addEventListener("DOMContentLoaded", () => {

    const $ = (selector, root = document) =>
        root.querySelector(selector);

    const $$ = (selector, root = document) =>
        [...root.querySelectorAll(selector)];

    const form = $("#wizard-form");

    let step = 1;
    let extra = "access";
    let activeLocation = 0;
    let map = null;
    let marker = null;

    const val = name =>
        $(`[name="${name}"]`)?.value?.trim() || "";

    // ====================================
    // 1. HOOFDNAVIGATIE
    // ====================================

    function showStep(n) {

        step = Math.max(1, Math.min(5, n));

        $$("[data-content-step]").forEach(element => {
            element.hidden =
                Number(element.dataset.contentStep) !== step;
        });

        $$("[data-step]").forEach(button => {
            button.classList.toggle(
                "active",
                Number(button.dataset.step) === step
            );
        });

        $("#previous-step").disabled = step === 1;
        $("#next-step").hidden = step === 5;
        $("#publish-event").hidden = step !== 5;

        $("#step-counter").textContent =
            `Stap ${step} van 5`;

        if (step === 5) {
            renderSummary();
        }

        if (step === 4 && extra === "locations") {
            setTimeout(refreshMap, 40);
        }
    }

    $$("[data-step]").forEach(button => {
        button.addEventListener("click", () => {
            showStep(Number(button.dataset.step));
        });
    });

    $("#previous-step").addEventListener("click", () => {
        showStep(step - 1);
    });

    $("#next-step").addEventListener("click", () => {
        showStep(step + 1);
    });

    // ====================================
    // 2. EXTRA INFO SUBTABS
    // ====================================

    function showExtra(name) {

        extra = name;

        $$("[data-extra-content]").forEach(element => {
            element.hidden =
                element.dataset.extraContent !== name;
        });

        $$("[data-extra]").forEach(button => {
            button.classList.toggle(
                "active",
                button.dataset.extra === name
            );
        });

        if (name === "locations" && step === 4) {
            setTimeout(refreshMap, 40);
        }
    }

    $$("[data-extra]").forEach(button => {
        button.addEventListener("click", () => {
            showExtra(button.dataset.extra);
        });
    });

    // Overige toegankelijkheid

    const other = $("#other-access");

    function syncOther() {
        $("#other-access-wrap").hidden = !other.checked;
    }

    other.addEventListener("change", syncOther);

    syncOther();

    // ====================================
    // 3. DYNAMISCHE PERIODES
    // ====================================

    function reindexPeriods() {

        $$("#period-list .period-row").forEach((row, i) => {

            $$("input", row).forEach((field, k) => {

                const properties = [
                    "Name",
                    "StartDate",
                    "StartTime",
                    "EndDate",
                    "EndTime"
                ];

                field.name =
                    `Input.Periods[${i}].${properties[k]}`;

                field.id = `period-${i}-${k}`;
            });
        });
    }

    $("#add-period").addEventListener("click", () => {

        $("#period-list").insertAdjacentHTML(
            "beforeend",
            `
            <div class="period-row">

                <input placeholder="Naam periode" />

                <input type="date" />

                <input type="time" />

                <input type="date" />

                <input type="time" />

                <button type="button"
                        class="remove-row"
                        aria-label="Periode verwijderen">
                    ✕
                </button>

            </div>
            `
        );

        reindexPeriods();
    });

    $("#period-list").addEventListener("click", event => {

        if (event.target.closest(".remove-row")) {

            event.target.closest(".period-row").remove();

            reindexPeriods();
        }
    });

    reindexPeriods();

    // ====================================
    // 4. DYNAMISCHE LOCATIES
    // ====================================

    const locationList = $("#location-list");

    const locRows = () =>
        $$(".location-row", locationList);

    function reindexLocations() {

        locRows().forEach((row, i) => {

            $$("input", row).forEach(field => {

                field.name = field.name.replace(
                    /Input\.Locations\[\d+\]/,
                    `Input.Locations[${i}]`
                );
            });
        });
    }

    function renderLocationTabs() {

        const nav = $("#location-tabs");

        nav.replaceChildren();

        activeLocation = Math.min(
            activeLocation,
            Math.max(0, locRows().length - 1)
        );

        locRows().forEach((row, i) => {

            row.hidden = i !== activeLocation;

            const button = document.createElement("button");

            button.type = "button";

            button.textContent =
                $(".location-name", row)?.value ||
                `Locatie ${i + 1}`;

            button.className =
                i === activeLocation ? "active" : "";

            button.addEventListener("click", () => {

                activeLocation = i;

                renderLocationTabs();

                refreshMap();
            });

            nav.appendChild(button);
        });

        const add = document.createElement("button");

        add.type = "button";
        add.textContent = "+";
        add.title = "Locatie toevoegen";

        add.addEventListener("click", addLocation);

        nav.appendChild(add);
    }

    function addLocation() {

        const i = locRows().length;

        locationList.insertAdjacentHTML(
            "beforeend",
            `
            <div class="event-panel location-row"
                 data-location-row>

                <div class="portal-form-group">
                    <label>Locatienaam</label>

                    <input class="location-name"
                           name="Input.Locations[${i}].Name"
                           value="Locatie ${i + 1}" />
                </div>

                <div class="portal-form-group">
                    <label>Plaats</label>

                    <input name="Input.Locations[${i}].City" />
                </div>

                <div class="portal-form-group">
                    <label>Straat</label>

                    <input name="Input.Locations[${i}].Street" />
                </div>

                <div class="event-grid">

                    <div class="portal-form-group">
                        <label>Nummer</label>

                        <input name="Input.Locations[${i}].Number" />
                    </div>

                    <div class="portal-form-group">
                        <label>Toevoeging</label>

                        <input name="Input.Locations[${i}].Addition" />
                    </div>

                </div>

                <div class="portal-form-group">
                    <label>Postcode</label>

                    <input name="Input.Locations[${i}].PostalCode" />
                </div>

                <input type="hidden"
                       class="location-lat"
                       name="Input.Locations[${i}].Latitude" />

                <input type="hidden"
                       class="location-lng"
                       name="Input.Locations[${i}].Longitude" />

                <button type="button"
                        class="remove-location portal-secondary-link">
                    Locatie verwijderen
                </button>

            </div>
            `
        );

        activeLocation = i;

        renderLocationTabs();

        refreshMap();
    }

    locationList.addEventListener("input", event => {

        if (event.target.matches(".location-name")) {
            renderLocationTabs();
        }
    });

    locationList.addEventListener("click", event => {

        if (!event.target.closest(".remove-location")) {
            return;
        }

        if (locRows().length === 1) {
            alert("Een hoofdlocatie is verplicht.");
            return;
        }

        event.target.closest(".location-row").remove();

        reindexLocations();

        renderLocationTabs();

        refreshMap();
    });

    function currentLocation() {
        return locRows()[activeLocation];
    }

    function coord(field) {
        return Number(
            (field?.value || "").replace(",", ".")
        );
    }

    function setCoords(lat, lng) {

        const row = currentLocation();

        if (!row) return;

        $(".location-lat", row).value =
            lat.toFixed(6).replace(".", ",");

        $(".location-lng", row).value =
            lng.toFixed(6).replace(".", ",");
    }

    // ====================================
    // 5. LEAFLET-KAART
    // ====================================

    function refreshMap() {

        if (
            !window.L ||
            step !== 4 ||
            extra !== "locations"
        ) {
            return;
        }

        if (!map) {

            map = L.map("event-location-map")
                .setView([50.8882, 5.9795], 13);

            L.tileLayer(
                "https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png",
                {
                    maxZoom: 19,
                    attribution: "&copy; OpenStreetMap"
                }
            ).addTo(map);

            map.on("click", event => {

                setCoords(
                    event.latlng.lat,
                    event.latlng.lng
                );

                setPin(
                    event.latlng.lat,
                    event.latlng.lng
                );
            });
        }

        setTimeout(() => {
            map.invalidateSize();
        }, 50);

        const row = currentLocation();

        if (!row) return;

        const latField = $(".location-lat", row);
        const lngField = $(".location-lng", row);

        if (latField.value && lngField.value) {

            const lat = coord(latField);
            const lng = coord(lngField);

            if (
                Number.isFinite(lat) &&
                Number.isFinite(lng)
            ) {
                setPin(lat, lng);
                map.setView([lat, lng], 15);
            }

        } else if (marker) {

            map.removeLayer(marker);
            marker = null;
        }
    }

    function setPin(lat, lng) {

        if (!map) return;

        if (marker) {

            marker.setLatLng([lat, lng]);

        } else {

            marker = L.marker(
                [lat, lng],
                { draggable: true }
            ).addTo(map);

            marker.on("dragend", event => {

                const position = event.target.getLatLng();

                setCoords(position.lat, position.lng);
            });
        }

        $("#map-status").textContent =
            `Pin gekozen: ${lat.toFixed(5)}, ${lng.toFixed(5)}`;
    }

    $("#find-location").addEventListener(
        "click",
        async () => {

            const row = currentLocation();

            if (!row) return;

            const values = [
                "Street",
                "Number",
                "PostalCode",
                "City"
            ]
            .map(key =>
                $(`[name$=".${key}"]`, row)?.value || ""
            )
            .filter(Boolean);

            if (!values.length) {

                $("#map-status").textContent =
                    "Vul eerst adresgegevens in.";

                return;
            }

            $("#map-status").textContent =
                "Locatie zoeken...";

            try {

                const url =
                    "https://nominatim.openstreetmap.org/search" +
                    "?format=jsonv2&limit=1&q=" +
                    encodeURIComponent(values.join(" "));

                const response = await fetch(url);

                if (!response.ok) {
                    throw new Error("Zoeken mislukt");
                }

                const matches = await response.json();

                if (!matches.length) {
                    throw new Error("Geen resultaat");
                }

                const lat = Number(matches[0].lat);
                const lng = Number(matches[0].lon);

                setCoords(lat, lng);

                refreshMap();

            } catch {

                $("#map-status").textContent =
                    "Adres niet gevonden. Kies zelf een pin.";
            }
        }
    );

    renderLocationTabs();

    // ====================================
    // 6. DYNAMISCHE TICKETS
    // ====================================

    const tickets = $("#ticket-list");

    const ticketRows = () =>
        $$(".ticket-row", tickets);

    function reindexTickets() {

        ticketRows().forEach((row, i) => {

            $$('[name^="Input.Tickets["]', row)
                .forEach(field => {

                    field.name = field.name.replace(
                        /Input\.Tickets\[\d+\]/,
                        `Input.Tickets[${i}]`
                    );
                });

            $$(".period-date-list input", row)
                .forEach((input, j) => {

                    input.name =
                        `Input.Tickets[${i}].PeriodDates[${j}]`;
                });
        });
    }

    function syncTicket(row) {

        const name = $(".ticket-name", row)
            .value.trim().toLowerCase();

        const kind = $(".ticket-kind", row).value;

        // Ticketnaam Gratis => automatisch type Gratis.
        if (name === "gratis" && kind !== "Gratis") {

            $(".ticket-kind", row).value = "Gratis";
        }

        const realKind = $(".ticket-kind", row).value;

        const free =
            realKind === "Gratis" ||
            name === "gratis";

        // Gratis ticket: prijsveld verdwijnt.
        $(".ticket-price-wrap", row).hidden = free;

        if (free) {
            $(".ticket-price", row).value = "0";
        }

        $(".ticket-single", row).hidden =
            !["Dagticket", "Tijdslot"].includes(realKind);

        $(".ticket-slot", row).hidden =
            realKind !== "Tijdslot";

        $(".ticket-period", row).hidden =
            realKind !== "Periode";
    }

    function addTicket() {

        const i = ticketRows().length;

        tickets.insertAdjacentHTML(
            "beforeend",
            `
            <div class="ticket-row">

                <input class="ticket-name"
                       name="Input.Tickets[${i}].TicketType"
                       placeholder="Naam (bijv. Volwassenen)" />

                <div class="ticket-price-wrap">

                    <input class="ticket-price"
                           type="number"
                           step="0.01"
                           min="0"
                           name="Input.Tickets[${i}].Price"
                           placeholder="Prijs (€)" />

                </div>

                <select class="ticket-kind"
                        name="Input.Tickets[${i}].DateType">

                    <option>Gratis</option>
                    <option selected>Dagticket</option>
                    <option>Periode</option>
                    <option>Tijdslot</option>

                </select>

                <div class="ticket-dates">

                    <div class="ticket-single">

                        <input type="date"
                               name="Input.Tickets[${i}].TicketDate" />

                    </div>

                    <div class="ticket-slot">

                        <input type="time"
                               name="Input.Tickets[${i}].SlotStart" />

                        <input type="time"
                               name="Input.Tickets[${i}].SlotEnd" />

                    </div>

                    <div class="ticket-period">

                        <div class="period-date-list"></div>

                        <button type="button"
                                class="add-day portal-secondary-link">
                            + dag toevoegen
                        </button>

                    </div>

                </div>

                <button type="button"
                        class="remove-ticket remove-row"
                        aria-label="Ticket verwijderen">
                    ✕
                </button>

            </div>
            `
        );

        syncTicket(ticketRows().at(-1));
    }

    $("#add-ticket").addEventListener(
        "click",
        addTicket
    );

    tickets.addEventListener("input", event => {

        const row =
            event.target.closest(".ticket-row");

        if (
            row &&
            event.target.matches(".ticket-name")
        ) {
            syncTicket(row);
        }
    });

    tickets.addEventListener("change", event => {

        const row =
            event.target.closest(".ticket-row");

        if (
            row &&
            event.target.matches(".ticket-kind")
        ) {
            syncTicket(row);
        }
    });

    tickets.addEventListener("click", event => {

        const row =
            event.target.closest(".ticket-row");

        if (!row) return;

        if (event.target.closest(".remove-ticket")) {

            row.remove();

            reindexTickets();

            return;
        }

        if (event.target.closest(".add-day")) {

            const list =
                $(".period-date-list", row);

            const input =
                document.createElement("input");

            input.type = "date";

            input.name =
                `Input.Tickets[${ticketRows().indexOf(row)}]` +
                `.PeriodDates[${list.children.length}]`;

            list.appendChild(input);
        }
    });

    ticketRows().forEach(syncTicket);

    // ====================================
    // 7. TAGS
    // ====================================

    const tags = $("#tags-list");
    const tagInput = $("#tag-text");

    let tagValues = $$(
        'input[type="hidden"]',
        tags
    )
    .map(element => element.value)
    .filter(Boolean);

    function renderTags() {

        tags.replaceChildren();

        tagValues.forEach((value, i) => {

            const pill =
                document.createElement("button");

            pill.type = "button";
            pill.className = "tag-pill";

            pill.textContent = "#" + value + " ×";

            pill.addEventListener("click", () => {

                tagValues.splice(i, 1);

                renderTags();
            });

            tags.appendChild(pill);

            const hidden =
                document.createElement("input");

            hidden.type = "hidden";

            hidden.name = `Input.Tags[${i}]`;
            hidden.value = value;

            tags.appendChild(hidden);
        });
    }

    function addTag() {

        const tag = tagInput.value
            .trim()
            .replace(/^#/, "");

        const exists = tagValues.some(
            value => value.toLowerCase() === tag.toLowerCase()
        );

        if (tag && !exists) {
            tagValues.push(tag);
        }

        tagInput.value = "";

        renderTags();
    }

    tagInput.addEventListener("keydown", event => {

        if (
            event.key === "Enter" ||
            event.key === ","
        ) {
            event.preventDefault();
            addTag();
        }
    });

    tagInput.addEventListener("blur", addTag);

    renderTags();

    // ====================================
    // 8. DYNAMISCHE SAMENVATTING
    // ====================================

    function renderSummary() {

        const root = $("#summary-content");

        root.replaceChildren();

        function box(title, lines) {

            const panel =
                document.createElement("div");

            panel.className = "event-panel";

            const heading =
                document.createElement("h3");

            heading.textContent = title;

            panel.appendChild(heading);

            lines.forEach(line => {

                const paragraph =
                    document.createElement("p");

                paragraph.textContent = line;

                panel.appendChild(paragraph);
            });

            root.appendChild(panel);
        }

        // BASIS EN DETAILS

        box("Basis & details", [

            val("Input.EventName"),

            val("Input.EventType"),

            val("Input.ShortDescription"),

            val("Input.Description"),

            [
                $("#Input_IsIndoor")?.checked
                    ? "Binnen" : "",

                $("#Input_IsOutdoor")?.checked
                    ? "Buiten" : "",

                $("#Input_IsOnline")?.checked
                    ? "Online" : ""

            ].filter(Boolean).join(", ")

        ]);

        // PERIODES

        box(
            "Periodes",

            $$("#period-list .period-row").map(row =>

                $$("input", row)
                    .slice(0, 5)
                    .map(input => input.value)
                    .join(" | ")
            )
        );

        // LOCATIES

        box(
            "Locaties",

            locRows().map(row =>

                $$('input:not([type="hidden"])', row)
                    .map(input => input.value)
                    .filter(Boolean)
                    .join(", ")
            )
        );

        // TICKETS

        box(
            "Tickets",

            ticketRows().map(row =>

                `${$(".ticket-name", row).value} | ` +
                `${$(".ticket-kind", row).value} | ` +
                `€${$(".ticket-price", row).value || "0"}`
            )
        );

        // EXTRA INFO

        box("Extra informatie", [

            "Categorie: " +
                val("Input.SelectedCategory"),

            "Tags: " +
                tagValues.join(", "),

            "Toegankelijkheid: " +
                $$(
                    'input[name="Input.AccessibilityOptions"]:checked'
                )
                .map(input => input.value)
                .join(", "),

            "Overige: " +
                (other.checked
                    ? val("Input.OtherAccessibilityDescription")
                    : ""),

            "Notities: " +
                val("Input.Notes"),

            "Afbeeldingen: " +
                ($("#images-input").files?.length || 0)

        ]);
    }

    // Nog ingetypte tags opslaan bij volgende stap.

    $("#next-step").addEventListener("click", () => {

        if (tagInput.value.trim()) {
            addTag();
        }
    });

    // OUDE OMGEVINGSKOLOM INVULLEN VOOR COMPATIBILITEIT.

    form.addEventListener("submit", () => {

        if (tagInput.value.trim()) {
            addTag();
        }

        const indoor =
            $("#Input_IsIndoor").checked;

        const outdoor =
            $("#Input_IsOutdoor").checked;

        $("#legacy-environment").value =
            indoor && outdoor ? "Beide" :
            indoor ? "Binnen" :
            outdoor ? "Buiten" : "Beide";
    });

    showStep(1);
});
