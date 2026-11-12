using System.ComponentModel.DataAnnotations;
using BEMO.Entidades;

namespace BEMOWeb.Models;

public class NuevoContratoModel
{
    [Required(ErrorMessage = "Seleccione un empleado.")]
    public int? IdEmpleado { get; set; }

    [Required(ErrorMessage = "Ingrese el DUI del empleado.")]
    [StringLength(10, ErrorMessage = "El DUI debe tener el formato 00000000-0.")]
    [RegularExpression(@"^\d{8}-\d$", ErrorMessage = "El DUI debe tener el formato 00000000-0.")]
    public string DuiBusqueda { get; set; } = string.Empty;

    [Required(ErrorMessage = "Seleccione el departamento laboral.")]
    public int? IdDepartamento { get; set; }

    [Required(ErrorMessage = "Seleccione el cargo.")]
    public int? IdCargo { get; set; }

    [Required(ErrorMessage = "Seleccione el tipo de contrato.")]
    public int? IdTipoContrato { get; set; }

    [Required(ErrorMessage = "Ingrese el salario bruto.")]
    [Range(typeof(decimal), "0.01", "99999999.99", ErrorMessage = "Ingrese un salario bruto válido.")]
    public decimal? SalarioBruto { get; set; }

    [Required(ErrorMessage = "Ingrese la fecha de contratación.")]
    public DateOnly? FechaContratacion { get; set; }

    public DateOnly? FechaCulminacion { get; set; }

    public List<ContratoEmpleadoDisponibleOption> EmpleadosDisponibles { get; set; } = new();

    public List<Departamento> Departamentos { get; set; } = new();

    public List<Cargo> Cargos { get; set; } = new();

    public List<TipoContrato> TiposContrato { get; set; } = new();
}

public class ContratoEmpleadoDisponibleOption
{
    public int IdEmpleado { get; init; }

    public string Nombre { get; init; } = string.Empty;

    public string Dui { get; init; } = string.Empty;
}
