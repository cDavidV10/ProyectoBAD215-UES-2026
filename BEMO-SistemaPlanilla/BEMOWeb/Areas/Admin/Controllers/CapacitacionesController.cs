using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BEMOWeb.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "ADMINISTRADOR")]
public class CapacitacionesController : Controller
{
    public IActionResult Index() => View();

    public IActionResult Nueva() => View();

    public IActionResult Participantes() => View();
}
