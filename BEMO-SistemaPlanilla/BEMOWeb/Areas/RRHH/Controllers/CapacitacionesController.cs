using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BEMOWeb.Areas.RRHH.Controllers;

[Area("RRHH")]
[Authorize(Roles = "RECURSOS HUMANOS")]
public class CapacitacionesController : Controller
{
    public IActionResult Index() => View("~/Areas/Admin/Views/Capacitaciones/Index.cshtml");

    public IActionResult Nueva() => View("~/Areas/Admin/Views/Capacitaciones/Nueva.cshtml");

    public IActionResult Participantes() => View("~/Areas/Admin/Views/Capacitaciones/Participantes.cshtml");
}
