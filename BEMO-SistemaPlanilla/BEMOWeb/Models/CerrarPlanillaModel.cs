using BEMO.Entidades;

namespace BEMOWeb.Models;

public class CerrarPlanillaModel
{
    public int IdPlanilla { get; set; }
    public PlanillaListado Planilla { get; set; } = null!;
    public bool Confirmar { get; set; }
}
