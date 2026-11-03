using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BEMO.Entidades;

[Table("TIPO_CONTRATO", Schema = "dbo")]
public class TipoContrato
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id_tipo_contrato")]
    public int IdTipoContrato { get; set; }

    [Required]
    [StringLength(50)]
    [Column("nombre_tipo_contrato")]
    public string NombreTipoContrato { get; set; } = string.Empty;

    public ICollection<Contrato> Contratos { get; set; } = new List<Contrato>();
}
