using BEMO.Negocio;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace BEMOWeb.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "ADMINISTRADOR")]
public class PrestamosAnticiposController : Controller
{
    private readonly CN_PrestamosAnticipos _prestamosAnticipos;

    public PrestamosAnticiposController(CN_PrestamosAnticipos prestamosAnticipos)
    {
        _prestamosAnticipos = prestamosAnticipos;
    }

    [HttpGet]
    public IActionResult Index(int pagina = 1, string? busqueda = null, string? estado = null, string tipo = "prestamos")
    {
        const int tamanoPagina = 5;
        pagina = Math.Max(1, pagina);
        tipo = NormalizarTipo(tipo);
        var modelo = new Models.PrestamosAnticiposIndexModel
        {
            TipoSeleccionado = tipo,
            Busqueda = string.IsNullOrWhiteSpace(busqueda) ? null : busqueda.Trim(),
            EstadoSeleccionado = string.IsNullOrWhiteSpace(estado) ? null : estado.Trim().ToUpperInvariant(),
            Resumen = _prestamosAnticipos.ObtenerResumen()
        };

        if (tipo == "prestamos")
        {
            var resultado = _prestamosAnticipos.ListarPrestamosPaginado(
                pagina, tamanoPagina, modelo.Busqueda, modelo.EstadoSeleccionado);
            var totalPaginas = Math.Max(1, (int)Math.Ceiling(resultado.Total / (double)tamanoPagina));
            if (pagina > totalPaginas)
            {
                pagina = totalPaginas;
                resultado = _prestamosAnticipos.ListarPrestamosPaginado(
                    pagina, tamanoPagina, modelo.Busqueda, modelo.EstadoSeleccionado);
            }

            modelo.Prestamos = resultado.Prestamos;
            modelo.TotalRegistros = resultado.Total;
            modelo.TotalPaginas = totalPaginas;
        }
        else
        {
            var resultado = _prestamosAnticipos.ListarAnticiposPaginado(
                pagina, tamanoPagina, modelo.Busqueda, modelo.EstadoSeleccionado);
            var totalPaginas = Math.Max(1, (int)Math.Ceiling(resultado.Total / (double)tamanoPagina));
            if (pagina > totalPaginas)
            {
                pagina = totalPaginas;
                resultado = _prestamosAnticipos.ListarAnticiposPaginado(
                    pagina, tamanoPagina, modelo.Busqueda, modelo.EstadoSeleccionado);
            }

            modelo.Anticipos = resultado.Anticipos;
            modelo.TotalRegistros = resultado.Total;
            modelo.TotalPaginas = totalPaginas;
        }

        modelo.PaginaActual = pagina;
        modelo.TamanoPagina = tamanoPagina;
        return View(modelo);
    }

