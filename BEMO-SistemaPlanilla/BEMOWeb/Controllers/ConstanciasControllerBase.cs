using BEMO.Negocio;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace BEMOWeb.Controllers;

public abstract class ConstanciasControllerBase : Controller
{
    private const string VistasConstancias = "~/Areas/Admin/Views/Constancias/";
    private readonly CN_ConstanciaSalarial _constancias;

    protected ConstanciasControllerBase(CN_ConstanciaSalarial constancias)
    {
        _constancias = constancias;
    }

    protected IActionResult IndexCore(string? busqueda, int pagina)
    {
        const int tamanoPagina = 8;
        pagina = Math.Max(1, pagina);
        busqueda = string.IsNullOrWhiteSpace(busqueda) ? null : busqueda.Trim();
        var resultado = _constancias.ListarEmpleadosDisponibles(pagina, tamanoPagina, busqueda);
        var totalPaginas = Math.Max(1, (int)Math.Ceiling(resultado.Total / (double)tamanoPagina));

        if (pagina > totalPaginas)
        {
            pagina = totalPaginas;
            resultado = _constancias.ListarEmpleadosDisponibles(pagina, tamanoPagina, busqueda);
        }

        return View(VistasConstancias + "Index.cshtml", new Models.ConstanciasSalarialesIndexModel
        {
            Empleados = resultado.Empleados,
            Busqueda = busqueda,
            FechaEmision = DateOnly.FromDateTime(DateTime.Today),
            PaginaActual = pagina,
            TotalPaginas = totalPaginas,
            TotalRegistros = resultado.Total,
            TamanoPagina = tamanoPagina
        });
    }

    protected IActionResult GenerarCore(int idEmpleado, int meses, DateOnly? fechaEmision)
    {
        var hoy = DateOnly.FromDateTime(DateTime.Today);
        var fecha = fechaEmision ?? hoy;

        if (idEmpleado <= 0)
        {
            TempData["Error"] = "Selecciona un empleado para generar la constancia.";
            return RedirectToAction("Index");
        }

        if (meses is not (3 or 6 or 12))
        {
            TempData["Error"] = "La constancia solo puede emitirse por 3, 6 o 12 meses.";
            return RedirectToAction("Index");
        }

        if (fecha > hoy)
        {
            TempData["Error"] = "La fecha de emisión no puede ser posterior a la fecha actual.";
            return RedirectToAction("Index");
        }

        try
        {
            var constancia = _constancias.Generar(idEmpleado, meses, fecha);
            if (constancia is null)
            {
                return NotFound();
            }

            return View(VistasConstancias + "Detalle.cshtml", new Models.ConstanciaSalarialViewModel
            {
                Constancia = constancia,
                Meses = meses
            });
        }
        catch (SqlException ex)
        {
            TempData["Error"] = MensajeError(ex);
            return RedirectToAction("Index");
        }
    }

    private static string MensajeError(SqlException ex) => ex.Number switch
    {
        51130 => "La constancia solo puede emitirse por 3, 6 o 12 meses.",
        51131 => "La constancia solo puede emitirse para empleados activos.",
        51132 => "El empleado seleccionado no tiene un contrato vigente.",
        51133 => "La fecha de emisión no puede ser posterior a la fecha actual.",
        _ => ex.Message
    };
}
