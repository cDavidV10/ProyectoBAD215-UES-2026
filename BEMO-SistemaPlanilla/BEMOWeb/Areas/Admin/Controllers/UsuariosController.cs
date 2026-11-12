using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BEMOWeb.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "ADMINISTRADOR NOMINA")]
public class UsuariosController : Controller
{
    public IActionResult Index() => View();

    [HttpGet]
    public IActionResult Nuevo() => View("~/Areas/Admin/Views/Usuarios/Nuevo.cshtml");
}