    [HttpGet]
    public IActionResult NuevoPrestamo(int pagina = 1, string? busqueda = null)
    {
        var modelo = new Models.NuevoPrestamoModel();
        CargarEmpleados(modelo, pagina, busqueda);
        return View(modelo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult NuevoPrestamo(Models.NuevoPrestamoModel modelo)
    {
        ValidarFecha(modelo.FechaOtorgamiento, "FechaOtorgamiento");
        if (!ModelState.IsValid)
        {
            CargarEmpleados(modelo, modelo.PaginaActual, modelo.Busqueda);
            return View(modelo);
        }

        try
        {
            var idPrestamo = _prestamosAnticipos.RegistrarPrestamo(
                modelo.IdEmpleado!.Value,
                modelo.Monto,
                modelo.FechaOtorgamiento);

            TempData["Mensaje"] = $"El préstamo PRE-{idPrestamo:D6} fue registrado correctamente. Las cuotas serán aplicadas por la planilla.";
            return RedirectToAction(nameof(Index), new { tipo = "prestamos" });
        }
        catch (SqlException ex)
        {
            ModelState.AddModelError(string.Empty, MensajeError(ex));
            CargarEmpleados(modelo, modelo.PaginaActual, modelo.Busqueda);
            return View(modelo);
        }
    }

    [HttpGet]
    public IActionResult NuevoAnticipo(int pagina = 1, string? busqueda = null)
    {
        var modelo = new Models.NuevoAnticipoModel();
        CargarEmpleados(modelo, pagina, busqueda);
        return View(modelo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult NuevoAnticipo(Models.NuevoAnticipoModel modelo)
    {
        ValidarFecha(modelo.FechaAnticipo, "FechaAnticipo");
        if (!ModelState.IsValid)
        {
            CargarEmpleados(modelo, modelo.PaginaActual, modelo.Busqueda);
            return View(modelo);
        }

        try
        {
            var idAnticipo = _prestamosAnticipos.RegistrarAnticipo(
                modelo.IdEmpleado!.Value,
                modelo.Monto,
                modelo.FechaAnticipo);

            TempData["Mensaje"] = $"El anticipo ANT-{idAnticipo:D6} fue registrado y quedó pendiente de descuento.";
            return RedirectToAction(nameof(Index), new { tipo = "anticipos" });
        }
        catch (SqlException ex)
        {
            ModelState.AddModelError(string.Empty, MensajeError(ex));
            CargarEmpleados(modelo, modelo.PaginaActual, modelo.Busqueda);
            return View(modelo);
        }
    }

    [HttpGet]
    public IActionResult RevisarPrestamo(int id)
    {
        var prestamo = _prestamosAnticipos.ObtenerPrestamo(id);
        return prestamo is null ? NotFound() : View(prestamo);
    }

    [HttpGet]
    public IActionResult DetalleAnticipo(int id)
    {
        var anticipo = _prestamosAnticipos.ObtenerAnticipo(id);
        return anticipo is null ? NotFound() : View(anticipo);
    }

    [HttpGet]
    public IActionResult MaximoAnticipo(int idEmpleado, DateOnly fecha)
    {
        if (idEmpleado <= 0 || fecha == default)
        {
            return Json(new { maximo = (decimal?)null });
        }

        var info = _prestamosAnticipos.ObtenerInformacionEmpleado(idEmpleado, fecha);
        return Json(new
        {
            maximo = info is null || info.TieneAnticipoPendiente
                ? null
                : _prestamosAnticipos.ObtenerMaximoAnticipo(idEmpleado, fecha)
        });
    }

    [HttpGet]
    public IActionResult InformacionEmpleado(int idEmpleado, DateOnly fecha)
    {
        if (idEmpleado <= 0 || fecha == default)
        {
            return Json(new { disponible = false });
        }

        var info = _prestamosAnticipos.ObtenerInformacionEmpleado(idEmpleado, fecha);
        return info is null
            ? Json(new { disponible = false })
            : Json(new
            {
                disponible = true,
                nombre = info.NombreEmpleado,
                cargo = info.NombreCargo,
                salarioBruto = info.SalarioBruto,
                salarioQuincenal = info.SalarioQuincenal,
                maximoPrestamo = info.MaximoPrestamo,
                maximoAnticipo = info.MaximoAnticipo,
                tienePrestamoActivo = info.TienePrestamoActivo,
                tieneAnticipoPendiente = info.TieneAnticipoPendiente
            });
    }

    private void ValidarFecha(DateOnly fecha, string campo)
    {
        if (fecha == default)
        {
            ModelState.AddModelError(campo, "Indique una fecha válida.");
        }
    }

    private static string NormalizarTipo(string? tipo)
    {
        return string.Equals(tipo, "anticipos", StringComparison.OrdinalIgnoreCase)
            ? "anticipos"
            : "prestamos";
    }

    private void CargarEmpleados(Models.NuevoPrestamoModel modelo, int pagina, string? busqueda)
    {
        const int tamanoPagina = 5;
        pagina = Math.Max(1, pagina);
        busqueda = NormalizarBusqueda(busqueda);
        var empleados = _prestamosAnticipos.ListarEmpleadosActivos();

        modelo.Empleados = empleados;
        modelo.Busqueda = busqueda;
        modelo.PaginaActual = 1;
        modelo.TotalRegistros = empleados.Count;
        modelo.TotalPaginas = 1;
        modelo.TamanoPagina = tamanoPagina;
    }

    private void CargarEmpleados(Models.NuevoAnticipoModel modelo, int pagina, string? busqueda)
    {
        const int tamanoPagina = 5;
        pagina = Math.Max(1, pagina);
        busqueda = NormalizarBusqueda(busqueda);
        var empleados = _prestamosAnticipos.ListarEmpleadosActivos();

        modelo.Empleados = empleados;
        modelo.Busqueda = busqueda;
        modelo.PaginaActual = 1;
        modelo.TotalRegistros = empleados.Count;
        modelo.TotalPaginas = 1;
        modelo.TamanoPagina = tamanoPagina;
    }

    private static string? NormalizarBusqueda(string? busqueda)
    {
        return string.IsNullOrWhiteSpace(busqueda) ? null : busqueda.Trim();
    }

    private static string MensajeError(SqlException ex)
    {
        return string.IsNullOrWhiteSpace(ex.Message)
            ? "No se pudo completar la operación."
            : ex.Message.Trim();
    }
}
