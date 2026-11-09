using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BEMO.Entidades;

[Table("DISTRITO_UBICACION", Schema = "dbo")]
public class DistritoUbicacion
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id_distrito")]
    public int IdDistrito { get; set; }

    [Column("id_departamento_ubicacion")]
    public int IdDepartamentoUbicacion { get; set; }

    [Required]
    [StringLength(50)]
    [Column("nombre_distrito")]
    public string NombreDistrito { get; set; } = string.Empty;

    [ForeignKey(nameof(IdDepartamentoUbicacion))]
    public DepartamentoUbicacion DepartamentoUbicacion { get; set; } = null!;

    public ICollection<Direccion> Direcciones { get; set; } = new List<Direccion>();
}
