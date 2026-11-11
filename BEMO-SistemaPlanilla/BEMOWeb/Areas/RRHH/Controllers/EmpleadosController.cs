using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BEMOWeb.Areas.RRHH.Controllers;

[Area("RRHH")]
[Authorize(Roles = "RRHH")]
public class EmpleadosController : Controller
{
    public IActionResult Index() => View("~/Areas/Admin/Views/Empleados/Index.cshtml");

    public IActionResult Nuevo() => View("~/Areas/Admin/Views/Empleados/Nuevo.cshtml");
}
