using BEMO.Entidades;
using BEMO.Negocio;
using BEMOWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Negocio;

namespace BEMOWeb.Controllers;

public abstract class CapacitacionesControllerBase : Controller
{
    private const string VistasCapacitaciones = "~/Areas/Admin/Views/Capacitaciones/";
    private readonly CN_Capacitacion _capacitaciones;
    private readonly CN_Departamento _departamentos;

    protected CapacitacionesControllerBase(
        CN_Capacitacion capacitaciones,
        CN_Departamento departamentos)
    {
        _capacitaciones = capacitaciones;
        _departamentos = departamentos;
    }

    protected IActionResult IndexCore(
        int pagina = 1,
        string? busqueda = null,
        int? tipo = null,
        string? estado = null)
    {
        const int tamanoPagina = 8;
        pagina = Math.Max(1, pagina);
        tipo = tipo is > 0 ? tipo : null;
        estado = Normalizar(estado)?.ToUpperInvariant();
        busqueda = Normalizar(busqueda);

        var resultado = _capacitaciones.ListarPaginado(pagina, tamanoPagina, busqueda, tipo, estado);
        var totalPaginas = Math.Max(1, (int)Math.Ceiling(resultado.Total / (double)tamanoPagina));

        if (pagina > totalPaginas)
        {
            pagina = totalPaginas;
            resultado = _capacitaciones.ListarPaginado(pagina, tamanoPagina, busqueda, tipo, estado);
        }

        var modelo = new CapacitacionesIndexModel
        {
            Capacitaciones = resultado.Capacitaciones,
            TiposCapacitacion = _capacitaciones.ListarTipos(),
            Busqueda = busqueda,
            TipoSeleccionado = tipo,
            EstadoSeleccionado = estado,
            PaginaActual = pagina,
            TotalPaginas = totalPaginas,
            TotalRegistros = resultado.Total,
            TamanoPagina = tamanoPagina
        };

        return View(VistasCapacitaciones + "Index.cshtml", modelo);
    }

    protected IActionResult NuevaGetCore()
    {
        return View(VistasCapacitaciones + "Nueva.cshtml", CargarTipos(new NuevaCapacitacionModel()));
    }

    protected IActionResult NuevaPostCore(NuevaCapacitacionModel modelo)
    {
        modelo.NombreCapacitacion = modelo.NombreCapacitacion?.Trim() ?? string.Empty;
        CargarTipos(modelo);

        if (modelo.IdTipoCapacitacion is null
            || !modelo.TiposCapacitacion.Any(tipo => tipo.IdTipoCapacitacion == modelo.IdTipoCapacitacion.Value))
        {
            ModelState.AddModelError(nameof(modelo.IdTipoCapacitacion), "Seleccione un tipo de capacitación válido.");
        }

        if (!ModelState.IsValid)
        {
            return View(VistasCapacitaciones + "Nueva.cshtml", modelo);
        }

        try
        {
            var idCapacitacion = _capacitaciones.Registrar(modelo.NombreCapacitacion, modelo.IdTipoCapacitacion!.Value);
            TempData["Mensaje"] = $"La capacitación CAP-{idCapacitacion:D6} fue creada correctamente. Ahora puede asignar participantes.";
            return RedirectToAction("Index");
        }
        catch (SqlException ex)
        {
            ModelState.AddModelError(string.Empty, MensajeError(ex, "No se pudo crear la capacitación."));
            return View(VistasCapacitaciones + "Nueva.cshtml", modelo);
        }
    }

    protected IActionResult ParticipantesGetCore(
        int id,
        string? busqueda = null,
        string? departamento = null,
        int pagina = 1,
        string? seleccionados = null)
    {
        var capacitacion = _capacitaciones.ObtenerPorId(id);
        if (capacitacion is null)
        {
            return NotFound();
        }

        if (string.Equals(capacitacion.Estado, "FINALIZADA", StringComparison.OrdinalIgnoreCase))
        {
            TempData["Error"] = "No se pueden asignar empleados a una capacitación finalizada.";
            return RedirectToAction("Detalle", new { id });
        }

        return View(
            VistasCapacitaciones + "Participantes.cshtml",
            CrearModeloParticipantes(capacitacion, busqueda, departamento, pagina, seleccionados));
    }

