using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BEMOWeb.Areas.RRHH.Controllers;

[Area("RRHH")]
[Authorize(Roles = "RECURSOS HUMANOS")]
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

        return View("~/Areas/Admin/Views/Empleados/Index.cshtml", modelo);
    }

    public IActionResult Nuevo() => View("~/Areas/Admin/Views/Empleados/Nuevo.cshtml");
}
