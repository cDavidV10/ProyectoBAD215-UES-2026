using BEMOWeb.Models;
using Microsoft.AspNetCore.Mvc;
using BEMO.Datos;
using System.Diagnostics;
using Microsoft.Data.SqlClient;

namespace BEMOWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly Conexion _conexion;

        public HomeController(Conexion conexion)
        {
            _conexion = conexion;
        }

        public IActionResult Index()
        {
            try
            {
                using var conexionSql = new SqlConnection(_conexion.Cadena);
                conexionSql.Open();
                ViewBag.Conectado = true;
            }
            catch (SqlException)
            {
                ViewBag.Conectado = false;
            }

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
