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
    public IActionResult Index(int pagina = 1, string? busqueda = null, string? departamento = null, string? estado = null)
        {
            const int tamanoPagina = 5;
            pagina = Math.Max(1, pagina);
            var resumen = _empleados.ObtenerResumen();
            var resultado = _empleados.ListarPaginado(pagina, tamanoPagina, busqueda, departamento, estado);
            var totalRegistros = resultado.Total;
            var totalPaginas = Math.Max(1, (int)Math.Ceiling(totalRegistros / (double)tamanoPagina));
            if (pagina > totalPaginas)
            {
                pagina = totalPaginas;
                resultado = _empleados.ListarPaginado(pagina, tamanoPagina, busqueda, departamento, estado);
            }

            var empleadoModel = new Models.EmpleadoModel
            {
                EmpleadosActivos = resumen.Activos,
                EmpleadosInactivos = resumen.Inactivos,
                EmpleadosTotales = resumen.Total,
                Departamentos = _departamento.Listar(),
                ListaEmpleados = resultado.Empleados,
                PaginaActual = pagina,
                TotalPaginas = totalPaginas,
                TotalRegistros = totalRegistros,
                TamanoPagina = tamanoPagina,
                DepartamentoSeleccionado = departamento,
                EstadoSeleccionado = estado,
                Busqueda = busqueda
            };

            return View(empleadoModel);
        }

    public IActionResult Nuevo() => View();
}
