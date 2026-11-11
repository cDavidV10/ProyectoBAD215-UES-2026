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
