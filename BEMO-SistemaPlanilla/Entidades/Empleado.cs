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

    [Required(ErrorMessage = "El DUI es obligatorio.")]
    [StringLength(10, ErrorMessage = "El DUI no puede superar 10 caracteres.")]
    [RegularExpression(@"^\d{8}-\d$", ErrorMessage = "El DUI debe tener el formato 00000000-0.")]
    [Column("dui", TypeName = "char(10)")]
    public string Dui { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(50, ErrorMessage = "El nombre no puede superar 50 caracteres.")]
    [RegularExpression(@"^[\p{L}]+(?:[ '\-][\p{L}]+)*$", ErrorMessage = "El nombre solo puede contener letras, espacios, apóstrofes o guiones.")]
    [Column("nombre")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es obligatorio.")]
    [StringLength(20, ErrorMessage = "El apellido no puede superar 20 caracteres.")]
    [RegularExpression(@"^[\p{L}]+(?:[ '\-][\p{L}]+)*$", ErrorMessage = "El apellido solo puede contener letras, espacios, apóstrofes o guiones.")]
    [Column("apellido")]
    public string Apellido { get; set; } = string.Empty;

    [StringLength(17, ErrorMessage = "El NIT no puede superar 17 caracteres.")]
    [RegularExpression(@"^(?:\d{4}-\d{6}-\d{3}-\d|\d{8}-\d)$", ErrorMessage = "El NIT debe tener el formato 0000-000000-000-0 o el formato de DUI 00000000-0.")]
    [Column("nit")]
    public string? Nit { get; set; }

    [Required(ErrorMessage = "El número ISSS es obligatorio.")]
    [StringLength(15, ErrorMessage = "El ISSS no puede superar 15 caracteres.")]
    [RegularExpression(@"^\d{1,15}$", ErrorMessage = "El ISSS solo puede contener entre 1 y 15 dígitos.")]
    [Column("numero_isss")]
    public string NumeroIsss { get; set; } = string.Empty;

    [Required(ErrorMessage = "El teléfono es obligatorio.")]
    [StringLength(12, ErrorMessage = "El teléfono no puede superar 12 caracteres.")]
    [RegularExpression(@"^\d{4}-\d{4}$", ErrorMessage = "El teléfono debe tener el formato 0000-0000.")]
    [Column("telefono")]
    public string Telefono { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "El correo no puede superar 100 caracteres.")]
    [EmailAddress(ErrorMessage = "Ingrese un correo electrónico válido.")]
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
