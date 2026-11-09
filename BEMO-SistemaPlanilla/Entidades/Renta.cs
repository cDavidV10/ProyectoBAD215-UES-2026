using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BEMO.Entidades;

[Table("RENTA", Schema = "dbo")]
public class Renta
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id_renta")]
    public int IdRenta { get; set; }

    [Column("id_tipo_renta")]
    public int IdTipoRenta { get; set; }

    [Required]
    [StringLength(20)]
    [Column("codigo_renta")]
    public string CodigoRenta { get; set; } = string.Empty;

    [Column("rango_minimo_renta", TypeName = "decimal(10,2)")]
    public decimal RangoMinimoRenta { get; set; }

    [Column("rango_maximo_renta", TypeName = "decimal(10,2)")]
    public decimal RangoMaximoRenta { get; set; }

    [Column("porcentaje_renta", TypeName = "decimal(10,2)")]
    public decimal PorcentajeRenta { get; set; }

    [Column("valor_fijo", TypeName = "decimal(10,2)")]
    public decimal ValorFijo { get; set; }

    [Column("fecha_vigencia_renta")]
    public DateOnly FechaVigenciaRenta { get; set; }

    [ForeignKey(nameof(IdTipoRenta))]
    public TipoRenta TipoRenta { get; set; } = null!;

    public ICollection<DetallePlanilla> DetallesPlanilla { get; set; } = new List<DetallePlanilla>();
}
