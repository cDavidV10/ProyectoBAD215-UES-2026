using System.ComponentModel.DataAnnotations;
using BEMO.Entidades;

namespace BEMOWeb.Models;

public class NuevoEmpleadoModel
{
    public Empleado Empleado { get; set; } = new()
    {
        Afp = new Afp(),
        Direccion = new Direccion
        {
            Distrito = new DistritoUbicacion()
        }
    };

    public int? IdDepartamentoUbicacion { get; set; }

    public int? IdMunicipio { get; set; }

    public int? IdCargo { get; set; }

    public int? IdDepartamento { get; set; }

    public int? IdTipoContrato { get; set; }

    public DateOnly? FechaContratacion { get; set; }

    public DateOnly? FechaCulminacion { get; set; }

    [Required(ErrorMessage = "La calle es obligatoria.")]
    [StringLength(50, ErrorMessage = "La calle no puede superar 50 caracteres.")]
    [RegularExpression(@"^[\p{L}\p{N}][\p{L}\p{N} .,#/\-]*$", ErrorMessage = "La calle contiene caracteres no válidos.")]
    public string NombreCalle { get; set; } = string.Empty;

    [Required(ErrorMessage = "El número de casa es obligatorio.")]
    [StringLength(10, ErrorMessage = "El número de casa no puede superar 10 caracteres.")]
    [RegularExpression(@"^[\p{L}\p{N}][\p{L}\p{N} .#/\-]*$", ErrorMessage = "El número de casa contiene caracteres no válidos.")]
    public string NumeroCasa { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.01", "99999999.99", ErrorMessage = "El salario debe ser mayor que cero.")]
    public decimal? SalarioBruto { get; set; }

    public List<DepartamentoUbicacion> DepartamentosUbicacion { get; set; } = new();

    public List<MunicipioUbicacion> Municipios { get; set; } = new();

    public List<Afp> Afps { get; set; } = new();

    public List<Departamento> Departamentos { get; set; } = new();

    public List<Cargo> Cargos { get; set; } = new();

    public List<TipoContrato> TiposContrato { get; set; } = new();
}
