using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BEMO.Entidades;

[Table("EMPLEADO", Schema = "dbo")]
public class Empleado
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id_empleado")]
    public int IdEmpleado { get; set; }

    [Column("id_afp")]
    public int IdAfp { get; set; }

    [Column("id_direccion")]
    public int IdDireccion { get; set; }

    [Required]
    [StringLength(10)]
    [Column("dui", TypeName = "char(10)")]
    public string Dui { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    [Column("nombre")]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    [Column("apellido")]
    public string Apellido { get; set; } = string.Empty;

    [StringLength(17)]
    [Column("nit")]
    public string? Nit { get; set; }

    [Required]
    [StringLength(15)]
    [Column("numero_isss")]
    public string NumeroIsss { get; set; } = string.Empty;

    [Required]
    [StringLength(12)]
    [Column("telefono")]
    public string Telefono { get; set; } = string.Empty;

    [StringLength(100)]
    [Column("correo")]
    public string? Correo { get; set; }

    [Column("fecha_nac")]
    public DateOnly FechaNac { get; set; }

    [Required]
    [StringLength(20)]
    [Column("estado")]
    public string Estado { get; set; } = "ACTIVO";

    [ForeignKey(nameof(IdAfp))]
    public Afp Afp { get; set; } = null!;

    [ForeignKey(nameof(IdDireccion))]
    public Direccion Direccion { get; set; } = null!;

    public Usuario? Usuario { get; set; }
    public ICollection<Contrato> Contratos { get; set; } = new List<Contrato>();
    public ICollection<Prestamo> Prestamos { get; set; } = new List<Prestamo>();
    public ICollection<AnticipoSalario> AnticiposSalario { get; set; } = new List<AnticipoSalario>();
    public ICollection<DetalleCapacitacion> DetallesCapacitacion { get; set; } = new List<DetalleCapacitacion>();
}
