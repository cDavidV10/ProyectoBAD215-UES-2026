using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BEMO.Entidades;

[Table("DEPARTAMENTO", Schema = "dbo")]
public class Departamento
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id_departamento")]
    public int IdDepartamento { get; set; }

    [Required]
    [StringLength(50)]
    [Column("nombre_departamento")]
    public string NombreDepartamento { get; set; } = string.Empty;

    public ICollection<Contrato> Contratos { get; set; } = new List<Contrato>();
}
