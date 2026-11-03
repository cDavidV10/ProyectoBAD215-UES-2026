using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BEMO.Entidades;

[Table("AUDITORIA", Schema = "dbo")]
public class Auditoria
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id_auditoria")]
    public int IdAuditoria { get; set; }

    [Column("codigo_empleado")]
    public int CodigoEmpleado { get; set; }

    [Column("fecha_aplicacion")]
    public DateOnly FechaAplicacion { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [Required]
    [StringLength(30)]
    [Column("campo_modificado")]
    public string CampoModificado { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    [Column("valor_viejo")]
    public string ValorViejo { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    [Column("valor_nuevo")]
    public string ValorNuevo { get; set; } = string.Empty;

    [Column("registro_afectado")]
    public int RegistroAfectado { get; set; }

    [Required]
    [StringLength(100)]
    [Column("accion")]
    public string Accion { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    [Column("entidad_afectada")]
    public string EntidadAfectada { get; set; } = string.Empty;
}
