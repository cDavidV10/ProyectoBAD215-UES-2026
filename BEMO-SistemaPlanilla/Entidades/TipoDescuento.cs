using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BEMO.Entidades;

[Table("TIPO_DESCUENTO", Schema = "dbo")]
public class TipoDescuento
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id_tipo_descuento")]
    public int IdTipoDescuento { get; set; }

    [Required]
    [StringLength(50)]
    [Column("nombre_tipo_descuento")]
    public string NombreTipoDescuento { get; set; } = string.Empty;

    public ICollection<DescuentoLey> DescuentosLey { get; set; } = new List<DescuentoLey>();
}
