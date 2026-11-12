using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BEMO.Entidades;

[Table("MUNICIPIO_UBICACION", Schema = "dbo")]
public class MunicipioUbicacion
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id_municipio")]
    public int IdMunicipio { get; set; }

    [Column("id_departamento_ubicacion")]
    public int IdDepartamentoUbicacion { get; set; }

    [Required]
    [StringLength(50)]
    [Column("nombre_municipio")]
    public string NombreMunicipio { get; set; } = string.Empty;

    [ForeignKey(nameof(IdDepartamentoUbicacion))]
    public DepartamentoUbicacion DepartamentoUbicacion { get; set; } = null!;
}
