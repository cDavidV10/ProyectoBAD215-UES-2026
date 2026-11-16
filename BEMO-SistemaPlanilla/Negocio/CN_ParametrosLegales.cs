using BEMO.Datos;
using BEMO.Entidades;

namespace BEMO.Negocio;

public class CN_ParametrosLegales
{
    private readonly DAOParametrosLegales _metodos;

    public CN_ParametrosLegales(DAOParametrosLegales metodos)
    {
        _metodos = metodos;
    }

    public List<TipoDescuento> ListarTiposDescuento() => _metodos.ListarTiposDescuento();
    public List<TipoRenta> ListarTiposRenta() => _metodos.ListarTiposRenta();
    public List<DescuentoLey> ListarDescuentosLey() => _metodos.ListarDescuentosLey();
    public DescuentoLey? ObtenerDescuentoLey(int id) => _metodos.ObtenerDescuentoLey(id);
    public List<Renta> ListarRentas() => _metodos.ListarRentas();
    public Renta? ObtenerRenta(int id) => _metodos.ObtenerRenta(id);
    public bool TienePlanillaCerradaDescuento(int id) => _metodos.TienePlanillaCerradaDescuento(id);
    public bool TienePlanillaCerradaRenta(int id) => _metodos.TienePlanillaCerradaRenta(id);

    public void CrearDescuentoLey(int idTipoDescuento, decimal porcentaje, decimal? topeMensual, DateOnly fechaVigencia) =>
        _metodos.CrearDescuentoLey(idTipoDescuento, porcentaje, topeMensual, fechaVigencia);

    public void ActualizarDescuentoLey(int id, int idTipoDescuento, decimal porcentaje, decimal? topeMensual, DateOnly fechaVigencia) =>
        _metodos.ActualizarDescuentoLey(id, idTipoDescuento, porcentaje, topeMensual, fechaVigencia);

    public void CrearRenta(int idTipoRenta, string codigoRenta, decimal rangoMinimo, decimal rangoMaximo, decimal porcentaje, decimal valorFijo, DateOnly fechaVigencia) =>
        _metodos.CrearRenta(idTipoRenta, codigoRenta, rangoMinimo, rangoMaximo, porcentaje, valorFijo, fechaVigencia);

    public void ActualizarRenta(int id, int idTipoRenta, string codigoRenta, decimal rangoMinimo, decimal rangoMaximo, decimal porcentaje, decimal valorFijo, DateOnly fechaVigencia) =>
        _metodos.ActualizarRenta(id, idTipoRenta, codigoRenta, rangoMinimo, rangoMaximo, porcentaje, valorFijo, fechaVigencia);
}
