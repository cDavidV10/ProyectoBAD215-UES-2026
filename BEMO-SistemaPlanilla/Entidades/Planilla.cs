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

    [Column("id_contrato")]
    public int IdContrato { get; set; }

    [Column("salario_neto", TypeName = "decimal(10,2)")]
    public decimal SalarioNeto { get; set; }

    [Column("fecha_pago")]
    public DateOnly FechaPago { get; set; }

    [ForeignKey(nameof(IdContrato))]
    public Contrato Contrato { get; set; } = null!;
    public ICollection<DetallePlanilla> DetallesPlanilla { get; set; } = new List<DetallePlanilla>();
}
