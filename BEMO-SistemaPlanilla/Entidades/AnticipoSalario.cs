using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BEMO.Entidades;

[Table("ANTICIPO_SALARIO", Schema = "dbo")]
public class AnticipoSalario
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id_anticipo")]
    public int IdAnticipo { get; set; }

    [Column("porcentaje_anticipo", TypeName = "decimal(5,2)")]
    public decimal PorcentajeAnticipo { get; set; }

    [Column("fecha_anticipo")]
    public DateOnly FechaAnticipo { get; set; }

    public ICollection<DetallePlanilla> DetallesPlanilla { get; set; } = new List<DetallePlanilla>();
}
