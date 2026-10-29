
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
    // 1. HOOFDNAVIGATIE: 6 STAPPEN
    // ====================================

    function showStep(n) {
        step = Math.max(1, Math.min(6, n));

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

        $("#previous-step").hidden = step === 1;
        $("#previous-step").disabled = step === 1;
        $("#next-step").hidden = step === 6;
        $("#publish-event").hidden = step !== 6;

        $("#step-counter").textContent =
            `Stap ${step} van 6`;

        if (step === 5) {
            renderSummary();
        }

        // Kaart bijwerken zodra de nieuwe stap zichtbaar is.
        setTimeout(refreshMap, 50);
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

        setTimeout(refreshMap, 40);
    }

    $$("[data-extra]").forEach(button => {
        button.addEventListener("click", () => {
            showExtra(button.dataset.extra);
        });
    });

    const other = $("#other-access");

    function syncOther() {
        $("#other-access-wrap").hidden = !other.checked;
    }

    other.addEventListener("change", syncOther);
    syncOther();

    // ====================================
    // 3. DYNAMISCHE PERIODES
    // ====================================

    const periodFields = [
        { key: "Name", type: "text", label: "Naam periode" },
        { key: "StartDate", type: "date", label: "Begindatum" },
        { key: "StartTime", type: "time", label: "Begintijd" },
        { key: "EndDate", type: "date", label: "Einddatum" },
        { key: "EndTime", type: "time", label: "Eindtijd" }
    ];

    const periodList = $("#period-list");

    const validPeriodDate = value =>
        /^\d{4}-\d{2}-\d{2}$/.test(value);

    const validPeriodTime = value =>
        /^([01]\d|2[0-3]):[0-5]\d(?::[0-5]\d)?$/.test(value);

    function getExistingPeriodValues(row) {
        const inputs =
            $$("input:not([type='hidden'])", row);

        function namedValue(key) {
            const input = inputs.find(field =>
                field.dataset.periodField === key ||
                new RegExp(`\\.${key}$`).test(
                    field.name || ""
                )
            );

            return input?.value?.trim() || "";
        }

        const dateValues = inputs
            .map(input => input.value.trim())
            .filter(validPeriodDate);

        const timeValues = inputs
            .map(input => input.value.trim())
            .filter(validPeriodTime);

        let startDate = namedValue("StartDate");
        let endDate = namedValue("EndDate");
        let startTime = namedValue("StartTime");
        let endTime = namedValue("EndTime");

        if (!validPeriodDate(startDate)) startDate = "";
        if (!validPeriodDate(endDate)) endDate = "";
        if (!validPeriodTime(startTime)) startTime = "";
        if (!validPeriodTime(endTime)) endTime = "";

        const remainingDates = [...dateValues];
        const remainingTimes = [...timeValues];

        for (const value of [startDate, endDate]) {
            if (value) {
                const pos = remainingDates.indexOf(value);

                if (pos !== -1) {
                    remainingDates.splice(pos, 1);
                }
            }
        }

        for (const value of [startTime, endTime]) {
            if (value) {
                const pos = remainingTimes.indexOf(value);

                if (pos !== -1) {
                    remainingTimes.splice(pos, 1);
                }
            }
        }

        if (!startDate) {
            startDate = remainingDates.shift() || "";
        }

        if (!endDate) {
            endDate = remainingDates.shift() || "";
        }

        if (!startTime) {
            startTime = remainingTimes.shift() || "";
        }

        if (!endTime) {
            endTime = remainingTimes.shift() || "";
        }

        let name = namedValue("Name");

        if (
            !name ||
            /^Input\.Periods\[\d+\]\./.test(name)
        ) {
            name = inputs
                .map(input => input.value.trim())
                .find(value =>
                    value &&
                    !validPeriodDate(value) &&
                    !validPeriodTime(value) &&
                    !/^Input\.Periods\[\d+\]\./.test(value)
                ) || "";
        }

        return {
            Name: name,
            StartDate: startDate,
            StartTime: startTime,
            EndDate: endDate,
            EndTime: endTime
        };
    }

    function reindexPeriods() {
        $$(".period-row", periodList).forEach(
            (row, i) => {
                const values =
                    getExistingPeriodValues(row);

                const inputs =
                    $$("input:not([type='hidden'])", row);

                while (inputs.length < periodFields.length) {
                    const input =
                        document.createElement("input");

                    row.insertBefore(
                        input,
                        $(".remove-row", row)
                    );

                    inputs.push(input);
                }

                periodFields.forEach(
                    ({ key, type, label }, k) => {
                        const field = inputs[k];

                        field.type = type;
                        field.name =
                            `Input.Periods[${i}].${key}`;

                        field.id = `period-${i}-${key}`;
                        field.dataset.periodField = key;
                        field.setAttribute("aria-label", label);
                        field.value = values[key] || "";

                        if (key === "Name") {
                            field.placeholder = "Naam periode";
                            field.maxLength = 100;
                        } else {
                            field.removeAttribute("placeholder");
                        }
                    }
                );
            }
        );
    }

    $("#add-period").addEventListener("click", () => {
        periodList.insertAdjacentHTML(
            "beforeend",
            `
            <div class="period-row">
                <input type="text"
                       data-period-field="Name"
                       placeholder="Naam periode"
                       maxlength="100" />

                <input type="date"
                       data-period-field="StartDate" />

                <input type="time"
                       data-period-field="StartTime" />

                <input type="date"
                       data-period-field="EndDate" />

                <input type="time"
                       data-period-field="EndTime" />

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

    periodList.addEventListener("click", event => {
        const button =
            event.target.closest(".remove-row");

        if (!button) return;

        const rows = $$(".period-row", periodList);

        if (rows.length === 1) {
            rows[0].querySelectorAll("input")
                .forEach(input => {
                    input.value = "";
                });

            return;
        }

        button.closest(".period-row").remove();
        reindexPeriods();
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

            const button =
                document.createElement("button");

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

        add.class = ".location-add-tabs button"
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
        const container =
            document.getElementById("event-location-map");

        // Alleen uitvoeren als de kaart zichtbaar is.
        if (
            !window.L ||
            !container ||
            container.getClientRects().length === 0
        ) {
            return;
        }

        // Kaart maar één keer initialiseren.
        if (!map) {
            map = L.map(container).setView(
                [50.8882, 5.9795],
                13
            );

            L.tileLayer(
                "https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png",
                {
                    maxZoom: 19,
                    attribution: "&copy; OpenStreetMap"
                }
            ).addTo(map);

            map.on("click", event => {
                const { lat, lng } = event.latlng;

                setCoords(lat, lng);
                setPin(lat, lng);
            });
        }

        requestAnimationFrame(() => {
            if (
                container.getClientRects().length === 0
            ) {
                return;
            }

            map.invalidateSize();

            const row = currentLocation();

            if (!row) return;

            const latField = $(".location-lat", row);
            const lngField = $(".location-lng", row);

            const lat = coord(latField);
            const lng = coord(lngField);

            if (
                latField?.value &&
                lngField?.value &&
                Number.isFinite(lat) &&
                Number.isFinite(lng) &&
                lat >= -90 &&
                lat <= 90 &&
                lng >= -180 &&
                lng <= 180
            ) {
                setPin(lat, lng);
                map.setView([lat, lng], 15);
            } else {
                if (marker) {
                    map.removeLayer(marker);
                    marker = null;
                }

                $("#map-status").textContent =
                    "Klik op de kaart om een locatie te kiezen.";
            }
        });
    }

    // ====================================
    // PIN PLAATSEN EN VERSLEPEN
    // ====================================

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
                const position =
                    event.target.getLatLng();

                setCoords(position.lat, position.lng);
                setPin(position.lat, position.lng);
            });
        }

        $("#map-status").textContent =
            `Pin gekozen: ${lat.toFixed(5)}, ${lng.toFixed(5)}`;
    }

    // ====================================
    // ADRES ZOEKEN MET OPENSTREETMAP
    // ====================================

    const findLocationButton = $("#find-location");

    if (findLocationButton) {
        findLocationButton.addEventListener(
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
                        $(`[name$=".${key}"]`, row)?.value?.trim() || ""
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

                    if (
                        !Number.isFinite(lat) ||
                        !Number.isFinite(lng)
                    ) {
                        throw new Error("Ongeldige coördinaten");
                    }

                    setCoords(lat, lng);
                    refreshMap();

                } catch (error) {
                    $("#map-status").textContent =
                        "Adres niet gevonden. Kies zelf een pin.";

                    console.error(
                        "Locatie zoeken mislukt:",
                        error
                    );
                }
            }
        );
    }

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

        if (name === "gratis" && kind !== "Gratis") {
            $(".ticket-kind", row).value = "Gratis";
        }

        const realKind = $(".ticket-kind", row).value;

        const free =
            realKind === "Gratis" ||
            name === "gratis";

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
            value =>
                value.toLowerCase() === tag.toLowerCase()
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
                periodFields.map(({ key }) =>
                    $(
                        `[data-period-field="${key}"]`,
                        row
                    )?.value || ""
                ).join(" | ")
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

    // ====================================
    // 9. FORMULIER VERZENDEN
    // ====================================

    $("#next-step").addEventListener("click", () => {
        if (tagInput.value.trim()) {
            addTag();
        }
    });

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