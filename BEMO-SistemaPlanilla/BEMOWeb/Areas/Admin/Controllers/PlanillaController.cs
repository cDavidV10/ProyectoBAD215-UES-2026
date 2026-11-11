using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BEMOWeb.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "ADMINISTRADOR NOMINA")]
public class PlanillaController : Controller
{
    public IActionResult Index() => View();

    public IActionResult Calcular() => View();

    public IActionResult Cerrar() => View();
}
