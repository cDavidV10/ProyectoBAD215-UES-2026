// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

document.addEventListener("DOMContentLoaded", () => {
    document.querySelectorAll("[data-filter-input]").forEach((input) => {
        const key = input.dataset.filterInput;
        const table = document.querySelector(`[data-filter-table="${key}"]`);
        if (!table) return;

        input.addEventListener("input", () => {
            const query = input.value.trim().toLocaleLowerCase();
            table.querySelectorAll("tbody tr").forEach((row) => {
                const value = (row.dataset.filterValue || row.textContent || "").toLocaleLowerCase();
                row.hidden = query.length > 0 && !value.includes(query);
            });
        });
    });

    document.querySelectorAll("form[data-server-search]").forEach((form) => {
        const input = form.querySelector("input[name='busqueda']");
        if (!input) return;

        let timer;
        input.addEventListener("input", () => {
            window.clearTimeout(timer);
            timer = window.setTimeout(() => form.submit(), 700);
        });
    });

    document.querySelectorAll("[data-tab]").forEach((tab) => {
        tab.addEventListener("click", () => {
            const group = tab.closest("div");
            group?.querySelectorAll("[data-tab]").forEach((item) => item.classList.remove("active"));
            tab.classList.add("active");
        });
    });

    document.querySelectorAll("[data-close-check]").forEach((checkbox) => {
        const submit = document.querySelector("[data-close-submit]");
        if (!submit) return;
        checkbox.addEventListener("change", () => { submit.disabled = !checkbox.checked; });
    });

    document.querySelectorAll("[data-location-department]").forEach((departmentSelect) => {
        const form = departmentSelect.closest("form");
        const municipalitySelect = form?.querySelector("[data-location-municipality]");
        if (!municipalitySelect) return;

        const municipalityOptions = Array.from(municipalitySelect.querySelectorAll("option[data-department-id]"));

        const updateMunicipalities = () => {
            const departmentId = departmentSelect.value;
            const previousMunicipality = municipalitySelect.value;
            municipalitySelect.disabled = departmentId.length === 0;
            let municipalityIsAvailable = false;

            municipalityOptions.forEach((option) => {
                const visible = option.dataset.departmentId === departmentId;
                option.hidden = !visible;
                option.disabled = !visible;
                municipalityIsAvailable = municipalityIsAvailable || (visible && option.value === previousMunicipality);
            });

            municipalitySelect.value = municipalityIsAvailable ? previousMunicipality : "";
        };

        departmentSelect.addEventListener("change", updateMunicipalities);
        updateMunicipalities();
    });

    document.querySelectorAll("[data-contract-start]").forEach((startDate) => {
        const form = startDate.closest("form");
        const endDate = form?.querySelector("[data-contract-end]");
        if (!endDate) return;

        const validateContractDates = () => {
            endDate.min = startDate.value || "";
            endDate.setCustomValidity(
                startDate.value && endDate.value && endDate.value < startDate.value
                    ? "La fecha de culminación no puede ser anterior a la fecha de contratación."
                    : ""
            );
        };

        startDate.addEventListener("change", validateContractDates);
        endDate.addEventListener("change", validateContractDates);
        validateContractDates();
    });

    document.querySelectorAll("form[data-employee-form]").forEach((form) => {
        const fields = Array.from(form.querySelectorAll("input:not([type='hidden']), select, textarea"));
        const clientMessage = form.querySelector("[data-client-validation-message]");

        const mensajeDeValidacion = (field) => {
            if (field.validity.valueMissing) {
                return field.tagName === "SELECT" ? "Seleccione una opción." : "Este campo es obligatorio.";
            }
            if (field.validity.typeMismatch) return "Ingrese un correo electrónico válido.";
            if (field.validity.patternMismatch) return field.title || "El formato ingresado no es válido.";
            if (field.validity.rangeUnderflow) return "El valor ingresado es menor al mínimo permitido.";
            if (field.validity.rangeOverflow) return "El valor ingresado supera el máximo permitido.";
            if (field.validity.badInput) return "Ingrese un valor válido.";
            if (field.validity.tooLong) return "El texto ingresado supera la longitud permitida.";
            return "Revise el valor ingresado.";
        };

        const updateClientMessage = () => {
            if (!clientMessage) return;
            clientMessage.hidden = !fields.some((field) => !field.checkValidity());
        };

        const markField = (field) => {
            const conservaValidacionDeFecha = field.matches("[data-contract-end]");
            if (!conservaValidacionDeFecha) field.setCustomValidity("");
            const invalid = !field.checkValidity();
            if (invalid && !conservaValidacionDeFecha) {
                field.setCustomValidity(mensajeDeValidacion(field));
            }
            field.classList.toggle("bemo-input-error", invalid);
            field.setAttribute("aria-invalid", invalid ? "true" : "false");
            updateClientMessage();
        };

        fields.forEach((field) => {
            const hasServerError = field.classList.contains("bemo-input-error");
            field.setAttribute("aria-invalid", hasServerError ? "true" : "false");
            field.addEventListener("blur", () => markField(field));
            field.addEventListener("input", () => markField(field));
            field.addEventListener("change", () => markField(field));
        });

        form.addEventListener("invalid", (event) => {
            if (event.target instanceof HTMLElement) {
                const field = event.target;
                if (!field.matches("[data-contract-end]")) {
                    field.setCustomValidity(mensajeDeValidacion(field));
                }
                event.target.classList.add("bemo-input-error");
                event.target.setAttribute("aria-invalid", "true");
            }
            if (clientMessage) clientMessage.hidden = false;
        }, true);

        form.addEventListener("submit", () => {
            fields.forEach(markField);
            updateClientMessage();
        });
    });

    document.querySelectorAll("form[data-demo-form]").forEach((form) => {
        form.addEventListener("submit", (event) => {
            if (!form.checkValidity()) return;
            event.preventDefault();
            const message = document.createElement("div");
            message.className = "bemo-info mt-4";
            message.innerHTML = "<span>La pantalla está lista; la persistencia se conectará al flujo de datos.</span>";
            form.appendChild(message);
        });
    });
});
