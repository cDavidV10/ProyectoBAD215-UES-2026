using System.ComponentModel.DataAnnotations;
using BEMO.Entidades;

namespace BEMOWeb.Models;

public class NuevoUsuarioModel
{
    [Required(ErrorMessage = "Seleccione un empleado.")]
    public int? IdEmpleado { get; set; }

    [Required(ErrorMessage = "Seleccione un rol.")]
    public int? IdRol { get; set; }

    [Required(ErrorMessage = "Ingrese una contraseña.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirme la contraseña.")]
    [Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden.")]
    [DataType(DataType.Password)]
    public string ConfirmarPassword { get; set; } = string.Empty;

    public List<Empleado> EmpleadosDisponibles { get; set; } = new();

    public List<Rol> Roles { get; set; } = new();
}
