using BEMO.Negocio;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Security.Claims;

namespace BEMOWeb.Controllers;

public abstract class PlanillaControllerBase : Controller
{
    private readonly CN_Planilla _planillas;

    protected PlanillaControllerBase(CN_Planilla planillas)
    {
        _planillas = planillas;
    }

    protected abstract string ViewPrefix { get; }

    private int? UsuarioActualId
    {
        get
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(idClaim, out var idUsuario))
            {
                return idUsuario;
            }

            return HttpContext.Session.GetInt32("id_usuario");
        }
    }

    [HttpGet]
    public IActionResult Index(int pagina = 1, string? estado = null)
    {
        const int tamanoPagina = 5;
        pagina = Math.Max(1, pagina);
        var resultado = _planillas.ListarPaginado(pagina, tamanoPagina, estado);
        var totalPaginas = Math.Max(1, (int)Math.Ceiling(resultado.Total / (double)tamanoPagina));

        if (pagina > totalPaginas)
        {
            pagina = totalPaginas;
            resultado = _planillas.ListarPaginado(pagina, tamanoPagina, estado);
        }

        return Ver("Index", new Models.PlanillaIndexModel
        {
            Planillas = resultado.Planillas,
            EstadoSeleccionado = estado,
            PaginaActual = pagina,
            TotalPaginas = totalPaginas,
            TotalRegistros = resultado.Total,
            TamanoPagina = tamanoPagina
        });
    }

    [HttpGet]
    public IActionResult Calcular(int? id, int pagina = 1)
    {
        if (!id.HasValue)
        {
            var inicio = new DateOnly(DateTime.Today.Year, DateTime.Today.Month, 1);
            return Ver("Calcular", new Models.CalcularPlanillaModel
            {
                PeriodoInicio = inicio,
                PeriodoFin = new DateOnly(inicio.Year, inicio.Month, 15),
                FechaPago = new DateOnly(inicio.Year, inicio.Month, 20),
                PaginaActual = 1,
                TotalPaginas = 1,
                TamanoPagina = 5
            });
        }

        var planilla = _planillas.Obtener(id.Value);
        if (planilla is null)
        {
            return NotFound();
        }

        if (!string.Equals(planilla.Estado, "BORRADOR", StringComparison.OrdinalIgnoreCase))
        {
            TempData["Error"] = "Solo se puede capturar asistencia en una planilla en borrador.";
            return RedirectToAction(nameof(Index));
        }

        const int tamanoPagina = 5;
        pagina = Math.Max(1, pagina);
        var resultadoAsistencia = _planillas.ListarAsistenciaPaginado(planilla.IdPlanilla, pagina, tamanoPagina);
        var totalPaginas = Math.Max(1, (int)Math.Ceiling(resultadoAsistencia.Total / (double)tamanoPagina));

        if (pagina > totalPaginas)
        {
            pagina = totalPaginas;
            resultadoAsistencia = _planillas.ListarAsistenciaPaginado(planilla.IdPlanilla, pagina, tamanoPagina);
        }

        return Ver("Calcular", new Models.CalcularPlanillaModel
        {
            IdPlanilla = planilla.IdPlanilla,
            Planilla = planilla,
            PeriodoInicio = planilla.PeriodoInicio,
            PeriodoFin = planilla.PeriodoFin,
            FechaPago = planilla.FechaPago,
            Asistencias = resultadoAsistencia.Asistencias,
            PaginaActual = pagina,
            TotalPaginas = totalPaginas,
            TotalRegistros = resultadoAsistencia.Total,
            TamanoPagina = tamanoPagina
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Crear(Models.CalcularPlanillaModel modelo)
    {
        var errorPeriodo = ValidarPeriodo(modelo);
        if (errorPeriodo is not null)
        {
            ModelState.AddModelError(string.Empty, errorPeriodo);
        }

        if (!ModelState.IsValid)
        {
            return Ver("Calcular", modelo);
        }

        try
        {
            var idPlanilla = _planillas.Crear(
                modelo.PeriodoInicio!.Value,
                modelo.PeriodoFin!.Value,
                modelo.FechaPago!.Value,
                UsuarioActualId);

            TempData["Mensaje"] = "La planilla fue creada en estado BORRADOR. Capture la asistencia antes de calcularla.";
            return RedirectToAction(nameof(Calcular), new { id = idPlanilla });
        }
        catch (SqlException ex)
        {
            ModelState.AddModelError(string.Empty, MensajeError(ex));
            return Ver("Calcular", modelo);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CapturarAsistencia(Models.CapturarAsistenciaModel modelo)
    {
        var pagina = Math.Max(1, modelo.Pagina);
        var planilla = _planillas.Obtener(modelo.IdPlanilla);
        if (planilla is null)
        {
            TempData["Error"] = "La planilla indicada no existe.";
            return RedirectToAction(nameof(Index));
        }

        if (!string.Equals(planilla.Estado, "BORRADOR", StringComparison.OrdinalIgnoreCase))
        {
            TempData["Error"] = "Solo se puede capturar asistencia en una planilla en borrador.";
            return RedirectToAction(nameof(Index));
        }

        var detalles = _planillas.ListarAsistencia(modelo.IdPlanilla);
        var idsValidos = detalles.Select(item => item.IdDetallePlanilla).ToHashSet();
        var diasPeriodo = planilla.PeriodoFin.DayNumber - planilla.PeriodoInicio.DayNumber + 1;

        foreach (var asistencia in modelo.Asistencias)
        {
            if (!idsValidos.Contains(asistencia.IdDetallePlanilla))
            {
                TempData["Error"] = "La asistencia contiene un detalle de planilla no valido.";
                return RedirectToAction(nameof(Calcular), new { id = modelo.IdPlanilla, pagina });
            }

            if (asistencia.DiasTrabajados < 0
                || asistencia.DiasNoTrabajados < 0
                || asistencia.HorasExtras < 0
                || asistencia.Bonificaciones < 0
                || asistencia.DiasTrabajados + asistencia.DiasNoTrabajados > diasPeriodo)
            {
                TempData["Error"] = "Los dias, horas y bonificaciones deben ser validos y no superar los dias del periodo.";
                return RedirectToAction(nameof(Calcular), new { id = modelo.IdPlanilla, pagina });
            }
        }

        try
        {
            foreach (var asistencia in modelo.Asistencias)
            {
                _planillas.CapturarAsistencia(
                    asistencia.IdDetallePlanilla,
                    asistencia.DiasTrabajados,
                    asistencia.DiasNoTrabajados,
                    asistencia.HorasExtras,
                    asistencia.Bonificaciones,
                    UsuarioActualId);
            }

            TempData["Mensaje"] = "La asistencia fue guardada correctamente.";
        }
        catch (SqlException ex)
        {
            TempData["Error"] = MensajeError(ex);
        }

        return RedirectToAction(nameof(Calcular), new { id = modelo.IdPlanilla, pagina });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Calcular(Models.CalcularPlanillaModel modelo)
    {
        if (!modelo.IdPlanilla.HasValue || modelo.IdPlanilla.Value <= 0)
        {
            TempData["Error"] = "Seleccione una planilla en borrador.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            _planillas.Calcular(modelo.IdPlanilla.Value, modelo.IncluirAguinaldo, UsuarioActualId);
            TempData["Mensaje"] = "La planilla fue calculada correctamente y esta lista para revision.";
        }
        catch (SqlException ex)
        {
            TempData["Error"] = MensajeError(ex);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Cerrar(int id)
    {
        var planilla = _planillas.Obtener(id);
        if (planilla is null)
        {
            return NotFound();
        }

        if (!string.Equals(planilla.Estado, "CALCULADA", StringComparison.OrdinalIgnoreCase))
        {
            TempData["Error"] = "Solo se puede cerrar una planilla que ya fue calculada.";
            return RedirectToAction(nameof(Index));
        }

        return Ver("Cerrar", new Models.CerrarPlanillaModel
        {
            IdPlanilla = id,
            Planilla = planilla
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Cerrar(Models.CerrarPlanillaModel modelo)
    {
        var planilla = _planillas.Obtener(modelo.IdPlanilla);
        if (planilla is null)
        {
            return NotFound();
        }

        modelo.Planilla = planilla;
        if (!modelo.Confirmar)
        {
            ModelState.AddModelError(nameof(modelo.Confirmar), "Debe confirmar que reviso los importes y descuentos.");
            return Ver("Cerrar", modelo);
        }

        try
        {
            _planillas.Cerrar(modelo.IdPlanilla, UsuarioActualId);
            TempData["Mensaje"] = "La planilla fue cerrada correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (SqlException ex)
        {
            ModelState.AddModelError(string.Empty, MensajeError(ex));
            return Ver("Cerrar", modelo);
        }
    }

    [HttpGet]
    public IActionResult Reabrir(int id)
    {
        var planilla = _planillas.Obtener(id);
        if (planilla is null)
        {
            return NotFound();
        }

        if (!string.Equals(planilla.Estado, "CALCULADA", StringComparison.OrdinalIgnoreCase))
        {
            TempData["Error"] = "Solo se puede reabrir una planilla calculada.";
            return RedirectToAction(nameof(Index));
        }

        return Ver("Reabrir", new Models.ReabrirPlanillaModel
        {
            IdPlanilla = id,
            Planilla = planilla
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ConfirmarReapertura(int id, string? motivo)
    {
        var planilla = _planillas.Obtener(id);
        if (planilla is null)
        {
            return NotFound();
        }

        if (!string.Equals(planilla.Estado, "CALCULADA", StringComparison.OrdinalIgnoreCase))
        {
            TempData["Error"] = "Solo se puede reabrir una planilla que este en estado CALCULADA. Las planillas CERRADAS son definitivas.";
            return RedirectToAction(nameof(Index));
        }

        if (string.IsNullOrWhiteSpace(motivo))
        {
            TempData["Error"] = "Indica el motivo de la reapertura antes de continuar.";
            return RedirectToAction(nameof(Reabrir), new { id });
        }

        try
        {
            var idUsuario = UsuarioActualId;
            if (!idUsuario.HasValue)
            {
                TempData["Error"] = "No se pudo identificar al usuario que intenta reabrir la planilla.";
                return RedirectToAction(nameof(Index));
            }

            _planillas.Reabrir(id, motivo.Trim(), idUsuario.Value);

            var planillaReabierta = _planillas.Obtener(id);
            if (planillaReabierta is null
                || !string.Equals(planillaReabierta.Estado, "BORRADOR", StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = "La reapertura no modifico el estado de la planilla. Verifica el usuario y los permisos de la base de datos.";
                return RedirectToAction(nameof(Index));
            }

            TempData["Mensaje"] = "La planilla fue reabierta y regreso a BORRADOR.";
            return RedirectToAction(nameof(Index));
        }
        catch (SqlException ex)
        {
            TempData["Error"] = MensajeError(ex);
            return RedirectToAction(nameof(Index));
        }
        catch (Exception)
        {
            TempData["Error"] = "No se pudo reabrir la planilla. Verifica la conexion y las reglas de la base de datos.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet]
    public IActionResult Detalle(int id, int pagina = 1)
    {
        var planilla = _planillas.Obtener(id);
        if (planilla is null)
        {
            return NotFound();
        }

        const int tamanoPagina = 5;
        pagina = Math.Max(1, pagina);
        var resultado = _planillas.ListarDetalle(id, pagina, tamanoPagina);
        var totalPaginas = Math.Max(1, (int)Math.Ceiling(resultado.Total / (double)tamanoPagina));

        if (pagina > totalPaginas)
        {
            pagina = totalPaginas;
            resultado = _planillas.ListarDetalle(id, pagina, tamanoPagina);
        }

        return Ver("Detalle", new Models.DetallePlanillaModel
        {
            Planilla = planilla,
            Detalles = resultado.Detalles,
            PaginaActual = pagina,
            TotalPaginas = totalPaginas,
            TotalRegistros = resultado.Total,
            TamanoPagina = tamanoPagina
        });
    }

    private IActionResult Ver(string vista, object modelo)
    {
        return View($"{ViewPrefix}/{vista}.cshtml", modelo);
    }

    private static string? ValidarPeriodo(Models.CalcularPlanillaModel modelo)
    {
        if (!modelo.PeriodoInicio.HasValue || !modelo.PeriodoFin.HasValue || !modelo.FechaPago.HasValue)
        {
            return null;
        }

        var inicio = modelo.PeriodoInicio.Value;
        var fin = modelo.PeriodoFin.Value;
        var esPrimeraQuincena = inicio.Day == 1 && fin == new DateOnly(inicio.Year, inicio.Month, 15);
        var esSegundaQuincena = inicio.Day == 16 && fin == new DateOnly(inicio.Year, inicio.Month, DateTime.DaysInMonth(inicio.Year, inicio.Month));

        if (!esPrimeraQuincena && !esSegundaQuincena)
        {
            return "El periodo debe ser del 1 al 15 o del 16 al ultimo dia del mes.";
        }

        return modelo.FechaPago.Value < fin
            ? "La fecha de pago no puede ser anterior al fin del periodo."
            : null;
    }

    private static string MensajeError(SqlException ex)
    {
        if (ex.Number == 50011)
        {
            return "El periodo debe ser del 1 al 15 o del 16 al ultimo dia del mes y no debe traslaparse con otra planilla.";
        }

        if (ex.Number == 51080)
        {
            return "La fecha de fin no puede ser anterior al inicio del periodo.";
        }

        if (ex.Number == 51081)
        {
            return "Una planilla quincenal no puede abarcar mas de 16 dias.";
        }

        if (ex.Number == 51082)
        {
            return "La fecha de pago no puede ser anterior al fin del periodo.";
        }

        if (ex.Number == 51083)
        {
            return "Ya existe una planilla que se traslapa con ese periodo.";
        }

        if (ex.Number == 51085)
        {
            return "No hay empleados con contrato vigente para ese periodo.";
        }

        if (ex.Number == 51086)
        {
            return "No se puede crear la planilla porque la quincena anterior aun no existe.";
        }

        if (ex.Number is 50012 or 50013)
        {
            return ex.Message;
        }

        if (ex.Number is 51090 or 51091 or 51092 or 51093)
        {
            return "La asistencia ingresada no es valida para el periodo seleccionado.";
        }

        if (ex.Number is 51100 or 51101 or 51102 or 51103 or 51104 or 51105 or 51106
            or 51110 or 51111 or 51112 or 51113
            or 51120 or 51121 or 51122)
        {
            return ex.Message;
        }

        return "No se pudo completar la operacion de planilla. Verifica las condiciones del periodo y vuelve a intentarlo.";
    }
}
