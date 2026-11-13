using System.ComponentModel.DataAnnotations;
using BEMO.Entidades;

namespace BEMOWeb.Models;

public class CalcularPlanillaModel
{
    public int? IdPlanilla { get; set; }
    public PlanillaListado? Planilla { get; set; }

    [Required(ErrorMessage = "Seleccione el inicio del periodo.")]
    [DataType(DataType.Date)]
    public DateOnly? PeriodoInicio { get; set; }

    [Required(ErrorMessage = "Seleccione el fin del periodo.")]
    [DataType(DataType.Date)]
    public DateOnly? PeriodoFin { get; set; }

    [Required(ErrorMessage = "Seleccione la fecha de pago.")]
    [DataType(DataType.Date)]
    public DateOnly? FechaPago { get; set; }

    public bool IncluirAguinaldo { get; set; }
    public List<RegistroAsistenciaPlanilla> Asistencias { get; set; } = new();
    public int PaginaActual { get; set; }
    public int TotalPaginas { get; set; }
    public int TotalRegistros { get; set; }
    public int TamanoPagina { get; set; }
}
