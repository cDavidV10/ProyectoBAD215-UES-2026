using BEMO.Entidades;
using System.ComponentModel.DataAnnotations;

namespace BEMOWeb.Models;

public class NuevoPrestamoModel
{
    [Required(ErrorMessage = "Seleccione un empleado.")]
    public int? IdEmpleado { get; set; }

    [Required(ErrorMessage = "Indique la fecha de otorgamiento.")]
    [DataType(DataType.Date)]
    public DateOnly FechaOtorgamiento { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [Required(ErrorMessage = "Indique el monto del préstamo.")]
    [Range(typeof(decimal), "0.01", "1200", ErrorMessage = "El monto debe estar entre $0.01 y $1,200.00.")]
    public decimal Monto { get; set; }

    public List<Empleado> Empleados { get; set; } = new();
    public string? Busqueda { get; set; }
    public int PaginaActual { get; set; } = 1;
    public int TotalPaginas { get; set; } = 1;
    public int TotalRegistros { get; set; }
    public int TamanoPagina { get; set; } = 5;
}
