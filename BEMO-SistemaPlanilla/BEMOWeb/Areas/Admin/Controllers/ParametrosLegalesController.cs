using BEMO.Negocio;
using BEMOWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace BEMOWeb.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "ADMINISTRADOR")]
public class ParametrosLegalesController : Controller
{
    private readonly CN_ParametrosLegales _parametros;

    public ParametrosLegalesController(CN_ParametrosLegales parametros)
    {
        _parametros = parametros;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View(new ParametrosLegalesModel
        {
            DescuentosLey = _parametros.ListarDescuentosLey(),
            Rentas = _parametros.ListarRentas()
        });
    }

    [HttpGet]
    public IActionResult NuevoDescuento()
    {
        return View("Descuento", CargarTipos(new ParametroDescuentoLeyModel()));
    }

    [HttpGet]
    public IActionResult EditarDescuento(int id)
    {
        var descuento = _parametros.ObtenerDescuentoLey(id);
        if (descuento is null)
        {
            return NotFound();
        }

        if (_parametros.TienePlanillaCerradaDescuento(id))
        {
            TempData["Error"] = "Este parámetro ya fue utilizado por una planilla cerrada. Registre una nueva vigencia para cambiarlo.";
            return RedirectToAction(nameof(Index));
        }

        return View("Descuento", CargarTipos(new ParametroDescuentoLeyModel
        {
            IdDescuento = descuento.IdDescuento,
            IdTipoDescuento = descuento.IdTipoDescuento,
            PorcentajeDescuento = descuento.PorcentajeDescuento,
            TopeMensual = descuento.TopeMensual,
            FechaVigencia = descuento.FechaVigencia
        }));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult GuardarDescuento(ParametroDescuentoLeyModel modelo)
    {
        CargarTipos(modelo);
        if (modelo.FechaVigencia is null)
        {
            ModelState.AddModelError(nameof(modelo.FechaVigencia), "Seleccione la fecha de vigencia.");
        }

        if (modelo.IdDescuento.HasValue && _parametros.TienePlanillaCerradaDescuento(modelo.IdDescuento.Value))
        {
            ModelState.AddModelError(string.Empty, "Este parámetro ya fue utilizado por una planilla cerrada. Registre una nueva vigencia para cambiarlo.");
        }

        if (!ModelState.IsValid)
        {
            return View("Descuento", modelo);
        }

        try
        {
            if (modelo.IdDescuento.HasValue)
            {
                _parametros.ActualizarDescuentoLey(
                    modelo.IdDescuento.Value,
                    modelo.IdTipoDescuento!.Value,
                    modelo.PorcentajeDescuento,
                    modelo.TopeMensual,
                    modelo.FechaVigencia!.Value);
                TempData["Mensaje"] = "El parámetro de descuento fue actualizado correctamente.";
            }
            else
            {
                _parametros.CrearDescuentoLey(
                    modelo.IdTipoDescuento!.Value,
                    modelo.PorcentajeDescuento,
                    modelo.TopeMensual,
                    modelo.FechaVigencia!.Value);
                TempData["Mensaje"] = "La nueva vigencia de descuento fue registrada correctamente.";
            }

            return RedirectToAction(nameof(Index));
        }
        catch (SqlException ex)
        {
            ModelState.AddModelError(string.Empty, MensajeError(ex, "el parámetro de descuento"));
            return View("Descuento", modelo);
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View("Descuento", modelo);
        }
    }

    [HttpGet]
    public IActionResult NuevaRenta()
    {
        return View("Renta", CargarTipos(new ParametroRentaModel()));
    }

    [HttpGet]
    public IActionResult EditarRenta(int id)
    {
        var renta = _parametros.ObtenerRenta(id);
        if (renta is null)
        {
            return NotFound();
        }

        if (_parametros.TienePlanillaCerradaRenta(id))
        {
            TempData["Error"] = "Este tramo ya fue utilizado por una planilla cerrada. Registre una nueva vigencia para cambiarlo.";
            return RedirectToAction(nameof(Index));
        }

        return View("Renta", CargarTipos(new ParametroRentaModel
        {
            IdRenta = renta.IdRenta,
            IdTipoRenta = renta.IdTipoRenta,
            CodigoRenta = renta.CodigoRenta,
            RangoMinimoRenta = renta.RangoMinimoRenta,
            RangoMaximoRenta = renta.RangoMaximoRenta,
            PorcentajeRenta = renta.PorcentajeRenta,
            ValorFijo = renta.ValorFijo,
            FechaVigenciaRenta = renta.FechaVigenciaRenta
        }));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult GuardarRenta(ParametroRentaModel modelo)
    {
        CargarTipos(modelo);
        if (modelo.RangoMaximoRenta <= modelo.RangoMinimoRenta)
        {
            ModelState.AddModelError(nameof(modelo.RangoMaximoRenta), "El rango máximo debe ser mayor que el rango mínimo.");
        }

        if (modelo.FechaVigenciaRenta is null)
        {
            ModelState.AddModelError(nameof(modelo.FechaVigenciaRenta), "Seleccione la fecha de vigencia.");
        }

        if (modelo.IdRenta.HasValue && _parametros.TienePlanillaCerradaRenta(modelo.IdRenta.Value))
        {
            ModelState.AddModelError(string.Empty, "Este tramo ya fue utilizado por una planilla cerrada. Registre una nueva vigencia para cambiarlo.");
        }

        if (!ModelState.IsValid)
        {
            return View("Renta", modelo);
        }

        try
        {
            if (modelo.IdRenta.HasValue)
            {
                _parametros.ActualizarRenta(
                    modelo.IdRenta.Value,
                    modelo.IdTipoRenta!.Value,
                    modelo.CodigoRenta,
                    modelo.RangoMinimoRenta,
                    modelo.RangoMaximoRenta,
                    modelo.PorcentajeRenta,
                    modelo.ValorFijo,
                    modelo.FechaVigenciaRenta!.Value);
                TempData["Mensaje"] = "El tramo de renta fue actualizado correctamente.";
            }
            else
            {
                _parametros.CrearRenta(
                    modelo.IdTipoRenta!.Value,
                    modelo.CodigoRenta,
                    modelo.RangoMinimoRenta,
                    modelo.RangoMaximoRenta,
                    modelo.PorcentajeRenta,
                    modelo.ValorFijo,
                    modelo.FechaVigenciaRenta!.Value);
                TempData["Mensaje"] = "El nuevo tramo de renta fue registrado correctamente.";
            }

            return RedirectToAction(nameof(Index));
        }
        catch (SqlException ex)
        {
            ModelState.AddModelError(string.Empty, MensajeError(ex, "el tramo de renta"));
            return View("Renta", modelo);
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View("Renta", modelo);
        }
    }

    private ParametroDescuentoLeyModel CargarTipos(ParametroDescuentoLeyModel modelo)
    {
        modelo.TiposDescuento = _parametros.ListarTiposDescuento();
        return modelo;
    }

    private ParametroRentaModel CargarTipos(ParametroRentaModel modelo)
    {
        modelo.TiposRenta = _parametros.ListarTiposRenta();
        return modelo;
    }

    private static string MensajeError(SqlException ex, string entidad)
    {
        if (ex.Number is 2601 or 2627)
        {
            return $"Ya existe {entidad} con la misma fecha de vigencia.";
        }

        if (ex.Number is 547)
        {
            return $"No se puede guardar {entidad} porque el tipo seleccionado no existe.";
        }

        if (ex.Number is 52050 or 52051 || ex.Message.Contains("planilla cerrada", StringComparison.OrdinalIgnoreCase))
        {
            return "No se puede modificar un parámetro utilizado por una planilla cerrada. Registre una nueva vigencia.";
        }

        return $"No se pudo guardar {entidad}. Verifique los datos ingresados.";
    }
}
