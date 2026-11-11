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

    public IActionResult Index(int pagina = 1)
    {
        const int tamanoPagina = 10;
        var totalRegistros = _empleados.TotalEmpleadosGeneral();
        var totalPaginas = Math.Max(1, (int)Math.Ceiling(totalRegistros / (double)tamanoPagina));
        pagina = Math.Clamp(pagina, 1, totalPaginas);

        var modelo = new BEMOWeb.Models.EmpleadoModel
        {
            EmpleadosActivos = _empleados.UsuariosActivos(),
            EmpleadosInactivos = _empleados.EmpleadosInactivos(),
            EmpleadosTotales = _empleados.EmpladosTotales(),
            Departamentos = _departamento.Listar(),
            ListaEmpleados = _empleados.Listar(pagina, tamanoPagina),
            PaginaActual = pagina,
            TotalPaginas = totalPaginas,
            TotalRegistros = totalRegistros,
            TamanoPagina = tamanoPagina
        };

        return View("~/Areas/Admin/Views/Empleados/Index.cshtml", modelo);
    }

    public IActionResult Nuevo() => View("~/Areas/Admin/Views/Empleados/Nuevo.cshtml");
}
