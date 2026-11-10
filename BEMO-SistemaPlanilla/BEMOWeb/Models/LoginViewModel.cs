using System.ComponentModel.DataAnnotations;

namespace BEMOWeb.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Ingresa tu usuario")]
        public string NombreUsuario { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ingresa tu contraseña")]
        [DataType(DataType.Password)]
        public string Clave { get; set; } = string.Empty;
    }
}