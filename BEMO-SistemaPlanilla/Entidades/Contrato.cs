using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BEMO.Entidades;

[Table("CONTRATO", Schema = "dbo")]
public class Contrato
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id_contrato")]
    public int IdContrato { get; set; }

    [Column("id_empleado")]
    public int IdEmpleado { get; set; }

    [Column("id_cargo")]
    public int IdCargo { get; set; }

    [Column("id_departamento")]
    public int IdDepartamento { get; set; }

    [Column("id_tipo_contrato")]
    public int IdTipoContrato { get; set; }

    [Column("salario_bruto", TypeName = "decimal(10,2)")]
    public decimal SalarioBruto { get; set; }

    [Column("fecha_contratacion")]
    public DateOnly FechaContratacion { get; set; }

    [Column("fecha_culminacion")]
    public DateOnly? FechaCulminacion { get; set; }

    [Required]
    [StringLength(20)]
    [Column("estado")]
    public string Estado { get; set; } = "VIGENTE";

    [ForeignKey(nameof(IdEmpleado))]
    public Empleado Empleado { get; set; } = null!;
    [ForeignKey(nameof(IdCargo))]
    public Cargo Cargo { get; set; } = null!;
    [ForeignKey(nameof(IdDepartamento))]
    public Departamento Departamento { get; set; } = null!;
    [ForeignKey(nameof(IdTipoContrato))]
    public TipoContrato TipoContrato { get; set; } = null!;
    public ICollection<DetallePlanilla> DetallesPlanilla { get; set; } = new List<DetallePlanilla>();
}
