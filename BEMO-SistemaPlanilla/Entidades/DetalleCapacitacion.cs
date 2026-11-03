using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BEMO.Entidades;

[Table("DETALLE_CAPACITACION", Schema = "dbo")]
public class DetalleCapacitacion
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id_detalle_capacitacion")]
    public int IdDetalleCapacitacion { get; set; }

    [Column("id_empleado")]
    public int IdEmpleado { get; set; }

    [Column("id_capacitacion")]
    public int IdCapacitacion { get; set; }

    [Column("fecha_inicio")]
    public DateOnly FechaInicio { get; set; }

    [Column("fecha_fin")]
    public DateOnly FechaFin { get; set; }

    [Column("resultado", TypeName = "decimal(5,2)")]
    public decimal Resultado { get; set; }

    [ForeignKey(nameof(IdEmpleado))]
    public Empleado Empleado { get; set; } = null!;
    [ForeignKey(nameof(IdCapacitacion))]
    public Capacitacion Capacitacion { get; set; } = null!;
}
