using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BEMO.Entidades;

[Table("CARGO", Schema = "dbo")]
public class Cargo
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id_cargo")]
    public int IdCargo { get; set; }

    [Column("id_cargo_jefe")]
    public int? IdCargoJefe { get; set; }

    [Required]
    [StringLength(50)]
    [Column("nombre_cargo")]
    public string NombreCargo { get; set; } = string.Empty;

    [Column("rango_minimo_cargo", TypeName = "decimal(10,2)")]
    public decimal RangoMinimoCargo { get; set; }

    [Column("rango_maximo_cargo", TypeName = "decimal(10,2)")]
    public decimal RangoMaximoCargo { get; set; }

    [ForeignKey(nameof(IdCargoJefe))]
    [InverseProperty(nameof(Cargo.Subordinados))]
    public Cargo? CargoJefe { get; set; }
    [InverseProperty(nameof(Cargo.CargoJefe))]
    public ICollection<Cargo> Subordinados { get; set; } = new List<Cargo>();
    public ICollection<Contrato> Contratos { get; set; } = new List<Contrato>();
}
