namespace BEMO.Entidades;

public class AnticipoSalarioListado
{
    public int IdAnticipo { get; set; }
    public int IdEmpleado { get; set; }
    public string NombreEmpleado { get; set; } = string.Empty;
    public string Dui { get; set; } = string.Empty;
    public decimal MontoAnticipo { get; set; }
    public DateOnly FechaAnticipo { get; set; }
    public string Estado { get; set; } = string.Empty;
}
