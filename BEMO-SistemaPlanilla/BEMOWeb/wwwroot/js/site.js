// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

document.addEventListener("DOMContentLoaded", () => {
    document.querySelectorAll("[data-password-toggle]").forEach((button) => {
        const targetId = button.dataset.passwordTarget;
        const input = targetId ? document.getElementById(targetId) : null;
        if (!input) return;

        button.addEventListener("click", () => {
            const mostrar = input.type === "password";
            input.type = mostrar ? "text" : "password";
            const icon = button.querySelector("[data-password-icon]");
            const label = mostrar ? "Ocultar contraseña" : "Mostrar contraseña";
            icon?.querySelector("use")?.setAttribute("href", mostrar ? "#icon-eye-closed" : "#icon-eye");
            if (icon) icon.setAttribute("aria-label", label);
            const labelElement = button.querySelector("[data-password-label]");
            if (labelElement) labelElement.textContent = label;
            button.setAttribute("aria-label", label);
            button.setAttribute("title", label);
            button.setAttribute("aria-pressed", mostrar ? "true" : "false");
        });
    });

    document.querySelectorAll("form[data-user-form]").forEach((form) => {
        const password = form.querySelector("[data-password-field]:not([data-password-confirmation])");
        const confirmation = form.querySelector("[data-password-confirmation]");
        if (!password || !confirmation) return;

        const updatePasswordMatch = () => {
            const hasValues = password.value.length > 0 && confirmation.value.length > 0;
            const matches = hasValues && password.value === confirmation.value;

            confirmation.classList.toggle("bemo-input-success", matches);
            confirmation.classList.toggle("bemo-input-error", hasValues && !matches);
            confirmation.setCustomValidity(hasValues && !matches ? "Las contraseñas no coinciden." : "");
        };

        password.addEventListener("input", updatePasswordMatch);
        confirmation.addEventListener("input", updatePasswordMatch);
        updatePasswordMatch();
    });

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

    document.querySelectorAll("form[data-auto-submit]").forEach((form) => {
        form.querySelectorAll("input[type='date'], select").forEach((field) => {
            field.addEventListener("change", () => form.submit());
        });
    });

    document.querySelectorAll("[data-notification]").forEach((notification) => {
        const closeButton = notification.querySelector("[data-notification-close]");
        const countdownElement = notification.querySelector("[data-notification-countdown]");
        const progressElement = notification.querySelector("[data-notification-progress]");
        const seconds = Math.max(Number(notification.dataset.notificationSeconds) || 5, 1);
        let remaining = seconds;
        let timer;
        let closed = false;

        const closeNotification = () => {
            if (closed) return;
            closed = true;
            window.clearInterval(timer);
            notification.classList.add("is-closing");
            window.setTimeout(() => notification.remove(), 220);
        };

        const updateCountdown = () => {
            if (countdownElement) {
                countdownElement.textContent = `Se cerrará en ${remaining} ${remaining === 1 ? "segundo" : "segundos"}`;
            }

            if (progressElement) {
                progressElement.style.setProperty("--notification-progress", `${(remaining / seconds) * 100}%`);
            }
        };

        updateCountdown();
        timer = window.setInterval(() => {
            remaining -= 1;
            if (remaining <= 0) {
                closeNotification();
                return;
            }
            updateCountdown();
        }, 1000);

        closeButton?.addEventListener("click", closeNotification);
    });

    const confirmationModal = document.querySelector("[data-confirm-modal]");
    if (confirmationModal) {
        const messageElement = confirmationModal.querySelector("[data-confirm-message]");
        const acceptButton = confirmationModal.querySelector("[data-confirm-accept]");
        const titleElement = confirmationModal.querySelector("#confirm-user-title");
        const cancelButtons = confirmationModal.querySelectorAll("[data-confirm-cancel]");
        let pendingForm = null;

        const closeConfirmationModal = () => {
            confirmationModal.classList.add("hidden");
            confirmationModal.setAttribute("aria-hidden", "true");
            pendingForm = null;
        };

        document.querySelectorAll("form[data-confirm]").forEach((form) => {
            form.addEventListener("submit", (event) => {
                if (form.dataset.confirmed === "true") {
                    delete form.dataset.confirmed;
                    return;
                }

                event.preventDefault();
                pendingForm = form;
                if (messageElement) {
                    messageElement.textContent = form.dataset.confirm || "¿Deseas continuar con esta operación?";
                }
                if (titleElement) {
                    titleElement.textContent = form.dataset.confirmTitle || "Confirmar desactivacion";
                }
                if (acceptButton) {
                    acceptButton.textContent = form.dataset.confirmAcceptText || "Desactivar usuario";
                }
                confirmationModal.classList.remove("hidden");
                confirmationModal.setAttribute("aria-hidden", "false");
                acceptButton?.focus();
            });
        });

        acceptButton?.addEventListener("click", () => {
            const form = pendingForm;
            closeConfirmationModal();
            if (form) {
                form.dataset.confirmed = "true";
                form.submit();
            }
        });

        cancelButtons.forEach((button) => button.addEventListener("click", closeConfirmationModal));
        confirmationModal.addEventListener("click", (event) => {
            if (event.target === confirmationModal) closeConfirmationModal();
        });
        document.addEventListener("keydown", (event) => {
            if (event.key === "Escape" && !confirmationModal.classList.contains("hidden")) {
                closeConfirmationModal();
            }
        });
    }

    const userCreatedModal = document.querySelector("[data-user-created-modal]");
    if (userCreatedModal) {
        const closeButtons = userCreatedModal.querySelectorAll("[data-user-created-close]");

        const closeUserCreatedModal = () => {
            userCreatedModal.classList.add("hidden");
            userCreatedModal.setAttribute("aria-hidden", "true");
        };

        closeButtons.forEach((button) => button.addEventListener("click", closeUserCreatedModal));
        userCreatedModal.addEventListener("click", (event) => {
            if (event.target === userCreatedModal) closeUserCreatedModal();
        });
        document.addEventListener("keydown", (event) => {
            if (event.key === "Escape" && !userCreatedModal.classList.contains("hidden")) {
                closeUserCreatedModal();
            }
        });

        userCreatedModal.querySelector("[data-user-created-close]")?.focus();
    }

    const userPasswordModal = document.querySelector("[data-user-password-modal]");
    if (userPasswordModal) {
        const closeButtons = userPasswordModal.querySelectorAll("[data-user-password-close]");
        const form = userPasswordModal.querySelector("[data-user-password-form]");
        const idInput = userPasswordModal.querySelector("[data-user-password-id]");
        const nameElement = userPasswordModal.querySelector("[data-user-password-name]");
        const passwordInput = userPasswordModal.querySelector("#UsuarioPasswordNueva");
        const confirmationInput = userPasswordModal.querySelector("#UsuarioPasswordConfirmar");

        const clearPasswordState = () => {
            [passwordInput, confirmationInput].forEach((input) => {
                if (!input) return;
                input.value = "";
                input.setCustomValidity("");
                input.classList.remove("bemo-input-error", "bemo-input-success");
            });
        };

        const closeUserPasswordModal = () => {
            userPasswordModal.classList.add("hidden");
            userPasswordModal.setAttribute("aria-hidden", "true");
            clearPasswordState();
        };

        document.querySelectorAll("[data-user-password-edit]").forEach((button) => {
            button.addEventListener("click", () => {
                if (idInput) idInput.value = button.dataset.userId || "";
                if (nameElement) nameElement.textContent = button.dataset.userName || "Usuario seleccionado";
                clearPasswordState();
                userPasswordModal.classList.remove("hidden");
                userPasswordModal.setAttribute("aria-hidden", "false");
                passwordInput?.focus();
            });
        });

        closeButtons.forEach((button) => button.addEventListener("click", closeUserPasswordModal));
        userPasswordModal.addEventListener("click", (event) => {
            if (event.target === userPasswordModal) closeUserPasswordModal();
        });
        document.addEventListener("keydown", (event) => {
            if (event.key === "Escape" && !userPasswordModal.classList.contains("hidden")) {
                closeUserPasswordModal();
            }
        });

        form?.addEventListener("submit", (event) => {
            if (!form.checkValidity()) event.preventDefault();
        });
    }

    const employeeModal = document.querySelector("[data-employee-modal]");
    if (employeeModal) {
        const closeButton = employeeModal.querySelector("[data-employee-modal-close]");

        const closeEmployeeModal = () => {
            employeeModal.classList.add("hidden");
            employeeModal.setAttribute("aria-hidden", "true");
        };

        document.querySelectorAll("[data-employee-view]").forEach((button) => {
            button.addEventListener("click", () => {
                employeeModal.querySelectorAll("[data-employee-field]").forEach((field) => {
                    const fieldName = field.dataset.employeeField || "";
                    const dataName = fieldName
                        .split("-")
                        .map((part) => part.charAt(0).toUpperCase() + part.slice(1))
                        .join("");
                    const value = button.dataset[`employee${dataName}`];
                    field.textContent = value || "No registrado";
                });

                employeeModal.classList.remove("hidden");
                employeeModal.setAttribute("aria-hidden", "false");
                closeButton?.focus();
            });
        });

        closeButton?.addEventListener("click", closeEmployeeModal);
        employeeModal.addEventListener("click", (event) => {
            if (event.target === employeeModal) closeEmployeeModal();
        });
        document.addEventListener("keydown", (event) => {
            if (event.key === "Escape" && !employeeModal.classList.contains("hidden")) {
                closeEmployeeModal();
            }
        });
    }

    const contractModal = document.querySelector("[data-contract-modal]");
    if (contractModal) {
        const closeButton = contractModal.querySelector("[data-contract-modal-close]");

        const closeContractModal = () => {
            contractModal.classList.add("hidden");
            contractModal.setAttribute("aria-hidden", "true");
        };

        document.querySelectorAll("[data-contract-view]").forEach((button) => {
            button.addEventListener("click", () => {
                contractModal.querySelectorAll("[data-contract-field]").forEach((field) => {
                    const fieldName = field.dataset.contractField || "";
                    const dataName = fieldName
                        .split("-")
                        .map((part) => part.charAt(0).toUpperCase() + part.slice(1))
                        .join("");
                    const value = button.dataset[`contract${dataName}`];
                    field.textContent = value || "No registrado";
                });

                contractModal.classList.remove("hidden");
                contractModal.setAttribute("aria-hidden", "false");
                closeButton?.focus();
            });
        });

        closeButton?.addEventListener("click", closeContractModal);
        contractModal.addEventListener("click", (event) => {
            if (event.target === contractModal) closeContractModal();
        });
        document.addEventListener("keydown", (event) => {
            if (event.key === "Escape" && !contractModal.classList.contains("hidden")) {
                closeContractModal();
            }
        });
    }

    const auditModal = document.querySelector("[data-audit-modal]");
    if (auditModal) {
        const closeButton = auditModal.querySelector("[data-audit-modal-close]");

        const closeAuditModal = () => {
            auditModal.classList.add("hidden");
            auditModal.setAttribute("aria-hidden", "true");
        };

        document.querySelectorAll("[data-audit-view]").forEach((button) => {
            button.addEventListener("click", () => {
                auditModal.querySelectorAll("[data-audit-field]").forEach((field) => {
                    const fieldName = field.dataset.auditField || "";
                    const dataName = fieldName
                        .split("-")
                        .map((part) => part.charAt(0).toUpperCase() + part.slice(1))
                        .join("");
                    const value = button.dataset[`audit${dataName}`];
                    field.textContent = value || "No registrado";
                });

                auditModal.classList.remove("hidden");
                auditModal.setAttribute("aria-hidden", "false");
                closeButton?.focus();
            });
        });

        closeButton?.addEventListener("click", closeAuditModal);
        auditModal.addEventListener("click", (event) => {
            if (event.target === auditModal) closeAuditModal();
        });
        document.addEventListener("keydown", (event) => {
            if (event.key === "Escape" && !auditModal.classList.contains("hidden")) {
                closeAuditModal();
            }
        });
    }

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

    document.querySelectorAll("[data-contract-type]").forEach((typeSelect) => {
        const form = typeSelect.closest("form");
        const endDate = form?.querySelector("[data-contract-end]");
        if (!endDate) return;

        const updateEndDateState = () => {
            const tipo = typeSelect.selectedOptions[0]?.textContent.trim().toLocaleUpperCase() || "";
            const esPermanente = tipo === "PERMANENTE";

            endDate.disabled = esPermanente;
            endDate.required = !esPermanente && typeSelect.value.length > 0;

            if (esPermanente) {
                endDate.value = "";
                endDate.setCustomValidity("");
                endDate.classList.remove("bemo-input-error");
                endDate.setAttribute("aria-invalid", "false");
            }
        };

        typeSelect.addEventListener("change", updateEndDateState);
        updateEndDateState();
    });

    const employeeLookup = document.querySelector("[data-employee-lookup]");
    if (employeeLookup) {
        const searchInput = employeeLookup.querySelector("[data-employee-search]");
        const employeeIdInput = employeeLookup.querySelector("#id-empleado");
        const employeeOptions = employeeLookup.querySelectorAll("[data-employee-option]");
        const result = employeeLookup.querySelector("[data-employee-result]");
        const message = employeeLookup.querySelector("[data-employee-search-message]");
        const codeElement = result?.querySelector("[data-employee-code]");
        const nameElement = result?.querySelector("[data-employee-name]");
        const statusElement = result?.querySelector("[data-employee-status]");
        const options = Array.from(employeeOptions).map((option) => ({
            dui: option.dataset.employeeDui?.trim() || "",
            id: option.dataset.employeeId || "",
            name: option.dataset.employeeName || ""
        }));

        const updateEmployee = () => {
            if (!searchInput || !employeeIdInput) return;

            const dui = searchInput.value.trim();
            const employee = options.find((option) => option.dui === dui);
            if (employee) {
                employeeIdInput.value = employee.id;
                result?.classList.remove("hidden");
                if (codeElement) codeElement.textContent = employee.id;
                if (nameElement) nameElement.textContent = employee.name;
                if (statusElement) statusElement.textContent = "ACTIVO";
                if (message) message.textContent = "";
                searchInput.setCustomValidity("");
                return;
            }

            employeeIdInput.value = "";
            result?.classList.add("hidden");
            if (message) {
                message.textContent = dui.length > 0
                    ? "No se encontró un empleado activo sin contrato vigente con ese DUI."
                    : "";
            }
            searchInput.setCustomValidity(dui.length > 0
                ? "El DUI no corresponde a un empleado activo sin contrato vigente."
                : "");
        };

        const employeeFromModel = options.find((option) => option.id === employeeIdInput?.value);
        if (searchInput && employeeFromModel && !searchInput.value) {
            searchInput.value = employeeFromModel.dui;
        }

        searchInput?.addEventListener("input", updateEmployee);
        searchInput?.addEventListener("change", updateEmployee);
        updateEmployee();
    }

    document.querySelectorAll("[data-contract-start]").forEach((startDate) => {
        const form = startDate.closest("form");
        const endDate = form?.querySelector("[data-contract-end]");
        if (!endDate) return;

        const validateContractDates = () => {
            const formatDate = (date) => [
                date.getFullYear(),
                String(date.getMonth() + 1).padStart(2, "0"),
                String(date.getDate()).padStart(2, "0")
            ].join("-");
            const tomorrow = new Date();
            tomorrow.setHours(0, 0, 0, 0);
            tomorrow.setDate(tomorrow.getDate() + 1);
            const fechaMinima = formatDate(tomorrow);
            const fechaMinimaContrato = startDate.value && startDate.value > fechaMinima
                ? startDate.value
                : fechaMinima;

            endDate.min = fechaMinimaContrato;

            let mensaje = "";
            if (endDate.value && endDate.value < fechaMinima) {
                mensaje = "La fecha de culminación debe ser posterior a la fecha actual.";
            } else if (startDate.value && endDate.value && endDate.value < startDate.value) {
                mensaje = "La fecha de culminación no puede ser anterior a la fecha de contratación.";
            }

            endDate.setCustomValidity(mensaje);
        };

        startDate.addEventListener("change", validateContractDates);
        endDate.addEventListener("change", validateContractDates);
        validateContractDates();
    });

    document.querySelectorAll("form[data-employee-form], form[data-contract-form]").forEach((form) => {
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
            const conservaValidacionPersonalizada = field.matches("[data-contract-end], [data-employee-search]");
            if (!conservaValidacionPersonalizada) field.setCustomValidity("");
            const invalid = !field.checkValidity();
            if (invalid && !conservaValidacionPersonalizada) {
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
                if (!field.matches("[data-contract-end], [data-employee-search]")) {
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
