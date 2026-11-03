using BEMO.Datos;
using Microsoft.AspNetCore.Mvc;

namespace PanelAdmin.Controllers;

public class HomeController : Controller
{
    private readonly BemoContext _contexto;

    public HomeController(BemoContext contexto)
    {
        _contexto = contexto;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.Conectado = await _contexto.Database.CanConnectAsync();
        return View();
    }
}