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

    [Column("id_empleado")]
    public int IdEmpleado { get; set; }

    [Column("id_tipo_descuento")]
    public int IdTipoDescuento { get; set; }

    [Column("porcentaje_descuento", TypeName = "decimal(5,2)")]
    public decimal PorcentajeDescuento { get; set; }

    [Column("fecha_vigencia")]
    public DateOnly FechaVigencia { get; set; }

    [ForeignKey(nameof(IdEmpleado))]
    public Empleado Empleado { get; set; } = null!;
    [ForeignKey(nameof(IdTipoDescuento))]
    public TipoDescuento TipoDescuento { get; set; } = null!;
}
