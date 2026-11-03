using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BEMO.Entidades;

[Table("CAPACITACION", Schema = "dbo")]
public class Capacitacion
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id_capacitacion")]
    public int IdCapacitacion { get; set; }

    [Column("id_tipo_capacitacion")]
    public int IdTipoCapacitacion { get; set; }

    [Required]
    [StringLength(50)]
    [Column("nombre_capacitacion")]
    public string NombreCapacitacion { get; set; } = string.Empty;

    [ForeignKey(nameof(IdTipoCapacitacion))]
    public TipoCapacitacion TipoCapacitacion { get; set; } = null!;
    public ICollection<DetalleCapacitacion> DetallesCapacitacion { get; set; } = new List<DetalleCapacitacion>();
}
