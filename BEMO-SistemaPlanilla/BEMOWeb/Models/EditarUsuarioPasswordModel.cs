using System.ComponentModel.DataAnnotations;

namespace BEMOWeb.Models;

public class EditarUsuarioPasswordModel
{
    [Required]
    public int IdUsuario { get; set; }

    [Required(ErrorMessage = "Ingrese la nueva contrasena.")]
    [DataType(DataType.Password)]
    public string PasswordNueva { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirme la nueva contrasena.")]
    [Compare(nameof(PasswordNueva), ErrorMessage = "Las contrasenas no coinciden.")]
    [DataType(DataType.Password)]
    public string ConfirmarPassword { get; set; } = string.Empty;
}
