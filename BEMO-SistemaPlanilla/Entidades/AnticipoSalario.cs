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

    [Column("id_empleado")]
    public int IdEmpleado { get; set; }

    [Column("monto_anticipo", TypeName = "decimal(10,2)")]
    public decimal MontoAnticipo { get; set; }

    [Column("fecha_anticipo")]
    public DateOnly FechaAnticipo { get; set; }

    [Required]
    [StringLength(20)]
    [Column("estado")]
    public string Estado { get; set; } = "PENDIENTE";

    [ForeignKey(nameof(IdEmpleado))]
    public Empleado Empleado { get; set; } = null!;
}
