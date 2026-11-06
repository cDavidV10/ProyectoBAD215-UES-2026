using BEMOWeb.Models;
using Microsoft.AspNetCore.Mvc;

namespace BEMOWeb.Controllers
{
    public class LoginController : Controller
    {
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel modelo)
        {
            if (!ModelState.IsValid)
            {
                return View("~/Views/Home/Index.cshtml", modelo);
            }

            return RedirectToAction("Privacy", "Home");
        }
    }
}
