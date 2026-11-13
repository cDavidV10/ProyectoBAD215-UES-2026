using BEMO.Entidades;

namespace BEMOWeb.Models;

public class DetallePlanillaModel
{
    public PlanillaListado Planilla { get; set; } = null!;
    public List<DetallePlanillaListado> Detalles { get; set; } = new();
    public int PaginaActual { get; set; }
    public int TotalPaginas { get; set; }
    public int TotalRegistros { get; set; }
    public int TamanoPagina { get; set; }
}
