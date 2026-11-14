namespace BEMO.Entidades;

public class EmpleadoPrestamoInfo
{
    public int IdEmpleado { get; set; }
    public string NombreEmpleado { get; set; } = string.Empty;
    public string NombreCargo { get; set; } = string.Empty;
    public decimal SalarioBruto { get; set; }
    public decimal SalarioQuincenal { get; set; }
    public DateOnly? FechaCulminacion { get; set; }
    public decimal CuotaPrestamoActiva { get; set; }
    public decimal AnticipoPendiente { get; set; }
    public decimal LiquidoEstimado { get; set; }
    public bool TienePrestamoActivo { get; set; }
    public bool TieneAnticipoPendiente { get; set; }
    public decimal MaximoPrestamo { get; set; }
    public decimal MaximoAnticipo { get; set; }
}
