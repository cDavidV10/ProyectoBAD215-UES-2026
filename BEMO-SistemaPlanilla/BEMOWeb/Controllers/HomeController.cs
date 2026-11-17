using BEMOWeb.Models;
using Microsoft.AspNetCore.Mvc;
using BEMO.Datos;
using System.Diagnostics;

namespace BEMOWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly Conexion _conexion;
        private readonly IWebHostEnvironment _entorno;

        public HomeController(Conexion conexion, IWebHostEnvironment entorno)
        {
            _conexion = conexion;
            _entorno = entorno;
        }

        public IActionResult Index()
        {
            ConexionUiHelper.CargarEstadoConexion(this, _conexion, _entorno);
            return View(new LoginViewModel());
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
