using BEMO.Entidades;
using BEMO.Negocio;
using Microsoft.AspNetCore.Mvc;

namespace BEMOWeb.Controllers;

public abstract class ReportesControllerBase : Controller
{
    private readonly CN_ReporteCostos _reportes;
    private readonly global::Negocio.CN_Departamento _departamentos;

    protected ReportesControllerBase(
        CN_ReporteCostos reportes,
        global::Negocio.CN_Departamento departamentos)
    {
        _reportes = reportes;
        _departamentos = departamentos;
    }

    protected abstract string ViewPrefix { get; }

    [HttpGet]
    public IActionResult Index(
        int pagina = 1,
        DateOnly? fechaDesde = null,
        DateOnly? fechaHasta = null,
        string? departamento = null)
    {
        const int tamanoPagina = 10;
        var hasta = fechaHasta ?? DateOnly.FromDateTime(DateTime.Today);
        var desde = fechaDesde ?? hasta.AddMonths(-12);
        var departamentos = _departamentos.Listar()
            .OrderBy(item => item.NombreDepartamento)
            .ToList();
        departamento = Normalizar(departamento);

        if (hasta < desde)
        {
            return Ver(CrearModelo(
                Array.Empty<CostoDepartamentoComparativo>(),
                departamentos,
                desde,
                hasta,
                departamento,
                1,
                tamanoPagina,
                "La fecha final no puede ser anterior a la fecha inicial."));
        }

        var registros = _reportes.Obtener(desde, hasta)
            .Where(item => departamento is null
                || string.Equals(item.NombreDepartamento, departamento, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(item => item.PeriodoInicio)
            .ThenBy(item => item.NombreDepartamento)
            .ThenBy(item => item.NumeroPlanilla)
            .ToList();

        pagina = Math.Max(1, pagina);
        var totalPaginas = Math.Max(1, (int)Math.Ceiling(registros.Count / (double)tamanoPagina));
        if (pagina > totalPaginas)
        {
            pagina = totalPaginas;
        }

        return Ver(CrearModelo(
            registros,
            departamentos,
            desde,
            hasta,
            departamento,
            pagina,
            tamanoPagina,
            null));
    }

    private Models.ReporteCostosModel CrearModelo(
        IEnumerable<CostoDepartamentoComparativo> registros,
        List<Departamento> departamentos,
        DateOnly fechaDesde,
        DateOnly fechaHasta,
        string? departamento,
        int pagina,
        int tamanoPagina,
        string? error)
    {
        var todos = registros.ToList();
        var totalRegistros = todos.Count;
        var totalPaginas = Math.Max(1, (int)Math.Ceiling(totalRegistros / (double)tamanoPagina));
        var paginaAjustada = Math.Min(Math.Max(1, pagina), totalPaginas);
        var registrosPagina = todos
            .Skip((paginaAjustada - 1) * tamanoPagina)
            .Take(tamanoPagina)
            .ToList();
        var resumen = todos
            .GroupBy(item => item.NombreDepartamento)
            .Select(grupo => CrearResumen(grupo.Key, grupo))
            .OrderByDescending(item => item.CostoTotal)
            .ThenBy(item => item.NombreDepartamento)
            .ToList();
        var costoTotal = todos.Sum(item => item.CostoTotal);
        var costoAnterior = todos.Sum(item => item.CostoPeriodoAnterior ?? 0);
        var diferencia = todos.Sum(item => item.Diferencia ?? 0);

        return new Models.ReporteCostosModel
        {
            Registros = registrosPagina,
            Resumen = resumen,
            Departamentos = departamentos,
            FechaDesde = fechaDesde,
            FechaHasta = fechaHasta,
            DepartamentoSeleccionado = departamento,
            Error = error,
            PaginaActual = paginaAjustada,
            TotalPaginas = totalPaginas,
            TotalRegistros = totalRegistros,
            TamanoPagina = tamanoPagina,
            CostoTotal = costoTotal,
            CostoPeriodoAnterior = costoAnterior,
            DiferenciaTotal = diferencia,
            VariacionTotal = costoAnterior > 0
                ? Math.Round(diferencia * 100 / costoAnterior, 2)
                : null,
            PlanillasComparadas = todos
                .Select(item => item.NumeroPlanilla)
                .Distinct()
                .Count()
        };
    }

    private static Models.ReporteCostoDepartamentoResumen CrearResumen(
        string departamento,
        IEnumerable<CostoDepartamentoComparativo> registros)
    {
        var elementos = registros.ToList();
        var costoTotal = elementos.Sum(item => item.CostoTotal);
        var costoAnterior = elementos.Sum(item => item.CostoPeriodoAnterior ?? 0);
        var diferencia = elementos.Sum(item => item.Diferencia ?? 0);

        return new Models.ReporteCostoDepartamentoResumen
        {
            NombreDepartamento = departamento,
            Periodos = elementos.Count,
            Empleados = elementos.Sum(item => item.Empleados),
            CostoTotal = costoTotal,
            CostoPeriodoAnterior = costoAnterior,
            Diferencia = diferencia,
            VariacionPorcentual = costoAnterior > 0
                ? Math.Round(diferencia * 100 / costoAnterior, 2)
                : null
        };
    }

    private IActionResult Ver(Models.ReporteCostosModel modelo)
    {
        return View($"{ViewPrefix}/Index.cshtml", modelo);
    }

    private static string? Normalizar(string? valor) =>
        string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
}
