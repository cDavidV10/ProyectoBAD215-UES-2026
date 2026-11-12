namespace BEMO.Entidades;

public class ContratoListado
{
    public int IdContrato { get; init; }
    public int IdEmpleado { get; init; }
    public string NombreEmpleado { get; init; } = string.Empty;
    public string NombreCargo { get; init; } = string.Empty;
    public string NombreTipoContrato { get; init; } = string.Empty;
    public string NombreDepartamento { get; init; } = string.Empty;
    public decimal SalarioBruto { get; init; }
    public DateOnly FechaContratacion { get; init; }
    public DateOnly? FechaCulminacion { get; init; }
    public string Estado { get; init; } = string.Empty;
    public int? DiasRestantes { get; init; }
    public string Situacion { get; init; } = string.Empty;
}
