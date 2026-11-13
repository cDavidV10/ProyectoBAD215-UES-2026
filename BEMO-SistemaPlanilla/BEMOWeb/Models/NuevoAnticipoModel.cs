using BEMO.Entidades;
using System.ComponentModel.DataAnnotations;

namespace BEMOWeb.Models;

public class NuevoAnticipoModel
{
    [Required(ErrorMessage = "Seleccione un empleado.")]
    public int? IdEmpleado { get; set; }

    [Required(ErrorMessage = "Indique la fecha del anticipo.")]
    [DataType(DataType.Date)]
    public DateOnly FechaAnticipo { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [Required(ErrorMessage = "Indique el monto del anticipo.")]
    [Range(typeof(decimal), "0.01", "99999999", ErrorMessage = "El monto debe ser mayor que cero.")]
    public decimal Monto { get; set; }

    public decimal? MaximoPermitido { get; set; }
    public List<Empleado> Empleados { get; set; } = new();
}
