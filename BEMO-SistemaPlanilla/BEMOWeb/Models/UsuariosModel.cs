using BEMO.Entidades;

namespace BEMOWeb.Models;

public class UsuariosModel
{
    public List<UsuarioListado> Usuarios { get; init; } = new();

    public List<string> Roles { get; init; } = new();

    public string? Busqueda { get; init; }

    public string? RolSeleccionado { get; init; }

    public string? EstadoSeleccionado { get; init; }

    public int PaginaActual { get; init; }

    public int TotalPaginas { get; init; }

    public int TotalRegistros { get; init; }

    public int TamanoPagina { get; init; }
}
