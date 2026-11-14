using BEMO.Entidades;

namespace BEMOWeb.Models;

public class CapacitacionDetalleModel
{
    public CapacitacionListado Capacitacion { get; set; } = new();
    public List<CapacitacionParticipante> Participantes { get; set; } = new();
    public int PaginaActual { get; set; } = 1;
    public int TotalPaginas { get; set; } = 1;
    public int TotalRegistros { get; set; }
    public int TamanoPagina { get; set; } = 8;
}
