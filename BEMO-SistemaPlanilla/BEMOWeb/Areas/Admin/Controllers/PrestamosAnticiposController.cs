using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BEMOWeb.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "ADMINISTRADOR")]
public class PrestamosAnticiposController : Controller
{
    public IActionResult Index() => View();

    public IActionResult NuevoPrestamo() => View();

    public IActionResult RevisarPrestamo() => View();

    public IActionResult NuevoAnticipo() => View();
}
