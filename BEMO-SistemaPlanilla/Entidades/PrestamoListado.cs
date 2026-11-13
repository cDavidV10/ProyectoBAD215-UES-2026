namespace BEMO.Entidades;

public class PrestamoListado
{
    public int IdPrestamo { get; set; }
    public int IdEmpleado { get; set; }
    public string NombreEmpleado { get; set; } = string.Empty;
    public string Dui { get; set; } = string.Empty;
    public DateOnly FechaOtorgamiento { get; set; }
    public DateOnly? FechaFinalizacion { get; set; }
    public decimal Monto { get; set; }
    public int NumCuotas { get; set; }
    public decimal CuotaQuincenal { get; set; }
    public decimal SaldoPendiente { get; set; }
    public int CuotasRestantes { get; set; }
    public string Estado { get; set; } = string.Empty;
}
