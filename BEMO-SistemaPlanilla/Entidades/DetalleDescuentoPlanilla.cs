using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BEMO.Entidades;

[Table("DETALLE_DESCUENTO_PLANILLA", Schema = "dbo")]
public class DetalleDescuentoPlanilla
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id_detalle_descuento")]
    public int IdDetalleDescuento { get; set; }

    [Column("id_detalle_planilla")]
    public int IdDetallePlanilla { get; set; }

    [Column("id_descuento")]
    public int IdDescuento { get; set; }

    [Column("porcentaje_aplicado", TypeName = "decimal(5,2)")]
    public decimal PorcentajeAplicado { get; set; }

    [Column("monto_descontado", TypeName = "decimal(10,2)")]
    public decimal MontoDescontado { get; set; }

    [ForeignKey(nameof(IdDetallePlanilla))]
    public DetallePlanilla DetallePlanilla { get; set; } = null!;

    [ForeignKey(nameof(IdDescuento))]
    public DescuentoLey DescuentoLey { get; set; } = null!;
}
