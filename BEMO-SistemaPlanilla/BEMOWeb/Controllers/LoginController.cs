using System.Security.Claims;
using BEMO.Datos;
using BEMO.Entidades;
using BEMO.Negocio;
using BEMOWeb.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace BEMOWeb.Controllers
{
    public class LoginController : Controller
    {
        private readonly CN_Login _loginNegocio;
        private readonly Conexion _conexion;

        public LoginController(CN_Login loginNegocio, Conexion conexion)
        {
            _loginNegocio = loginNegocio;
            _conexion = conexion;
        }

        private bool HayConexion()
        {
            try
            {
                using var conexionSql = new SqlConnection(_conexion.Cadena);
                conexionSql.Open();
                return true;
            }
            catch (SqlException)
            {
                return false;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();

            return RedirectToAction("Index", "Home", new { area = "" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel modelo)
        {
            ViewBag.Conectado = HayConexion();

            if (!ModelState.IsValid)
            {
                return View("~/Views/Home/Index.cshtml", modelo);
            }

            if (ViewBag.Conectado is not true)
            {
                modelo.Clave = string.Empty;
                ModelState.AddModelError(string.Empty, "No fue posible conectar con la base de datos.");
                return View("~/Views/Home/Index.cshtml", modelo);
            }

            Usuario? usuario;
            try
            {
                if (!_loginNegocio.ValidarCredenciales(
                        modelo.NombreUsuario,
                        modelo.Clave,
                        out usuario)
                    || usuario == null)
                {
                    modelo.Clave = string.Empty;
                    ModelState.AddModelError(string.Empty, "El usuario o la contraseña son incorrectos.");
                    return View("~/Views/Home/Index.cshtml", modelo);
                }
            }
            catch (SqlException ex)
            {
                modelo.Clave = string.Empty;
                var mensaje = ex.Message.Contains("revocado", StringComparison.OrdinalIgnoreCase)
                    ? "Acceso denegado: tu acceso ha sido revocado temporalmente."
                    : "El usuario o la contraseña son incorrectos.";
                ModelState.AddModelError(string.Empty, mensaje);
                return View("~/Views/Home/Index.cshtml", modelo);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
                new Claim(ClaimTypes.Name, usuario.NombreUsuario),
                new Claim(ClaimTypes.Role, usuario.Rol?.NombreRol ?? "Usuario"),
                new Claim("id_empleado", usuario.IdEmpleado.ToString()),
                new Claim("id_rol", usuario.IdRol.ToString())
            };

            HttpContext.Session.SetInt32("id_usuario", usuario.IdUsuario);
            HttpContext.Session.SetInt32("id_empleado", usuario.IdEmpleado);
            HttpContext.Session.SetInt32("id_rol", usuario.IdRol);

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
                });

            var rol = usuario.Rol?.NombreRol;
            Debug.WriteLine(rol);

            return rol switch
            {
                "ADMINISTRADOR NOMINA" => RedirectToAction("Index", "Home", new { area = "Admin" }),
                "RECURSOS HUMANOS"  => RedirectToAction("Index", "Home", new { area = "RRHH" }),
                _       => RedirectToAction("Privacy", "Home", new { area = "" })
            };


        }

        [HttpGet]
        public IActionResult ProbarHash()
        {
            string contraseñaTextoPlano = "123"; 
            string hashResultado;
            using (var sha256 = SHA256.Create())
            {
                hashResultado = Convert.ToHexString(
                    sha256.ComputeHash(Encoding.UTF8.GetBytes(contraseñaTextoPlano)));
            }

            // 📢 Esto imprimirá el resultado directamente en la consola de Visual Studio
            Debug.WriteLine("\n==================================================");
            Debug.WriteLine("=== GENERADOR DE HASHES BEMO (CONSOLA DE DEPURACIÓN) ===");
            Debug.WriteLine($"Contraseña original: {contraseñaTextoPlano}");
            Debug.WriteLine("HASH GENERADO (Copia la siguiente línea completa):");
            Debug.WriteLine(hashResultado);
            Debug.WriteLine("==================================================\n");

            // Mensaje simple para la web indicando dónde buscar
            return Content("¡Hash generado! Ve a Visual Studio, abre la ventana 'Salida' (Output) y selecciona 'Depurar' para copiar el código.");
        }
        
    }

    
}
