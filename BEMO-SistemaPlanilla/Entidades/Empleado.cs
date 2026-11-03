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

    [Column("id_municipio")]
    public int IdMunicipio { get; set; }

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

    [StringLength(10)]
    [Column("nit")]
    public string? Nit { get; set; }

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

    [ForeignKey(nameof(IdMunicipio))]
    public MunicipioUbicacion Municipio { get; set; } = null!;
    public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
    public ICollection<Contrato> Contratos { get; set; } = new List<Contrato>();
    public ICollection<Prestamo> Prestamos { get; set; } = new List<Prestamo>();
    public ICollection<DescuentoLey> DescuentosLey { get; set; } = new List<DescuentoLey>();
    public ICollection<DetalleCapacitacion> DetallesCapacitacion { get; set; } = new List<DetalleCapacitacion>();
}
