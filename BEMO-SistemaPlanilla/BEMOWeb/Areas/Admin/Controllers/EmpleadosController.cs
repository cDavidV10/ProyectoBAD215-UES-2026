using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BEMOWeb.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "ADMINISTRADOR NOMINA")]
public class EmpleadosController : Controller
{
     private readonly BEMO.Negocio.CN_Empleado _empleados;

        public EmpleadosController(BEMO.Negocio.CN_Empleado empleados)
        {
            _empleados = empleados;
        }
    public IActionResult Index()
        {
            var modelo = new BEMOWeb.Models.AdminDashboardViewModel
            {
                EmpleadosActivos = _empleados.UsuariosActivos(),
                EmpleadosInactivos = _empleados.EmpleadosInactivos(),
                EmpleadosTotales = _empleados.EmpladosTotales()
            };

            return View(modelo);
        }

    public IActionResult Nuevo() => View();
}
