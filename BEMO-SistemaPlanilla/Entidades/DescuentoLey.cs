using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BEMO.Entidades;

[Table("DESCUENTO_LEY", Schema = "dbo")]
public class DescuentoLey
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id_descuento")]
    public int IdDescuento { get; set; }

    [Column("id_tipo_descuento")]
    public int IdTipoDescuento { get; set; }

    [Column("porcentaje_descuento", TypeName = "decimal(5,2)")]
    public decimal PorcentajeDescuento { get; set; }

    [Column("tope_mensual", TypeName = "decimal(10,2)")]
    public decimal? TopeMensual { get; set; }

    [Column("fecha_vigencia")]
    public DateOnly FechaVigencia { get; set; }

    [ForeignKey(nameof(IdTipoDescuento))]
    public TipoDescuento TipoDescuento { get; set; } = null!;

    public ICollection<DetalleDescuentoPlanilla> DetallesDescuentoPlanilla { get; set; } = new List<DetalleDescuentoPlanilla>();
}
