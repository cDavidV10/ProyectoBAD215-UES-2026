using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BEMOWeb.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Administrador")]
public class EmpleadosController : Controller
{
    public IActionResult Index() => View();

    public IActionResult Nuevo() => View();
}
