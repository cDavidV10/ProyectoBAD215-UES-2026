using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using BEMO.Negocio;
using Negocio;

namespace BEMOWeb.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "ADMINISTRADOR NOMINA")]
public class ContratosController : Controller
{
    private readonly CN_Contrato _contratos;
    private readonly CN_Empleado _empleados;
    private readonly CN_TipoContrato _tiposContrato;
    private readonly CN_Cargo _cargos;
    private readonly CN_Departamento _departamento;

    public ContratosController(
        CN_Contrato contratos,
        CN_Empleado empleados,
        CN_TipoContrato tiposContrato,
        CN_Cargo cargos,
        CN_Departamento departamento)
    {
        _contratos = contratos;
        _empleados = empleados;
        _tiposContrato = tiposContrato;
        _cargos = cargos;
        _departamento = departamento;
    }

    public IActionResult Index(
        int pagina = 1,
        string? busqueda = null,
        int? empleado = null,
        string? tipo = null,
        string? estado = null)
    {
        const int tamanoPagina = 5;
        pagina = Math.Max(1, pagina);
        var todosLosContratos = _contratos.ListarVigentes();
        var resultado = _contratos.ListarPaginado(
            pagina,
            tamanoPagina,
            busqueda,
            empleado,
            tipo,
            estado);
        var totalRegistros = resultado.Total;
        var totalPaginas = Math.Max(1, (int)Math.Ceiling(totalRegistros / (double)tamanoPagina));

        if (pagina > totalPaginas)
        {
            pagina = totalPaginas;
            resultado = _contratos.ListarPaginado(
                pagina,
                tamanoPagina,
                busqueda,
                empleado,
                tipo,
                estado);
        }

        var modelo = new Models.ContratosModel
        {
            ContratosVigentes = todosLosContratos.Count,
            ContratosPorVencer = todosLosContratos.Count(contrato => contrato.Situacion == "POR VENCER"),
            Contratos = resultado.Contratos,
            Empleados = todosLosContratos
                .GroupBy(contrato => new { contrato.IdEmpleado, contrato.NombreEmpleado })
                .OrderBy(grupo => grupo.Key.NombreEmpleado)
                .Select(grupo => new Models.ContratoEmpleadoOption
                {
                    IdEmpleado = grupo.Key.IdEmpleado,
                    Nombre = grupo.Key.NombreEmpleado
                })
                .ToList(),
            TiposContrato = _tiposContrato.Listar()
                .Select(tipoContrato => tipoContrato.NombreTipoContrato)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(tipoContrato => tipoContrato)
                .ToList(),
            EmpleadoSeleccionado = empleado,
            TipoSeleccionado = tipo,
            EstadoSeleccionado = estado,
            Busqueda = busqueda,
            PaginaActual = pagina,
            TotalPaginas = totalPaginas,
            TotalRegistros = totalRegistros,
            TamanoPagina = tamanoPagina
        };

        return View(modelo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Finalizar(int id)
    {
        try
        {
            _contratos.Finalizar(id);
            TempData["Mensaje"] = "El contrato fue finalizado y el empleado quedó inactivo.";
        }
        catch (SqlException ex)
        {
            TempData["Error"] = MensajeErrorFinalizar(ex);
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Editar(int id)
    {
        var contrato = _contratos.ObtenerPorId(id);
        if (contrato is null || !string.Equals(contrato.Estado, "VIGENTE", StringComparison.OrdinalIgnoreCase))
        {
            return NotFound();
        }

        return View("~/Areas/Admin/Views/Contratos/Editar.cshtml", CrearModeloEdicion(contrato));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Editar(Models.EditarContratoModel modelo)
    {
        CargarCatalogos(modelo);
        var contratoActual = _contratos.ObtenerPorId(modelo.IdContrato);
        RestaurarDatosProtegidos(modelo, contratoActual);
        ValidarEdicion(modelo, contratoActual);

        if (!ModelState.IsValid)
        {
            return View("~/Areas/Admin/Views/Contratos/Editar.cshtml", modelo);
        }

        try
        {
            _contratos.Actualizar(
                modelo.IdContrato,
                modelo.IdCargo!.Value,
                modelo.IdDepartamento!.Value,
                modelo.IdTipoContrato!.Value,
                modelo.SalarioBruto!.Value,
                modelo.FechaContratacion!.Value,
                modelo.FechaCulminacion);

            TempData["Mensaje"] = "El contrato fue actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (SqlException ex)
        {
            ModelState.AddModelError(string.Empty, MensajeErrorContrato(ex));
            return View("~/Areas/Admin/Views/Contratos/Editar.cshtml", modelo);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View("~/Areas/Admin/Views/Contratos/Editar.cshtml", modelo);
        }
    }

    private Models.EditarContratoModel CrearModeloEdicion(BEMO.Entidades.ContratoListado contrato)
    {
        var modelo = new Models.EditarContratoModel
        {
            IdContrato = contrato.IdContrato,
            IdEmpleado = contrato.IdEmpleado,
            NombreEmpleado = contrato.NombreEmpleado,
            IdCargo = contrato.IdCargo,
            IdDepartamento = contrato.IdDepartamento,
            IdTipoContrato = contrato.IdTipoContrato,
            SalarioBruto = contrato.SalarioBruto,
            FechaContratacion = contrato.FechaContratacion,
            FechaCulminacion = contrato.FechaCulminacion
        };

        return CargarCatalogos(modelo);
    }

    private Models.EditarContratoModel CargarCatalogos(Models.EditarContratoModel modelo)
    {
        modelo.Departamentos = _departamento.Listar();
        modelo.Cargos = _cargos.Listar();
        modelo.TiposContrato = _tiposContrato.Listar();
        return modelo;
    }

    private static void RestaurarDatosProtegidos(
        Models.EditarContratoModel modelo,
        BEMO.Entidades.ContratoListado? contratoActual)
    {
        if (contratoActual is null)
        {
            modelo.NombreEmpleado = string.Empty;
            return;
        }

        modelo.IdEmpleado = contratoActual.IdEmpleado;
        modelo.NombreEmpleado = contratoActual.NombreEmpleado;
    }

    private void ValidarEdicion(
        Models.EditarContratoModel modelo,
        BEMO.Entidades.ContratoListado? contratoActual)
    {
        if (contratoActual is null)
        {
            ModelState.AddModelError(string.Empty, "El contrato no existe.");
            return;
        }

        if (!string.Equals(contratoActual.Estado, "VIGENTE", StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(string.Empty, "Solo se pueden editar contratos vigentes.");
        }

        if (!modelo.IdDepartamento.HasValue || !modelo.Departamentos.Any(item => item.IdDepartamento == modelo.IdDepartamento.Value))
        {
            ModelState.AddModelError(nameof(modelo.IdDepartamento), "Seleccione un departamento laboral válido.");
        }

        var cargo = modelo.Cargos.FirstOrDefault(item => item.IdCargo == modelo.IdCargo);
        if (!modelo.IdCargo.HasValue || cargo is null)
        {
            ModelState.AddModelError(nameof(modelo.IdCargo), "Seleccione un cargo válido.");
        }
        else if (modelo.SalarioBruto.HasValue
                 && (modelo.SalarioBruto.Value < cargo.RangoMinimoCargo || modelo.SalarioBruto.Value > cargo.RangoMaximoCargo))
        {
            ModelState.AddModelError(nameof(modelo.SalarioBruto),
                $"El salario debe estar entre {cargo.RangoMinimoCargo:N2} y {cargo.RangoMaximoCargo:N2} para el cargo seleccionado.");
        }

        var tipo = modelo.TiposContrato.FirstOrDefault(item => item.IdTipoContrato == modelo.IdTipoContrato);
        if (!modelo.IdTipoContrato.HasValue || tipo is null)
        {
            ModelState.AddModelError(nameof(modelo.IdTipoContrato), "Seleccione un tipo de contrato válido.");
        }
        else if (string.Equals(tipo.NombreTipoContrato, "PERMANENTE", StringComparison.OrdinalIgnoreCase)
                 && modelo.FechaCulminacion.HasValue)
        {
            ModelState.AddModelError(nameof(modelo.FechaCulminacion), "Un contrato permanente no lleva fecha de culminación.");
        }
        else if (string.Equals(tipo.NombreTipoContrato, "TEMPORAL", StringComparison.OrdinalIgnoreCase)
                 && !modelo.FechaCulminacion.HasValue)
        {
            ModelState.AddModelError(nameof(modelo.FechaCulminacion), "Un contrato temporal requiere fecha de culminación.");
        }

        var fechaHoy = DateOnly.FromDateTime(DateTime.Today);
        if (modelo.FechaCulminacion.HasValue && modelo.FechaCulminacion.Value <= fechaHoy)
        {
            ModelState.AddModelError(nameof(modelo.FechaCulminacion), "La fecha de culminación debe ser posterior a la fecha actual.");
        }
        else if (modelo.FechaCulminacion.HasValue
                 && modelo.FechaContratacion.HasValue
                 && modelo.FechaCulminacion.Value < modelo.FechaContratacion.Value)
        {
            ModelState.AddModelError(nameof(modelo.FechaCulminacion), "La fecha de culminación no puede ser anterior a la contratación.");
        }
    }

    private static string MensajeErrorContrato(SqlException ex)
    {
        if (ex.Message.Contains("permanente", StringComparison.OrdinalIgnoreCase))
        {
            return "Un contrato permanente no lleva fecha de culminación.";
        }

        if (ex.Message.Contains("temporal", StringComparison.OrdinalIgnoreCase))
        {
            return "Un contrato temporal requiere fecha de culminación.";
        }

        if (ex.Message.Contains("traslapan", StringComparison.OrdinalIgnoreCase))
        {
            return "Las fechas del contrato se traslapan con otro contrato del empleado.";
        }

        return "No se pudo actualizar el contrato. Verifica la información ingresada.";
    }

    private static string MensajeErrorFinalizar(SqlException ex)
    {
        if (ex.Number is 51030 or 51031)
        {
            return ex.Message.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()
                ?? "No se pudo finalizar el contrato.";
        }

        return "No se pudo finalizar el contrato. El empleado no fue inactivado.";
    }

    [HttpGet]
    public IActionResult Nuevo()
    {
        return View("~/Areas/Admin/Views/Contratos/Nuevo.cshtml", CargarCatalogos(new Models.NuevoContratoModel()));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Nuevo(Models.NuevoContratoModel modelo)
    {
        CargarCatalogos(modelo);
        ValidarRegistro(modelo);
        TraducirErroresDeModelo();

        if (!ModelState.IsValid)
        {
            return View("~/Areas/Admin/Views/Contratos/Nuevo.cshtml", modelo);
        }

        try
        {
            var idContrato = _contratos.Registrar(
                modelo.IdEmpleado!.Value,
                modelo.IdCargo!.Value,
                modelo.IdDepartamento!.Value,
                modelo.IdTipoContrato!.Value,
                modelo.SalarioBruto!.Value,
                modelo.FechaContratacion!.Value,
                modelo.FechaCulminacion);

            TempData["Mensaje"] = $"El contrato fue registrado correctamente con código {idContrato}.";
            return RedirectToAction(nameof(Index));
        }
        catch (SqlException ex)
        {
            ModelState.AddModelError(string.Empty, MensajeErrorNuevoContrato(ex));
            return View("~/Areas/Admin/Views/Contratos/Nuevo.cshtml", CargarCatalogos(modelo));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View("~/Areas/Admin/Views/Contratos/Nuevo.cshtml", CargarCatalogos(modelo));
        }
    }

    private Models.NuevoContratoModel CargarCatalogos(Models.NuevoContratoModel modelo)
    {
        modelo.EmpleadosDisponibles = _empleados.ListarActivosSinContrato()
            .Select(empleado => new Models.ContratoEmpleadoDisponibleOption
            {
                IdEmpleado = empleado.IdEmpleado,
                Nombre = $"{empleado.Nombre} {empleado.Apellido}".Trim(),
                Dui = empleado.Dui
            })
            .ToList();
        modelo.Departamentos = _departamento.Listar();
        modelo.Cargos = _cargos.Listar();
        modelo.TiposContrato = _tiposContrato.Listar();
        return modelo;
    }

    private void ValidarRegistro(Models.NuevoContratoModel modelo)
    {
        var empleado = modelo.EmpleadosDisponibles.FirstOrDefault(item => item.IdEmpleado == modelo.IdEmpleado);
        if (modelo.IdEmpleado.HasValue && empleado is null)
        {
            ModelState.AddModelError(nameof(modelo.IdEmpleado), "Seleccione un empleado activo sin contrato vigente.");
        }
        else if (empleado is not null
                 && !string.Equals(modelo.DuiBusqueda.Trim(), empleado.Dui, StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(nameof(modelo.DuiBusqueda), "El DUI no corresponde al empleado seleccionado.");
        }

        if (modelo.IdDepartamento.HasValue
            && !modelo.Departamentos.Any(departamento => departamento.IdDepartamento == modelo.IdDepartamento.Value))
        {
            ModelState.AddModelError(nameof(modelo.IdDepartamento), "El departamento laboral no es válido.");
        }

        var cargo = modelo.Cargos.FirstOrDefault(item => item.IdCargo == modelo.IdCargo);
        if (modelo.IdCargo.HasValue && cargo is null)
        {
            ModelState.AddModelError(nameof(modelo.IdCargo), "El cargo no es válido.");
        }
        else if (cargo is not null
                 && modelo.SalarioBruto.HasValue
                 && (modelo.SalarioBruto.Value < cargo.RangoMinimoCargo
                     || modelo.SalarioBruto.Value > cargo.RangoMaximoCargo))
        {
            ModelState.AddModelError(nameof(modelo.SalarioBruto),
                $"El salario debe estar entre {cargo.RangoMinimoCargo:N2} y {cargo.RangoMaximoCargo:N2} para el cargo seleccionado.");
        }

        var tipo = modelo.TiposContrato.FirstOrDefault(item => item.IdTipoContrato == modelo.IdTipoContrato);
        if (modelo.IdTipoContrato.HasValue && tipo is null)
        {
            ModelState.AddModelError(nameof(modelo.IdTipoContrato), "El tipo de contrato no es válido.");
        }
        else if (tipo is not null
                 && string.Equals(tipo.NombreTipoContrato, "PERMANENTE", StringComparison.OrdinalIgnoreCase)
                 && modelo.FechaCulminacion.HasValue)
        {
            ModelState.AddModelError(nameof(modelo.FechaCulminacion), "Un contrato permanente no lleva fecha de culminación.");
        }
        else if (tipo is not null
                 && string.Equals(tipo.NombreTipoContrato, "TEMPORAL", StringComparison.OrdinalIgnoreCase)
                 && !modelo.FechaCulminacion.HasValue)
        {
            ModelState.AddModelError(nameof(modelo.FechaCulminacion), "Un contrato temporal requiere fecha de culminación.");
        }

        var fechaHoy = DateOnly.FromDateTime(DateTime.Today);
        if (modelo.FechaCulminacion.HasValue && modelo.FechaCulminacion.Value <= fechaHoy)
        {
            ModelState.AddModelError(nameof(modelo.FechaCulminacion), "La fecha de culminación debe ser posterior a la fecha actual.");
        }
        else if (modelo.FechaCulminacion.HasValue
                 && modelo.FechaContratacion.HasValue
                 && modelo.FechaCulminacion.Value < modelo.FechaContratacion.Value)
        {
            ModelState.AddModelError(nameof(modelo.FechaCulminacion), "La fecha de culminación no puede ser anterior a la contratación.");
        }
    }

    private void TraducirErroresDeModelo()
    {
        foreach (var item in ModelState)
        {
            var mensajes = item.Value.Errors
                .Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage)
                    ? MensajeParaCampo(item.Key)
                    : error.ErrorMessage)
                .ToList();

            if (mensajes.Any(string.IsNullOrWhiteSpace))
            {
                item.Value.Errors.Clear();
                foreach (var mensaje in mensajes)
                {
                    item.Value.Errors.Add(mensaje);
                }
            }
        }
    }

    private static string MensajeParaCampo(string campo)
    {
        return campo switch
        {
            nameof(Models.NuevoContratoModel.IdEmpleado) => "Seleccione un empleado.",
            nameof(Models.NuevoContratoModel.DuiBusqueda) => "Ingrese el DUI del empleado.",
            nameof(Models.NuevoContratoModel.IdDepartamento) => "Seleccione un departamento laboral.",
            nameof(Models.NuevoContratoModel.IdCargo) => "Seleccione un cargo.",
            nameof(Models.NuevoContratoModel.IdTipoContrato) => "Seleccione un tipo de contrato.",
            nameof(Models.NuevoContratoModel.SalarioBruto) => "Ingrese un salario bruto válido.",
            nameof(Models.NuevoContratoModel.FechaContratacion) => "Ingrese una fecha de contratación válida.",
            nameof(Models.NuevoContratoModel.FechaCulminacion) => "Ingrese una fecha de culminación válida.",
            _ => "El valor ingresado no es válido."
        };
    }

    private static string MensajeErrorNuevoContrato(SqlException ex)
    {
        return ex.Number switch
        {
            51020 => "El empleado seleccionado no existe o no está activo.",
            51028 => "El empleado seleccionado ya tiene un contrato vigente.",
            51032 => "Un contrato permanente no lleva fecha de culminación.",
            51033 => "Un contrato temporal requiere fecha de culminación.",
            51009 => "El salario ingresado no está dentro del rango permitido para el cargo.",
            51010 => "La fecha de culminación no puede ser anterior a la contratación.",
            51011 => "La fecha de culminación debe ser posterior a la fecha actual.",
            50003 when ex.Message.Contains("permanente", StringComparison.OrdinalIgnoreCase)
                => "Un contrato permanente no lleva fecha de culminación.",
            50003 when ex.Message.Contains("temporal", StringComparison.OrdinalIgnoreCase)
                => "Un contrato temporal requiere fecha de culminación.",
            50003 => "El empleado ya tiene un contrato vigente.",
            50015 => "Las fechas del contrato se traslapan con otro contrato del empleado.",
            _ => "No se pudo guardar el contrato. Verifica la información ingresada e inténtalo nuevamente."
        };
    }
}
