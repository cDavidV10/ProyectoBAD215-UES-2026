using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BEMO.Entidades;

[Table("DETALLE_PLANILLA", Schema = "dbo")]
public class DetallePlanilla
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id_detalle_planilla")]
    public int IdDetallePlanilla { get; set; }

    [Column("id_planilla")]
    public int IdPlanilla { get; set; }

    [Required]
    [StringLength(20)]
    [Column("codigo_renta")]
    public string CodigoRenta { get; set; } = string.Empty;

    [Column("id_anticipo")]
    public int? IdAnticipo { get; set; }

    [Column("dias_trabajados")]
    public int DiasTrabajados { get; set; }

    [Column("dias_no_trabajados")]
    public int DiasNoTrabajados { get; set; }

    [Column("horas_extras")]
    public int HorasExtras { get; set; }

    [Column("bonificaciones", TypeName = "decimal(10,2)")]
    public decimal Bonificaciones { get; set; }

    [Column("aguinaldo", TypeName = "decimal(10,2)")]
    public decimal Aguinaldo { get; set; }

    [Column("afp", TypeName = "decimal(10,2)")]
    public decimal Afp { get; set; }

    [Column("isss", TypeName = "decimal(10,2)")]
    public decimal Isss { get; set; }

    [Column("renta", TypeName = "decimal(10,2)")]
    public decimal MontoRenta { get; set; }

    [Column("total_descuentos", TypeName = "decimal(10,2)")]
    public decimal TotalDescuentos { get; set; }

    [Column("valor_cuota_prestamo", TypeName = "decimal(10,2)")]
    public decimal ValorCuotaPrestamo { get; set; }

    [ForeignKey(nameof(IdPlanilla))]
    public Planilla Planilla { get; set; } = null!;
    [ForeignKey(nameof(CodigoRenta))]
    public Renta RentaAplicada { get; set; } = null!;
    [ForeignKey(nameof(IdAnticipo))]
    public AnticipoSalario? Anticipo { get; set; }
}
