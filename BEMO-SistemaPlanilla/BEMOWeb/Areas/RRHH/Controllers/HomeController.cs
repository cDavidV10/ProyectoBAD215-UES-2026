using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BEMOWeb.Areas.RRHH.Controllers
{
    [Area("RRHH")]
    [Authorize(Roles = "RRHH")]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}