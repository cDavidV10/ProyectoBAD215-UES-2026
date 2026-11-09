using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BEMO.Entidades;

[Table("PLANILLA", Schema = "dbo")]
public class Planilla
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id_planilla")]
    public int IdPlanilla { get; set; }

    [Column("numero_planilla")]
    public int NumeroPlanilla { get; set; }

    [Column("periodo_inicio")]
    public DateOnly PeriodoInicio { get; set; }

    [Column("periodo_fin")]
    public DateOnly PeriodoFin { get; set; }

    [Column("fecha_pago")]
    public DateOnly FechaPago { get; set; }

    [Column("fecha_generacion")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public DateOnly FechaGeneracion { get; set; }

    [Column("total_neto", TypeName = "decimal(10,2)")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public decimal TotalNeto { get; set; }

    [Required]
    [StringLength(20)]
    [Column("estado")]
    public string Estado { get; set; } = "BORRADOR";

    public ICollection<DetallePlanilla> DetallesPlanilla { get; set; } = new List<DetallePlanilla>();
}
