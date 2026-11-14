namespace BEMO.Entidades;

public class AuditoriaListado
{
    public int IdAuditoria { get; init; }
    public DateTime FechaHora { get; init; }
    public int IdUsuario { get; init; }
    public string NombreUsuario { get; init; } = string.Empty;
    public string Modulo { get; init; } = string.Empty;
    public string Operacion { get; init; } = string.Empty;
    public string Registro { get; init; } = string.Empty;
    public string CampoModificado { get; init; } = string.Empty;
    public string? ValorViejo { get; init; }
    public string ValorNuevo { get; init; } = string.Empty;
}
