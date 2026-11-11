using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Negocio;

namespace BEMOWeb.Areas.RRHH.Controllers;

[Area("RRHH")]
[Authorize(Roles = "RECURSOS HUMANOS")]
public class EmpleadosController : Controller
{
    private readonly BEMO.Negocio.CN_Empleado _empleados;
    private readonly CN_Departamento _departamento;

    public EmpleadosController(BEMO.Negocio.CN_Empleado empleados, CN_Departamento departamento)
    {
        _empleados = empleados;
        _departamento = departamento;
    }

    public IActionResult Index()
    {
        var modelo = new BEMOWeb.Models.EmpleadoModel
        {
            EmpleadosActivos = _empleados.UsuariosActivos(),
            EmpleadosInactivos = _empleados.EmpleadosInactivos(),
            EmpleadosTotales = _empleados.EmpladosTotales(),
            Departamentos = _departamento.Listar()
        };

        return View("~/Areas/Admin/Views/Empleados/Index.cshtml", modelo);
    }

    public IActionResult Nuevo() => View("~/Areas/Admin/Views/Empleados/Nuevo.cshtml");
}
