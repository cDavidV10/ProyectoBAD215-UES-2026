using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BEMO.Entidades;

[Table("DIRECCION", Schema = "dbo")]
public class Direccion
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id_direccion")]
    public int IdDireccion { get; set; }

    [Column("id_distrito")]
    public int IdDistrito { get; set; }

    [Required]
    [StringLength(50)]
    [Column("nombre_calle")]
    public string NombreCalle { get; set; } = string.Empty;

    [Required]
    [StringLength(10)]
    [Column("numero_casa")]
    public string NumeroCasa { get; set; } = string.Empty;

    [ForeignKey(nameof(IdDistrito))]
    public DistritoUbicacion Distrito { get; set; } = null!;

    public ICollection<Empleado> Empleados { get; set; } = new List<Empleado>();
}
