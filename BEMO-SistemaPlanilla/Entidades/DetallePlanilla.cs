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

    [Column("id_contrato")]
    public int IdContrato { get; set; }

    [Column("id_renta")]
    public int IdRenta { get; set; }

    [Column("dias_trabajados")]
    public int DiasTrabajados { get; set; }

    [Column("dias_no_trabajados")]
    public int DiasNoTrabajados { get; set; }

    [Column("horas_extras")]
    public int HorasExtras { get; set; }

    [Column("salario_mensual", TypeName = "decimal(10,2)")]
    public decimal SalarioMensual { get; set; }

    [Column("salario_quincenal", TypeName = "decimal(10,2)")]
    public decimal SalarioQuincenal { get; set; }

    [Column("bonificaciones", TypeName = "decimal(10,2)")]
    public decimal Bonificaciones { get; set; }

    [Column("aguinaldo", TypeName = "decimal(10,2)")]
    public decimal Aguinaldo { get; set; }

    [Column("total_bruto", TypeName = "decimal(10,2)")]
    public decimal TotalBruto { get; set; }

    [Column("monto_renta", TypeName = "decimal(10,2)")]
    public decimal MontoRenta { get; set; }

    [Column("descuentos_prestamos", TypeName = "decimal(10,2)")]
    public decimal DescuentosPrestamos { get; set; }

    [Column("descuentos_anticipos", TypeName = "decimal(10,2)")]
    public decimal DescuentosAnticipos { get; set; }

    [Column("total_deducciones", TypeName = "decimal(10,2)")]
    public decimal TotalDeducciones { get; set; }

    [Column("salario_liquido", TypeName = "decimal(10,2)")]
    public decimal SalarioLiquido { get; set; }

    [Required]
    [StringLength(20)]
    [Column("estado")]
    public string Estado { get; set; } = "PENDIENTE";

    [ForeignKey(nameof(IdPlanilla))]
    public Planilla Planilla { get; set; } = null!;

    [ForeignKey(nameof(IdContrato))]
    public Contrato Contrato { get; set; } = null!;

    [ForeignKey(nameof(IdRenta))]
    public Renta Renta { get; set; } = null!;

    public ICollection<DetalleDescuentoPlanilla> DetallesDescuentoPlanilla { get; set; } = new List<DetalleDescuentoPlanilla>();
}
