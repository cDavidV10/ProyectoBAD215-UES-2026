using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BEMO.Entidades;

[Table("PRESTAMO", Schema = "dbo")]
public class Prestamo
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id_prestamo")]
    public int IdPrestamo { get; set; }

    [Column("id_empleado")]
    public int IdEmpleado { get; set; }

    [Column("fecha_otorgamiento")]
    public DateOnly FechaOtorgamiento { get; set; }

    [Column("fecha_finalizacion")]
    public DateOnly FechaFinalizacion { get; set; }

    [Column("monto", TypeName = "decimal(10,2)")]
    public decimal Monto { get; set; }

    [Required]
    [StringLength(20)]
    [Column("estado")]
    public string Estado { get; set; } = "ACTIVO";

    [Column("saldo_pendiente", TypeName = "decimal(10,2)")]
    public decimal SaldoPendiente { get; set; }

    [Column("num_cuotas")]
    public int NumCuotas { get; set; }

    [ForeignKey(nameof(IdEmpleado))]
    public Empleado Empleado { get; set; } = null!;
}
