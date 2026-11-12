using System.ComponentModel.DataAnnotations;
using BEMO.Entidades;

namespace BEMOWeb.Models;

public class EditarContratoModel
{
    public int IdContrato { get; set; }

    public int IdEmpleado { get; set; }

    public string NombreEmpleado { get; set; } = string.Empty;

    [Required(ErrorMessage = "Seleccione el departamento laboral.")]
    public int? IdDepartamento { get; set; }

    [Required(ErrorMessage = "Seleccione el cargo.")]
    public int? IdCargo { get; set; }

    [Required(ErrorMessage = "Seleccione el tipo de contrato.")]
    public int? IdTipoContrato { get; set; }

    [Required(ErrorMessage = "Ingrese el salario bruto.")]
    [Range(typeof(decimal), "0.01", "99999999.99", ErrorMessage = "Ingrese un salario válido.")]
    public decimal? SalarioBruto { get; set; }

    [Required(ErrorMessage = "Ingrese la fecha de contratación.")]
    public DateOnly? FechaContratacion { get; set; }

    public DateOnly? FechaCulminacion { get; set; }

    public List<Departamento> Departamentos { get; set; } = new();

    public List<Cargo> Cargos { get; set; } = new();

    public List<TipoContrato> TiposContrato { get; set; } = new();
}
