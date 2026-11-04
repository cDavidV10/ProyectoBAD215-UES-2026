using BEMO.Negocio;
using BEMOWeb.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BEMOWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly CN_DeptoUbicacion _departamentos;

        public HomeController(CN_DeptoUbicacion departamentos)
        {
            _departamentos = departamentos;
        }

        public IActionResult Index()
        {
            return View(_departamentos.listar());
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
