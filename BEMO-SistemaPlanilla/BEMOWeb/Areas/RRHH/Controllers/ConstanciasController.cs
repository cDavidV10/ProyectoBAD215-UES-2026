using BEMO.Negocio;
using BEMOWeb.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BEMOWeb.Areas.RRHH.Controllers;

[Area("RRHH")]
[Authorize(Roles = "RECURSOS HUMANOS")]
public class ConstanciasController : ConstanciasControllerBase
{
    public ConstanciasController(CN_ConstanciaSalarial constancias)
        : base(constancias)
    {
    }

    [HttpGet]
    public IActionResult Index(string? busqueda = null, int pagina = 1) => IndexCore(busqueda, pagina);

    [HttpGet]
    public IActionResult Generar(int idEmpleado, int meses = 3, DateOnly? fechaEmision = null) =>
        GenerarCore(idEmpleado, meses, fechaEmision);
}
