using System.ComponentModel.DataAnnotations;
using BEMO.Entidades;

namespace BEMOWeb.Models;

public class ReabrirPlanillaModel
{
    public int IdPlanilla { get; set; }
    public PlanillaListado Planilla { get; set; } = null!;

    [Required(ErrorMessage = "Indique el motivo de la reapertura.")]
    [StringLength(100)]
    public string Motivo { get; set; } = string.Empty;
}
