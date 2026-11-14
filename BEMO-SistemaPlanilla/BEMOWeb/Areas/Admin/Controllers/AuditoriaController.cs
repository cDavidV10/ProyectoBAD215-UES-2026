using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BEMO.Negocio;

namespace BEMOWeb.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "ADMINISTRADOR")]
public class AuditoriaController : Controller
{
    private readonly CN_Auditoria _auditoria;

    public AuditoriaController(CN_Auditoria auditoria)
    {
        _auditoria = auditoria;
    }

    [HttpGet]
    public IActionResult Index(
        int pagina = 1,
        DateOnly? fechaDesde = null,
        DateOnly? fechaHasta = null,
        string? usuario = null,
        string? modulo = null,
        string? operacion = null,
        int? id = null)
    {
        const int tamanoPagina = 10;
        pagina = Math.Max(1, pagina);

        var usuarios = _auditoria.ListarUsuarios();
        var modulos = _auditoria.ListarModulos();
        var operaciones = _auditoria.ListarOperaciones();

        if (fechaDesde.HasValue && fechaHasta.HasValue && fechaHasta.Value < fechaDesde.Value)
        {
            return View(CrearModelo(
                new List<BEMO.Entidades.AuditoriaListado>(),
                0,
                1,
                tamanoPagina,
                fechaDesde,
                fechaHasta,
                usuario,
                modulo,
                operacion,
                id.HasValue ? _auditoria.ObtenerPorId(id.Value) : null,
                usuarios,
                modulos,
                operaciones,
                "La fecha final no puede ser anterior a la fecha inicial."));
        }

        var resultado = _auditoria.ListarPaginado(
            pagina,
            tamanoPagina,
            fechaDesde,
            fechaHasta,
            usuario,
            modulo,
            operacion);
        var totalPaginas = Math.Max(1, (int)Math.Ceiling(resultado.Total / (double)tamanoPagina));

        if (pagina > totalPaginas)
        {
            pagina = totalPaginas;
            resultado = _auditoria.ListarPaginado(
                pagina,
                tamanoPagina,
                fechaDesde,
                fechaHasta,
                usuario,
                modulo,
                operacion);
        }

        return View(CrearModelo(
            resultado.Auditorias,
            resultado.Total,
            pagina,
            tamanoPagina,
            fechaDesde,
            fechaHasta,
            usuario,
            modulo,
            operacion,
            id.HasValue ? _auditoria.ObtenerPorId(id.Value) : null,
            usuarios,
            modulos,
            operaciones,
            null));
    }

    private static Models.AuditoriaIndexModel CrearModelo(
        List<BEMO.Entidades.AuditoriaListado> auditorias,
        int total,
        int pagina,
        int tamanoPagina,
        DateOnly? fechaDesde,
        DateOnly? fechaHasta,
        string? usuario,
        string? modulo,
        string? operacion,
        BEMO.Entidades.AuditoriaListado? auditoriaSeleccionada,
        List<string> usuarios,
        List<string> modulos,
        List<string> operaciones,
        string? error)
    {
        return new Models.AuditoriaIndexModel
        {
            Auditorias = auditorias,
            TotalRegistros = total,
            PaginaActual = pagina,
            TotalPaginas = Math.Max(1, (int)Math.Ceiling(total / (double)tamanoPagina)),
            TamanoPagina = tamanoPagina,
            FechaDesde = fechaDesde,
            FechaHasta = fechaHasta,
            UsuarioSeleccionado = usuario,
            ModuloSeleccionado = modulo,
            OperacionSeleccionada = operacion,
            AuditoriaSeleccionada = auditoriaSeleccionada,
            Usuarios = usuarios,
            Modulos = modulos,
            Operaciones = operaciones,
            Error = error
        };
    }
}
