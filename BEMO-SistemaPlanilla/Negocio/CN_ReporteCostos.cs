using BEMO.Datos;
using BEMO.Entidades;

namespace BEMO.Negocio;

public class CN_ReporteCostos
{
    private readonly DAOReporteCostos _metodos;

    public CN_ReporteCostos(DAOReporteCostos metodos)
    {
        _metodos = metodos;
    }

    public List<CostoDepartamentoComparativo> Obtener(
        DateOnly? fechaDesde,
        DateOnly? fechaHasta) =>
        _metodos.Obtener(fechaDesde, fechaHasta);
}
