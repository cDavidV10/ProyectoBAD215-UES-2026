using BEMO.Entidades;

namespace BEMOWeb.Models;

public class ReporteCostosModel
{
    public List<CostoDepartamentoComparativo> Registros { get; init; } = new();
    public List<ReporteCostoDepartamentoResumen> Resumen { get; init; } = new();
    public List<Departamento> Departamentos { get; init; } = new();
    public DateOnly FechaDesde { get; init; }
    public DateOnly FechaHasta { get; init; }
    public string? DepartamentoSeleccionado { get; init; }
    public string? Error { get; init; }
    public int PaginaActual { get; init; }
    public int TotalPaginas { get; init; }
    public int TotalRegistros { get; init; }
    public int TamanoPagina { get; init; }
    public decimal CostoTotal { get; init; }
    public decimal CostoPeriodoAnterior { get; init; }
    public decimal DiferenciaTotal { get; init; }
    public decimal? VariacionTotal { get; init; }
    public int PlanillasComparadas { get; init; }
}

public class ReporteCostoDepartamentoResumen
{
    public string NombreDepartamento { get; init; } = string.Empty;
    public int Periodos { get; init; }
    public int Empleados { get; init; }
    public decimal CostoTotal { get; init; }
    public decimal CostoPeriodoAnterior { get; init; }
    public decimal Diferencia { get; init; }
    public decimal? VariacionPorcentual { get; init; }
}
