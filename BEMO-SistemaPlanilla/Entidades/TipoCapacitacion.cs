using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BEMO.Entidades;

[Table("TIPO_CAPACITACION", Schema = "dbo")]
public class TipoCapacitacion
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id_tipo_capacitacion")]
    public int IdTipoCapacitacion { get; set; }

    [Required]
    [StringLength(50)]
    [Column("nombre_tipo_capacitacion")]
    public string NombreTipoCapacitacion { get; set; } = string.Empty;

    public ICollection<Capacitacion> Capacitaciones { get; set; } = new List<Capacitacion>();
}
