using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BEMO.Negocio;
using Negocio;

namespace BEMOWeb.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "ADMINISTRADOR NOMINA")]
public class EmpleadosController : Controller
{
     private readonly CN_Empleado _empleados;
     private readonly CN_Departamento _departamento;

        public EmpleadosController(CN_Empleado empleados, CN_Departamento departamento)
        {
            _empleados = empleados;
            _departamento = departamento;
        }
    public IActionResult Index()
        {
            var empleadoModel = new Models.EmpleadoModel
            {
                EmpleadosActivos = _empleados.UsuariosActivos(),
                EmpleadosInactivos = _empleados.EmpleadosInactivos(),
                EmpleadosTotales = _empleados.EmpladosTotales(),
                Departamentos = _departamento.Listar()
            };

            return View(empleadoModel);
        }

    public IActionResult Nuevo() => View();
}
