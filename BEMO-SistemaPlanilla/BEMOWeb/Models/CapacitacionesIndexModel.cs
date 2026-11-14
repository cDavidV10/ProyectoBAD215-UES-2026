using BEMO.Entidades;

namespace BEMOWeb.Models;

public class CapacitacionesIndexModel
{
    public List<CapacitacionListado> Capacitaciones { get; set; } = new();
    public List<TipoCapacitacion> TiposCapacitacion { get; set; } = new();
    public string? Busqueda { get; set; }
    public int? TipoSeleccionado { get; set; }
    public string? EstadoSeleccionado { get; set; }
    public int PaginaActual { get; set; } = 1;
    public int TotalPaginas { get; set; } = 1;
    public int TotalRegistros { get; set; }
    public int TamanoPagina { get; set; } = 8;
}
