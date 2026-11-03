using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BEMO.Entidades;

[Table("TIPO_RENTA", Schema = "dbo")]
public class TipoRenta
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id_tipo_renta")]
    public int IdTipoRenta { get; set; }

    [Required]
    [StringLength(20)]
    [Column("nombre_tramo")]
    public string NombreTramo { get; set; } = string.Empty;

    public ICollection<Renta> Rentas { get; set; } = new List<Renta>();
}