    protected IActionResult ParticipantesPostCore(CapacitacionParticipantesModel modelo)
    {
        var capacitacion = _capacitaciones.ObtenerPorId(modelo.IdCapacitacion);
        if (capacitacion is null)
        {
            return NotFound();
        }

        if (string.Equals(capacitacion.Estado, "FINALIZADA", StringComparison.OrdinalIgnoreCase))
        {
            TempData["Error"] = "No se pueden asignar empleados a una capacitación finalizada.";
            return RedirectToAction("Detalle", new { id = modelo.IdCapacitacion });
        }

        modelo.NombreCapacitacion = capacitacion.NombreCapacitacion;
        CargarEmpleados(modelo);
        ValidarParticipantes(modelo);

        if (!ModelState.IsValid)
        {
            return View(VistasCapacitaciones + "Participantes.cshtml", modelo);
        }

        try
        {
            _capacitaciones.RegistrarParticipaciones(
                modelo.IdCapacitacion,
                modelo.IdEmpleadosSeleccionados,
                modelo.FechaInicio!.Value,
                modelo.FechaFin!.Value);

            TempData["Mensaje"] = "Los participantes fueron asignados correctamente.";
            return RedirectToAction("Index");
        }
        catch (SqlException ex)
        {
            ModelState.AddModelError(string.Empty, MensajeError(ex, "No se pudieron asignar los participantes."));
            return View(VistasCapacitaciones + "Participantes.cshtml", modelo);
        }
    }

    protected IActionResult DetalleCore(int id, int pagina = 1)
    {
        var capacitacion = _capacitaciones.ObtenerPorId(id);
        if (capacitacion is null)
        {
            return NotFound();
        }

        const int tamanoPagina = 8;
        pagina = Math.Max(1, pagina);
        var resultado = _capacitaciones.ListarParticipantesPaginado(id, pagina, tamanoPagina);
        var totalPaginas = Math.Max(1, (int)Math.Ceiling(resultado.Total / (double)tamanoPagina));

        if (pagina > totalPaginas)
        {
            pagina = totalPaginas;
            resultado = _capacitaciones.ListarParticipantesPaginado(id, pagina, tamanoPagina);
        }

        return View(VistasCapacitaciones + "Detalle.cshtml", new CapacitacionDetalleModel
        {
            Capacitacion = capacitacion,
            Participantes = resultado.Participantes,
            PaginaActual = pagina,
            TotalPaginas = totalPaginas,
            TotalRegistros = resultado.Total,
            TamanoPagina = tamanoPagina
        });
    }

