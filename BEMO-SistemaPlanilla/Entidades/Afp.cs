using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BEMO.Entidades;

[Table("AFP", Schema = "dbo")]
public class Afp
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id_afp")]
    public int IdAfp { get; set; }

    [Required]
    [StringLength(15)]
    [Column("nombre_afp")]
    public string NombreAfp { get; set; } = string.Empty;

    public ICollection<Empleado> Empleados { get; set; } = new List<Empleado>();
}
