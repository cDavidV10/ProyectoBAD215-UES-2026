using BEMO.Entidades;

namespace BEMOWeb.Models;

public class ParametrosLegalesModel
{
    public List<DescuentoLey> DescuentosLey { get; set; } = new();
    public List<Renta> Rentas { get; set; } = new();
}

public class ParametroDescuentoLeyModel
{
    public int? IdDescuento { get; set; }

    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Seleccione el tipo de descuento.")]
    public int? IdTipoDescuento { get; set; }

    [System.ComponentModel.DataAnnotations.Range(typeof(decimal), "0.01", "100", ErrorMessage = "El porcentaje debe estar entre 0.01 y 100.")]
    public decimal PorcentajeDescuento { get; set; }

    [System.ComponentModel.DataAnnotations.Range(typeof(decimal), "0.01", "9999999999", ErrorMessage = "El tope mensual debe ser mayor que cero.")]
    public decimal? TopeMensual { get; set; }

    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Seleccione la fecha de vigencia.")]
    public DateOnly? FechaVigencia { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    public List<TipoDescuento> TiposDescuento { get; set; } = new();
}

public class ParametroRentaModel
{
    public int? IdRenta { get; set; }

    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Seleccione el tramo de renta.")]
    public int? IdTipoRenta { get; set; }

    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Ingrese el código del tramo.")]
    [System.ComponentModel.DataAnnotations.StringLength(20, ErrorMessage = "El código no puede superar 20 caracteres.")]
    public string CodigoRenta { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Range(typeof(decimal), "0", "9999999999", ErrorMessage = "El rango mínimo no puede ser negativo.")]
    public decimal RangoMinimoRenta { get; set; }

    [System.ComponentModel.DataAnnotations.Range(typeof(decimal), "0.01", "9999999999", ErrorMessage = "El rango máximo debe ser mayor que cero.")]
    public decimal RangoMaximoRenta { get; set; }

    [System.ComponentModel.DataAnnotations.Range(typeof(decimal), "0", "100", ErrorMessage = "El porcentaje debe estar entre 0 y 100.")]
    public decimal PorcentajeRenta { get; set; }

    [System.ComponentModel.DataAnnotations.Range(typeof(decimal), "0", "9999999999", ErrorMessage = "El valor fijo no puede ser negativo.")]
    public decimal ValorFijo { get; set; }

    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Seleccione la fecha de vigencia.")]
    public DateOnly? FechaVigenciaRenta { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    public List<TipoRenta> TiposRenta { get; set; } = new();
}
