using System.Security.Claims;
using BEMO.Datos;
using BEMO.Negocio;
using BEMOWeb.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Diagnostics;

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

            if (!_loginNegocio.ValidarCredenciales(
                    modelo.NombreUsuario,
                    modelo.Clave,
                    out var usuario,
                    out var hashInvalido)
                || usuario == null)
            {
              ModelState.AddModelError(
                    string.Empty,
                    hashInvalido
                        ? "El hash de la contraseña de este usuario no es válido. Contacta al administrador."
                        : "No se encontró el usuario o la contraseña es incorrecta.");
               
                modelo.Clave = string.Empty;
                return View("~/Views/Home/Index.cshtml", modelo);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
                new Claim(ClaimTypes.Name, usuario.NombreUsuario),
                new Claim(ClaimTypes.Role, usuario.Rol?.NombreRol ?? "Usuario")
            };

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

            return rol switch
            {
                "Administrador" => RedirectToAction("Index", "Home", new { area = "Admin" }),
                "RRHH"  => RedirectToAction("Index", "Home", new { area = "RRHH" }),
                _       => RedirectToAction("Privacy", "Home", new { area = "" })
            };


        }

        [HttpGet]
        public IActionResult ProbarHash()
        {
            var passwordHasher = new Microsoft.AspNetCore.Identity.PasswordHasher<string>();

            string contraseñaTextoPlano = "123"; 
            string usuarioSimulado = "admin";
            string hashResultado = passwordHasher.HashPassword(usuarioSimulado, contraseñaTextoPlano);

            // 📢 Esto imprimirá el resultado directamente en la consola de Visual Studio
            Debug.WriteLine("\n==================================================");
            Debug.WriteLine("=== GENERADOR DE HASHES BEMO (CONSOLA DE DEPURACIÓN) ===");
            Debug.WriteLine($"Usuario: {usuarioSimulado}");
            Debug.WriteLine($"Contraseña original: {contraseñaTextoPlano}");
            Debug.WriteLine("HASH GENERADO (Copia la siguiente línea completa):");
            Debug.WriteLine(hashResultado);
            Debug.WriteLine("==================================================\n");

            // Mensaje simple para la web indicando dónde buscar
            return Content("¡Hash generado! Ve a Visual Studio, abre la ventana 'Salida' (Output) y selecciona 'Depurar' para copiar el código.");
        }
        
    }

    
}