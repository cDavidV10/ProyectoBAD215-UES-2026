using BEMO.Entidades;
using System.ComponentModel.DataAnnotations;

namespace BEMOWeb.Models;

public class NuevaCapacitacionModel
{
    [Required(ErrorMessage = "Ingrese el nombre de la capacitación.")]
    [StringLength(50, ErrorMessage = "El nombre no puede superar 50 caracteres.")]
    public string NombreCapacitacion { get; set; } = string.Empty;

    [Required(ErrorMessage = "Seleccione un tipo de capacitación.")]
    public int? IdTipoCapacitacion { get; set; }

    public List<TipoCapacitacion> TiposCapacitacion { get; set; } = new();
}
