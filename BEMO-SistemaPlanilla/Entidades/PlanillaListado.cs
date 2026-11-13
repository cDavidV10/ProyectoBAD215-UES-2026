namespace BEMO.Entidades;

public class PlanillaListado
{
    public int IdPlanilla { get; set; }
    public int NumeroPlanilla { get; set; }
    public DateOnly PeriodoInicio { get; set; }
    public DateOnly PeriodoFin { get; set; }
    public DateOnly FechaPago { get; set; }
    public DateOnly FechaGeneracion { get; set; }
    public decimal TotalNeto { get; set; }
    public string Estado { get; set; } = string.Empty;
    public int CantidadEmpleados { get; set; }
    public decimal TotalBruto { get; set; }
    public decimal TotalDeducciones { get; set; }
    public decimal TotalLiquido { get; set; }
}
