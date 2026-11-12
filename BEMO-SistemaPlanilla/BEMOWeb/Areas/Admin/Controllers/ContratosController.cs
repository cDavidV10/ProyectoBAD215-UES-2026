using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BEMO.Negocio;

namespace BEMOWeb.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "ADMINISTRADOR NOMINA")]
public class ContratosController : Controller
{
    private readonly CN_Contrato _contratos;
    private readonly CN_TipoContrato _tiposContrato;

    public ContratosController(CN_Contrato contratos, CN_TipoContrato tiposContrato)
    {
        _contratos = contratos;
        _tiposContrato = tiposContrato;
    }

    public IActionResult Index(
        int pagina = 1,
        string? busqueda = null,
        int? empleado = null,
        string? tipo = null,
        string? estado = null)
    {
        const int tamanoPagina = 5;
        pagina = Math.Max(1, pagina);
        var todosLosContratos = _contratos.ListarVigentes();
        var resultado = _contratos.ListarPaginado(
            pagina,
            tamanoPagina,
            busqueda,
            empleado,
            tipo,
            estado);
        var totalRegistros = resultado.Total;
        var totalPaginas = Math.Max(1, (int)Math.Ceiling(totalRegistros / (double)tamanoPagina));

        if (pagina > totalPaginas)
        {
            pagina = totalPaginas;
            resultado = _contratos.ListarPaginado(
                pagina,
                tamanoPagina,
                busqueda,
                empleado,
                tipo,
                estado);
        }

        var modelo = new Models.ContratosModel
        {
            ContratosVigentes = todosLosContratos.Count,
            ContratosPorVencer = todosLosContratos.Count(contrato => contrato.Situacion == "POR VENCER"),
            Contratos = resultado.Contratos,
            Empleados = todosLosContratos
                .GroupBy(contrato => new { contrato.IdEmpleado, contrato.NombreEmpleado })
                .OrderBy(grupo => grupo.Key.NombreEmpleado)
                .Select(grupo => new Models.ContratoEmpleadoOption
                {
                    IdEmpleado = grupo.Key.IdEmpleado,
                    Nombre = grupo.Key.NombreEmpleado
                })
                .ToList(),
            TiposContrato = _tiposContrato.Listar()
                .Select(tipoContrato => tipoContrato.NombreTipoContrato)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(tipoContrato => tipoContrato)
                .ToList(),
            EmpleadoSeleccionado = empleado,
            TipoSeleccionado = tipo,
            EstadoSeleccionado = estado,
            Busqueda = busqueda,
            PaginaActual = pagina,
            TotalPaginas = totalPaginas,
            TotalRegistros = totalRegistros,
            TamanoPagina = tamanoPagina
        };

        return View(modelo);
    }

    public IActionResult Nuevo() => View();
}
