namespace BEMO.Entidades;

public class CostoDepartamentoComparativo
{
    public int NumeroPlanilla { get; set; }
    public DateOnly PeriodoInicio { get; set; }
    public DateOnly PeriodoFin { get; set; }
    public string NombreDepartamento { get; set; } = string.Empty;
    public int Empleados { get; set; }
    public decimal CostoTotal { get; set; }
    public decimal? CostoPeriodoAnterior { get; set; }
    public decimal? Diferencia { get; set; }
    public decimal? VariacionPorcentual { get; set; }
}
