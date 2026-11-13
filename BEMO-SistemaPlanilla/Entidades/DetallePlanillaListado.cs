namespace BEMO.Entidades;

public class DetallePlanillaListado
{
    public int IdPlanilla { get; set; }
    public int NumeroPlanilla { get; set; }
    public DateOnly PeriodoInicio { get; set; }
    public DateOnly PeriodoFin { get; set; }
    public DateOnly FechaPago { get; set; }
    public string EstadoPlanilla { get; set; } = string.Empty;
    public int IdDetallePlanilla { get; set; }
    public int IdEmpleado { get; set; }
    public string NombreEmpleado { get; set; } = string.Empty;
    public string Dui { get; set; } = string.Empty;
    public string NombreDepartamento { get; set; } = string.Empty;
    public string NombreCargo { get; set; } = string.Empty;
    public int DiasTrabajados { get; set; }
    public int DiasNoTrabajados { get; set; }
    public int HorasExtras { get; set; }
    public decimal SalarioMensual { get; set; }
    public decimal SalarioQuincenal { get; set; }
    public decimal Bonificaciones { get; set; }
    public decimal Aguinaldo { get; set; }
    public decimal TotalBruto { get; set; }
    public decimal MontoIsss { get; set; }
    public decimal MontoAfp { get; set; }
    public decimal MontoRenta { get; set; }
    public decimal DescuentosPrestamos { get; set; }
    public decimal DescuentosAnticipos { get; set; }
    public decimal TotalDeducciones { get; set; }
    public decimal SalarioLiquido { get; set; }
    public string EstadoDetalle { get; set; } = string.Empty;
}
