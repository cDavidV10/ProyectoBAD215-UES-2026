using BEMO.Negocio;
using Microsoft.AspNetCore.Mvc;

namespace PanelAdmin.Controllers;

public class HomeController : Controller
{
    private readonly ConexionNegocio _conexion;
    private readonly CN_DeptoUbicacion _departamentos;

    public HomeController(
        ConexionNegocio conexion,
        CN_DeptoUbicacion departamentos)
    {
        _conexion = conexion;
        _departamentos = departamentos;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.Conectado = await _conexion.Probar();
        ViewBag.Departamentos = _departamentos.listar();
        return View();
    }
}