namespace BEMO.Entidades;

public class CapacitacionParticipante
{
    public int IdDetalleCapacitacion { get; set; }
    public int IdEmpleado { get; set; }
    public int IdCapacitacion { get; set; }
    public string Dui { get; set; } = string.Empty;
    public string NombreEmpleado { get; set; } = string.Empty;
    public string NombreDepartamento { get; set; } = string.Empty;
    public DateOnly FechaInicio { get; set; }
    public DateOnly FechaFin { get; set; }
    public decimal Resultado { get; set; }
}
