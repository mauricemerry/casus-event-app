document.addEventListener("DOMContentLoaded", () => {
    const cancelForm = document.getElementById("cancel-event-form");
    const cancelModal = document.getElementById("cancel-confirm-modal");
    const cancelNo = document.getElementById("cancel-confirm-no");
    const cancelYes = document.getElementById("cancel-confirm-yes");
    let cancelRequested = false;

    cancelForm?.addEventListener("submit", event => {
        if (!cancelRequested) {
            event.preventDefault();
            if (cancelModal) cancelModal.hidden = false;
            cancelNo?.focus();
        }
        cancelRequested = false;
    });

    cancelNo?.addEventListener("click", () => {
        if (cancelModal) cancelModal.hidden = true;
    });

    cancelYes?.addEventListener("click", () => {
        cancelRequested = true;
        if (cancelModal) cancelModal.hidden = true;
        cancelForm?.requestSubmit();
    });

    cancelModal?.addEventListener("click", event => {
        if (event.target === cancelModal) cancelModal.hidden = true;
    });

    document.addEventListener("keydown", event => {
        if (event.key === "Escape" && cancelModal && !cancelModal.hidden) {
            cancelModal.hidden = true;
        }
    });

    const form = document.getElementById("edit-wizard-form");
    if (!form) return;

    const steps = [...form.querySelectorAll("[data-edit-step]")];
    const buttons = [...document.querySelectorAll("[data-edit-step-button]")];
    const summary = document.getElementById("edit-summary-content");
    const errors = document.getElementById("edit-wizard-summary");
    const previous = document.getElementById("edit-previous-step");
    const next = document.getElementById("edit-next-step");
    const save = document.getElementById("edit-save-event");
    const counter = document.getElementById("edit-step-counter");
    let currentStep = 1;

    const tagList = document.getElementById("edit-tags-list");
    const tagInput = document.getElementById("edit-tag-text");
    function reindexTags() {
        tagList?.querySelectorAll('input[name^="Input.Tags["]').forEach((input, index) => {
            input.name = `Input.Tags[${index}]`;
        });
    }
    tagInput?.addEventListener("keydown", event => {
        if (event.key !== "Enter") return;
        event.preventDefault();
        const value = tagInput.value.trim();
        if (!value || !tagList) return;
        const pill = document.createElement("span");
        pill.className = "tag-pill";
        pill.innerHTML = `${value} <button type="button" class="tag-remove" aria-label="Tag verwijderen">×</button>`;
        const hidden = document.createElement("input");
        hidden.type = "hidden";
        hidden.name = "Input.Tags[]";
        hidden.value = value;
        pill.appendChild(hidden);
        tagList.appendChild(pill);
        tagInput.value = "";
        reindexTags();

    });
    tagList?.addEventListener("click", event => {
        if (event.target.closest(".tag-remove")) {
            event.target.closest(".tag-pill")?.remove();
            reindexTags();
        }
    });
    reindexTags();

    const otherAccess = document.getElementById("edit-other-access");
    const otherAccessWrap = document.getElementById("edit-other-access-wrap");
    otherAccess?.addEventListener("change", () => {
        if (otherAccessWrap) otherAccessWrap.hidden = !otherAccess.checked;
    });

    function reindexPeriods() {
        form.querySelectorAll("#period-list .period-row").forEach((row, index) => {
            row.querySelectorAll("input").forEach(input => {
                const field = input.dataset.periodField;
                if (field) input.name = `Input.Periods[${index}].${field}`;
            });
        });
    }

    document.getElementById("edit-add-period")?.addEventListener("click", () => {
        const list = document.getElementById("period-list");
        const row = document.createElement("div");
        row.className = "period-row";
        row.innerHTML = `<input type="text" data-period-field="Name" maxlength="100" placeholder="Naam periode" /><input type="date" data-period-field="StartDate" /><input type="time" data-period-field="StartTime" /><input type="date" data-period-field="EndDate" /><input type="time" data-period-field="EndTime" /><button type="button" class="remove-period-btn">Verwijderen</button>`;
        list.appendChild(row);
        reindexPeriods();
        row.querySelector("input")?.focus();
    });

    document.getElementById("period-list")?.addEventListener("click", event => {
        if (!event.target.closest(".remove-period-btn")) return;
        const rows = [...document.querySelectorAll("#period-list .period-row")];
        if (rows.length === 1) rows[0].querySelectorAll("input").forEach(input => input.value = "");
        else event.target.closest(".period-row")?.remove();
        reindexPeriods();
    });
    reindexPeriods();

    function reindexLocations() {
        form.querySelectorAll("#location-list .location-row").forEach((row, index) => {
            row.dataset.locationPanel = String(index);
            row.querySelectorAll("input").forEach(input => {
                const match = input.name.match(/\.([A-Za-z]+)$/);
                if (match) input.name = `Input.Locations[${index}].${match[1]}`;
            });
        });
        document.querySelectorAll("#location-tabs [data-location-tab]").forEach((tab, index) => {
            tab.dataset.locationTab = String(index);
            tab.textContent = `Locatie ${index + 1}`;
        });
    }

    document.getElementById("edit-add-location")?.addEventListener("click", () => {
        const list = document.getElementById("location-list");
        const index = list.querySelectorAll(".location-row").length;
        const tabs = document.getElementById("location-tabs");
        const tab = document.createElement("button");
        tab.type = "button";
        tab.role = "tab";
        tab.dataset.locationTab = String(index);
        tab.textContent = `Locatie ${index + 1}`;
        tab.setAttribute("aria-selected", "false");
        tabs.insertBefore(tab, document.getElementById("edit-add-location"));
        const row = document.createElement("div");
        row.className = "event-panel location-row";
        row.dataset.locationPanel = String(index);
        row.hidden = true;
        row.innerHTML = `<div class="portal-form-group"><label>Locatienaam</label><input name="Input.Locations[${index}].Name" placeholder="Hoofdingang" /></div><div class="portal-form-group"><label>Plaats</label><input name="Input.Locations[${index}].City" placeholder="Plaats" /></div><div class="portal-form-group"><label>Straat</label><input name="Input.Locations[${index}].Street" placeholder="Straat" /></div><div class="event-grid"><div class="portal-form-group"><label>Nummer</label><input name="Input.Locations[${index}].Number" placeholder="Huisnummer" /></div><div class="portal-form-group"><label>Toevoeging</label><input name="Input.Locations[${index}].Addition" placeholder="Optioneel" /></div></div><div class="portal-form-group"><label>Postcode</label><input name="Input.Locations[${index}].PostalCode" placeholder="Postcode" /></div><input type="hidden" class="location-lat" name="Input.Locations[${index}].Latitude" /><input type="hidden" class="location-lng" name="Input.Locations[${index}].Longitude" /><button type="button" class="remove-location portal-secondary-link">Locatie verwijderen</button>`;
        list.appendChild(row);
        reindexLocations();
        selectLocationTab(tab);
    });

    document.getElementById("location-list")?.addEventListener("click", event => {
        if (!event.target.closest(".remove-location")) return;
        const rows = [...document.querySelectorAll("#location-list .location-row")];
        if (rows.length > 1) {
            const row = event.target.closest(".location-row");
            const index = rows.indexOf(row);
            row?.remove();
            document.querySelector(`#location-tabs [data-location-tab="${index}"]`)?.remove();
        }
        reindexLocations();
        const firstTab = document.querySelector("#location-tabs [data-location-tab]");
        if (firstTab) selectLocationTab(firstTab);
    });
    reindexLocations();

    function selectLocationTab(tab) {
        const selected = tab.dataset.locationTab;
        document.querySelectorAll("[data-location-tab]").forEach(item => {
            const active = item === tab;
            item.classList.toggle("active", active);
            item.setAttribute("aria-selected", active ? "true" : "false");
        });
        document.querySelectorAll("[data-location-panel]").forEach(panel => {
            panel.hidden = panel.dataset.locationPanel !== selected;
        });
    }

    document.getElementById("location-tabs")?.addEventListener("click", event => {
        const tab = event.target.closest("[data-location-tab]");
        if (tab) selectLocationTab(tab);
    });

    const field = name => form.querySelector(`[name="${name}"]`);

    function reindexTickets() {
        form.querySelectorAll("#ticket-list .ticket-row").forEach((row, index) => {
            row.querySelector(".ticket-name").name = `Input.Tickets[${index}].Name`;
            row.querySelector(".ticket-price").name = `Input.Tickets[${index}].Price`;
            row.querySelector(".ticket-kind").name = `Input.Tickets[${index}].TypeName`;
        });
    }

    document.getElementById("add-ticket")?.addEventListener("click", () => {
        const list = document.getElementById("ticket-list");
        const row = document.createElement("div");
        row.className = "ticket-row";
        row.innerHTML = `
            <input class="ticket-name" placeholder="Naam" aria-label="Ticketnaam" />
            <div class="ticket-price-wrap"><input class="ticket-price" type="number" step="0.01" min="0" placeholder="Prijs (€)" aria-label="Ticketprijs" /></div>
            <select class="ticket-kind" aria-label="Tickettype"><option>Gratis</option><option>Dagticket</option><option>Periode</option><option>Tijdslot</option></select>
            <button type="button" class="remove-ticket remove-row" aria-label="Ticket verwijderen">✕</button>`;
        list.appendChild(row);
        reindexTickets();
    });

    function syncDateRange() {
        const value = field => form.querySelector(`[data-period-field="${field}"]`)?.value || "";
        const start = form.querySelector("#edit-start-datetime");
        const end = form.querySelector("#edit-end-datetime");
        if (start && value("StartDate") && value("StartTime")) {
            start.value = `${value("StartDate")}T${value("StartTime")}`;
        }
        if (end && value("EndDate") && value("EndTime")) {
            end.value = `${value("EndDate")}T${value("EndTime")}`;
        }
    }

    form.querySelectorAll("[data-period-field]").forEach(input => {
        input.addEventListener("change", syncDateRange);
    });

    document.getElementById("ticket-list")?.addEventListener("click", event => {
        if (!event.target.closest(".remove-ticket")) return;
        event.target.closest(".ticket-row")?.remove();
        reindexTickets();
    });

    function clearErrors() {
        errors.hidden = true;
        errors.replaceChildren();
        form.querySelectorAll(".wizard-invalid, .edit-field-error").forEach(element => {
            element.classList.remove("wizard-invalid", "edit-field-error");
            if (element.matches("[aria-invalid]")) element.removeAttribute("aria-invalid");
        });
    }

    function invalid(input, message, messages) {
        if (input) {
            input.classList.add("wizard-invalid");
            input.setAttribute("aria-invalid", "true");
            messages.push(message);
        }
    }

    function validateStep() {
        clearErrors();
        const messages = [];

        if (currentStep === 1) {
            const name = field("Input.EventName");
            const description = field("Input.Description");
            const environments = form.querySelectorAll('input[name^="Input.Is"]:checked');
            if (!name?.value.trim()) invalid(name, "Vul een naam voor het evenement in.", messages);
            if (!description?.value.trim()) invalid(description, "Vul een beschrijving in.", messages);
            if (!environments.length) messages.push("Kies minimaal één omgeving.");
        }

        if (currentStep === 3 && (!field("Input.Latitude")?.value || !field("Input.Longitude")?.value)) {
            messages.push("Kies een locatie op de kaart.");
        }

        if (currentStep === 4) {
            const start = field("Input.StartDateTime");
            const end = field("Input.EndDateTime");
            if (start?.value && end?.value && new Date(start.value) > new Date(end.value)) {
                invalid(start, "Het startmoment moet voor het eindmoment liggen.", messages);
                invalid(end, "Het eindmoment moet na het startmoment liggen.", messages);
            }
        }

        if (messages.length) {
            errors.innerHTML = `<ul>${[...new Set(messages)].map(message => `<li>${message}</li>`).join("")}</ul>`;
            errors.hidden = false;
            return false;
        }

        return true;
    }

    function addSummaryLine(label, value) {
        if (!value) return;
        const paragraph = document.createElement("p");
        paragraph.textContent = `${label}: ${value}`;
        summary.appendChild(paragraph);
    }

    function renderSummary() {
        summary.replaceChildren();
        addSummaryLine("Naam", field("Input.EventName")?.value);
        addSummaryLine("Type", field("Input.EventType")?.value);
        addSummaryLine("Korte omschrijving", field("Input.ShortDescription")?.value);
        addSummaryLine("Beschrijving", field("Input.Description")?.value);
        addSummaryLine("Notities", field("Input.Notes")?.value);
        addSummaryLine("Categorie", field("Input.CategoryName")?.value);
        addSummaryLine("Start", field("Input.StartDateTime")?.value);
        addSummaryLine("Einde", field("Input.EndDateTime")?.value);
        addSummaryLine("Adres", field("Input.Address")?.value);
        addSummaryLine("Omgeving", [...form.querySelectorAll('input[name^="Input.Is"]:checked')].map(input => input.parentElement.textContent.trim()).join(", "));
        addSummaryLine("Tickets", [...form.querySelectorAll('input[name^="Input.Tickets["][name$="].Name"]')].map(input => input.value).filter(Boolean).join(", "));
        addSummaryLine("Tags", [...form.querySelectorAll('input[name^="Input.Tags"]')].map(input => input.value).filter(Boolean).join(", "));
    }

    function showStep(step) {
        currentStep = Math.max(1, Math.min(6, step));
        steps.forEach(element => {
            element.hidden = Number(element.dataset.editStep) !== currentStep;
        });
        buttons.forEach(button => {
            const active = Number(button.dataset.editStepButton) === currentStep;
            button.classList.toggle("active", active);
            if (active) button.setAttribute("aria-current", "step");
            else button.removeAttribute("aria-current");
        });
        previous.hidden = currentStep === 1;
        next.hidden = currentStep === 6;
        save.hidden = currentStep !== 6;
        counter.textContent = `Stap ${currentStep} van 6`;
        if (currentStep === 6) renderSummary();
        if (currentStep === 3) {
            window.dispatchEvent(new Event("edit-location-step-visible"));
        }
    }

    buttons.forEach(button => button.addEventListener("click", () => {
        const target = Number(button.dataset.editStepButton);
        showStep(target);
    }));

    previous.addEventListener("click", () => showStep(currentStep - 1));
    next.addEventListener("click", () => {
        if (validateStep()) showStep(currentStep + 1);
    });

    form.addEventListener("submit", event => {
        if (currentStep !== 6) {
            event.preventDefault();
            if (validateStep()) showStep(currentStep + 1);
        }
    });

    showStep(1);
});
