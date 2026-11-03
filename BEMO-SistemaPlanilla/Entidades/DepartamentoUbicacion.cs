using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BEMO.Entidades;

[Table("DEPARTAMENTO_UBICACION", Schema = "dbo")]
public class DepartamentoUbicacion
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id_departamento_ubicacion")]
    public int IdDepartamentoUbicacion { get; set; }

    [Required]
    [StringLength(50)]
    [Column("nombre_departamento_ubicacion")]
    public string NombreDepartamentoUbicacion { get; set; } = string.Empty;

    public ICollection<MunicipioUbicacion> Municipios { get; set; } = new List<MunicipioUbicacion>();
}
