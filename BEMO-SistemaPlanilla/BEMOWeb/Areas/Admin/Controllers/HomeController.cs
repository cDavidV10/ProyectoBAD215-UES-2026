using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BEMOWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "ADMINISTRADOR NOMINA")]
    public class HomeController : Controller
    {
        private readonly BEMO.Negocio.CN_Empleado _empleados;

        public HomeController(BEMO.Negocio.CN_Empleado empleados)
        {
            _empleados = empleados;
        }

        public IActionResult Index()
        {
            var modelo = new BEMOWeb.Models.AdminDashboardViewModel
            {
                EmpleadosActivos = _empleados.UsuariosActivos(),
                EmpleadosInactivos = _empleados.EmpleadosInactivos()
            };

            return View(modelo);
        }
    }
}
