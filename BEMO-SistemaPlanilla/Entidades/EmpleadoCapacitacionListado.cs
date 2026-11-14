namespace BEMO.Entidades;

public class EmpleadoCapacitacionListado
{
    public int IdEmpleado { get; set; }
    public string Dui { get; set; } = string.Empty;
    public string NombreEmpleado { get; set; } = string.Empty;
    public string NombreDepartamento { get; set; } = string.Empty;
    public bool Asignado { get; set; }
}
