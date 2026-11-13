using BEMO.Entidades;

namespace BEMOWeb.Models;

public class PlanillaIndexModel
{
    public List<PlanillaListado> Planillas { get; set; } = new();
    public string? EstadoSeleccionado { get; set; }
    public int PaginaActual { get; set; }
    public int TotalPaginas { get; set; }
    public int TotalRegistros { get; set; }
    public int TamanoPagina { get; set; }
}
