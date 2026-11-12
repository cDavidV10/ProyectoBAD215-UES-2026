using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BEMO.Entidades;

[Table("USUARIO", Schema = "dbo")]
public class Usuario
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id_usuario")]
    public int IdUsuario { get; set; }

    [Column("id_empleado")]
    public int IdEmpleado { get; set; }

    [Column("id_rol")]
    public int IdRol { get; set; }

    [Required]
    [StringLength(50)]
    [Column("nombre_usuario")]
    public string NombreUsuario { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    [Column("password_hash")]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    [Column("estado")]
    public string Estado { get; set; } = "ACTIVO";

    [ForeignKey(nameof(IdEmpleado))]
    public Empleado Empleado { get; set; } = null!;

    [ForeignKey(nameof(IdRol))]
    public Rol Rol { get; set; } = null!;
}
