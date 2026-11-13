using System.ComponentModel.DataAnnotations;
using BEMO.Entidades;

namespace BEMOWeb.Models;

public class EditarEmpleadoModel
{
    public int IdEmpleado { get; set; }

    public string Dui { get; set; } = string.Empty;

    public DateOnly FechaNac { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(50, ErrorMessage = "El nombre no puede superar 50 caracteres.")]
    [RegularExpression(@"^[\p{L}]+(?:[ '\-][\p{L}]+)*$", ErrorMessage = "El nombre solo puede contener letras, espacios, apóstrofes o guiones.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es obligatorio.")]
    [StringLength(20, ErrorMessage = "El apellido no puede superar 20 caracteres.")]
    [RegularExpression(@"^[\p{L}]+(?:[ '\-][\p{L}]+)*$", ErrorMessage = "El apellido solo puede contener letras, espacios, apóstrofes o guiones.")]
    public string Apellido { get; set; } = string.Empty;

    [StringLength(17, ErrorMessage = "El NIT no puede superar 17 caracteres.")]
    [RegularExpression(@"^(?:\d{4}-\d{6}-\d{3}-\d|\d{8}-\d)$", ErrorMessage = "El NIT debe tener el formato 0000-000000-000-0 o 00000000-0.")]
    public string? Nit { get; set; }

    [StringLength(15, ErrorMessage = "El ISSS no puede superar 15 caracteres.")]
    [RegularExpression(@"^\d{1,15}$", ErrorMessage = "El ISSS solo puede contener entre 1 y 15 dígitos.")]
    public string NumeroIsss { get; set; } = string.Empty;

    [Required(ErrorMessage = "El teléfono es obligatorio.")]
    [StringLength(12, ErrorMessage = "El teléfono no puede superar 12 caracteres.")]
    [RegularExpression(@"^\d{4}-\d{4}$", ErrorMessage = "El teléfono debe tener el formato 0000-0000.")]
    public string Telefono { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "El correo no puede superar 100 caracteres.")]
    [EmailAddress(ErrorMessage = "Ingrese un correo electrónico válido.")]
    public string? Correo { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Seleccione una AFP.")]
    public int IdAfp { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Seleccione un departamento de ubicación.")]
    public int IdDepartamentoUbicacion { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Seleccione un municipio.")]
    public int IdMunicipio { get; set; }

    [Required(ErrorMessage = "La calle es obligatoria.")]
    [StringLength(50, ErrorMessage = "La calle no puede superar 50 caracteres.")]
    [RegularExpression(@"^[\p{L}\p{N}][\p{L}\p{N} .,#/\-]*$", ErrorMessage = "La calle contiene caracteres no válidos.")]
    public string NombreCalle { get; set; } = string.Empty;

    [Required(ErrorMessage = "El número de casa es obligatorio.")]
    [StringLength(10, ErrorMessage = "El número de casa no puede superar 10 caracteres.")]
    [RegularExpression(@"^[\p{L}\p{N}][\p{L}\p{N} .#/\-]*$", ErrorMessage = "El número de casa contiene caracteres no válidos.")]
    public string NumeroCasa { get; set; } = string.Empty;

    [Required(ErrorMessage = "Seleccione un estado.")]
    [RegularExpression("^(ACTIVO|INACTIVO)$", ErrorMessage = "El estado seleccionado no es válido.")]
    public string Estado { get; set; } = "ACTIVO";

    public List<DepartamentoUbicacion> DepartamentosUbicacion { get; set; } = new();

    public List<MunicipioUbicacion> Municipios { get; set; } = new();

    public List<Afp> Afps { get; set; } = new();
}
