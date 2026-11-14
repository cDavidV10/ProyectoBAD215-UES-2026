namespace BEMO.Entidades;

public class ConstanciaEmpleadoOpcion
{
    public int IdEmpleado { get; init; }
    public string NombreCompleto { get; init; } = string.Empty;
    public string Dui { get; init; } = string.Empty;
    public string NombreCargo { get; init; } = string.Empty;
    public string NombreDepartamento { get; init; } = string.Empty;
    public decimal SalarioBruto { get; init; }
    public DateOnly FechaContratacion { get; init; }
}
