namespace BEMO.Entidades;

public class UsuarioListado
{
    public int IdUsuario { get; init; }
    public int IdEmpleado { get; init; }
    public string NombreUsuario { get; init; } = string.Empty;
    public string NombreEmpleado { get; init; } = string.Empty;
    public string NombreRol { get; init; } = string.Empty;
    public string Estado { get; init; } = string.Empty;
}
