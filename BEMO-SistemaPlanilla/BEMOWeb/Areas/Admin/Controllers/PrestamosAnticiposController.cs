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
    public IActionResult Index(string? busqueda, string? estado, string tipo = "prestamos")
    {
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
            modelo.Prestamos = _prestamosAnticipos.ListarPrestamos(modelo.Busqueda, modelo.EstadoSeleccionado);
        }
        else
        {
            modelo.Anticipos = _prestamosAnticipos.ListarAnticipos(modelo.Busqueda, modelo.EstadoSeleccionado);
        }

        return View(modelo);
    }

    [HttpGet]
    public IActionResult NuevoPrestamo()
    {
        return View(new Models.NuevoPrestamoModel
        {
            Empleados = _prestamosAnticipos.ListarEmpleadosActivos()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult NuevoPrestamo(Models.NuevoPrestamoModel modelo)
    {
        ValidarFecha(modelo.FechaOtorgamiento, "FechaOtorgamiento");
        if (!ModelState.IsValid)
        {
            modelo.Empleados = _prestamosAnticipos.ListarEmpleadosActivos();
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
            modelo.Empleados = _prestamosAnticipos.ListarEmpleadosActivos();
            return View(modelo);
        }
    }

    [HttpGet]
    public IActionResult NuevoAnticipo()
    {
        return View(new Models.NuevoAnticipoModel
        {
            Empleados = _prestamosAnticipos.ListarEmpleadosActivos()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult NuevoAnticipo(Models.NuevoAnticipoModel modelo)
    {
        ValidarFecha(modelo.FechaAnticipo, "FechaAnticipo");
        if (!ModelState.IsValid)
        {
            modelo.Empleados = _prestamosAnticipos.ListarEmpleadosActivos();
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
            modelo.Empleados = _prestamosAnticipos.ListarEmpleadosActivos();
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

        return Json(new
        {
            maximo = _prestamosAnticipos.ObtenerMaximoAnticipo(idEmpleado, fecha)
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

    private static string MensajeError(SqlException ex)
    {
        return string.IsNullOrWhiteSpace(ex.Message)
            ? "No se pudo completar la operación."
            : ex.Message.Trim();
    }
}