    protected IActionResult ActualizarResultadoCore(
        int idCapacitacion,
        int idDetalleCapacitacion,
        decimal resultado,
        int pagina = 1)
    {
        if (resultado is < 0 or > 100)
        {
            TempData["Error"] = "El resultado debe estar entre 0 y 100.";
            return RedirectToAction("Detalle", new { id = idCapacitacion, pagina });
        }

        var participante = _capacitaciones.ListarParticipantes(idCapacitacion)
            .FirstOrDefault(item => item.IdDetalleCapacitacion == idDetalleCapacitacion);
        if (participante is null)
        {
            return NotFound();
        }

        try
        {
            _capacitaciones.ActualizarResultado(idDetalleCapacitacion, resultado);
            TempData["Mensaje"] = "El resultado fue actualizado correctamente.";
        }
        catch (SqlException ex)
        {
            TempData["Error"] = MensajeError(ex, "No se pudo actualizar el resultado.");
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction("Detalle", new { id = idCapacitacion, pagina });
    }

    private NuevaCapacitacionModel CargarTipos(NuevaCapacitacionModel modelo)
    {
        modelo.TiposCapacitacion = _capacitaciones.ListarTipos();
        return modelo;
    }

    private CapacitacionParticipantesModel CrearModeloParticipantes(
        CapacitacionListado capacitacion,
        string? busqueda,
        string? departamento,
        int pagina,
        string? seleccionados)
    {
        var modelo = new CapacitacionParticipantesModel
        {
            IdCapacitacion = capacitacion.IdCapacitacion,
            NombreCapacitacion = capacitacion.NombreCapacitacion,
            Busqueda = Normalizar(busqueda),
            DepartamentoSeleccionado = Normalizar(departamento),
            FechaInicio = capacitacion.FechaInicio,
            FechaFin = capacitacion.FechaFin,
            PaginaActual = Math.Max(1, pagina),
            IdEmpleadosSeleccionados = ParsearIds(seleccionados),
            Departamentos = _departamentos.Listar()
        };

        CargarEmpleados(modelo);
        return modelo;
    }

    private void CargarEmpleados(CapacitacionParticipantesModel modelo)
    {
        const int tamanoPagina = 8;
        modelo.PaginaActual = Math.Max(1, modelo.PaginaActual);
        modelo.Departamentos = _departamentos.Listar();
        var resultado = _capacitaciones.ListarEmpleadosParaAsignacionPaginado(
            modelo.IdCapacitacion,
            Normalizar(modelo.Busqueda),
            Normalizar(modelo.DepartamentoSeleccionado),
            modelo.PaginaActual,
            tamanoPagina);
        modelo.TotalRegistros = resultado.Total;
        modelo.TotalPaginas = Math.Max(1, (int)Math.Ceiling(resultado.Total / (double)tamanoPagina));

        if (modelo.PaginaActual > modelo.TotalPaginas)
        {
            modelo.PaginaActual = modelo.TotalPaginas;
            resultado = _capacitaciones.ListarEmpleadosParaAsignacionPaginado(
                modelo.IdCapacitacion,
                Normalizar(modelo.Busqueda),
                Normalizar(modelo.DepartamentoSeleccionado),
                modelo.PaginaActual,
                tamanoPagina);
        }

        modelo.TamanoPagina = tamanoPagina;
        modelo.Empleados = resultado.Empleados;
    }

    private void ValidarParticipantes(CapacitacionParticipantesModel modelo)
    {
        if (modelo.FechaInicio is null || modelo.FechaFin is null)
        {
            return;
        }

        if (modelo.FechaFin.Value < modelo.FechaInicio.Value)
        {
            ModelState.AddModelError(nameof(modelo.FechaFin), "La fecha final no puede ser anterior a la fecha inicial.");
        }

        var seleccionados = modelo.IdEmpleadosSeleccionados.Distinct().ToList();
        modelo.IdEmpleadosSeleccionados = seleccionados;
        if (seleccionados.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "Seleccione al menos un empleado para asignar.");
            return;
        }

        var empleadosDisponibles = _capacitaciones.ListarEmpleadosParaAsignacion(modelo.IdCapacitacion, null, null);
        var empleadosPorId = empleadosDisponibles.ToDictionary(empleado => empleado.IdEmpleado);
        var noValidos = seleccionados
            .Where(id => !empleadosPorId.TryGetValue(id, out var empleado) || empleado.Asignado)
            .ToList();

        if (noValidos.Count > 0)
        {
            ModelState.AddModelError(string.Empty, "Uno o más empleados ya están asignados o no están disponibles.");
        }
    }

    private static string? Normalizar(string? valor)
    {
        return string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
    }

    private static List<int> ParsearIds(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            return new List<int>();
        }

        return valor
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(id => int.TryParse(id, out var resultado) ? resultado : 0)
            .Where(id => id > 0)
            .Distinct()
            .ToList();
    }

    private static string MensajeError(SqlException ex, string mensajePredeterminado)
    {
        if (!string.IsNullOrWhiteSpace(ex.Message)
            && (ex.Number is >= 52050 and <= 52069 || ex.Number == 2627 || ex.Number == 2601))
        {
            return ex.Message.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()
                ?? mensajePredeterminado;
        }

        return mensajePredeterminado;
    }
}
