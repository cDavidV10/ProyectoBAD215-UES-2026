using BEMO.Entidades;

namespace BEMOWeb.Models;

public class AuditoriaIndexModel
{
    public List<AuditoriaListado> Auditorias { get; init; } = new();

    public List<string> Usuarios { get; init; } = new();

    public List<string> Modulos { get; init; } = new();

    public List<string> Operaciones { get; init; } = new();

    public DateOnly? FechaDesde { get; init; }

    public DateOnly? FechaHasta { get; init; }

    public string? UsuarioSeleccionado { get; init; }

    public string? ModuloSeleccionado { get; init; }

    public string? OperacionSeleccionada { get; init; }

    public AuditoriaListado? AuditoriaSeleccionada { get; init; }

    public string? Error { get; init; }

    public int PaginaActual { get; init; }

    public int TotalPaginas { get; init; }

    public int TotalRegistros { get; init; }

    public int TamanoPagina { get; init; }
}
