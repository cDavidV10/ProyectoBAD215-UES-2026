using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BEMOWeb.Areas.RRHH.Controllers;

[Area("RRHH")]
[Authorize(Roles = "RRHH")]
public class PrestamosAnticiposController : Controller
{
    public IActionResult Index() => View("~/Areas/Admin/Views/PrestamosAnticipos/Index.cshtml");

    public IActionResult NuevoPrestamo() => View("~/Areas/Admin/Views/PrestamosAnticipos/NuevoPrestamo.cshtml");

    public IActionResult RevisarPrestamo() => View("~/Areas/Admin/Views/PrestamosAnticipos/RevisarPrestamo.cshtml");

    public IActionResult NuevoAnticipo() => View("~/Areas/Admin/Views/PrestamosAnticipos/NuevoAnticipo.cshtml");
}
