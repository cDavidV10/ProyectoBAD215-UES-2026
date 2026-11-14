using BEMO.Datos;
using BEMO.Entidades;

namespace BEMO.Negocio;

public class CN_Capacitacion
{
    private readonly DAOCapacitacion _metodos;

    public CN_Capacitacion(DAOCapacitacion metodos)
    {
        _metodos = metodos;
    }

    public List<TipoCapacitacion> ListarTipos() => _metodos.ListarTipos();

    public (List<CapacitacionListado> Capacitaciones, int Total) ListarPaginado(
        int pagina,
        int tamanoPagina,
        string? busqueda,
        int? idTipoCapacitacion,
        string? estado) =>
        _metodos.ListarPaginado(pagina, tamanoPagina, busqueda, idTipoCapacitacion, estado);

    public CapacitacionListado? ObtenerPorId(int idCapacitacion) => _metodos.ObtenerPorId(idCapacitacion);

    public List<CapacitacionParticipante> ListarParticipantes(int idCapacitacion) =>
        _metodos.ListarParticipantes(idCapacitacion);

    public (List<CapacitacionParticipante> Participantes, int Total) ListarParticipantesPaginado(
        int idCapacitacion,
        int pagina,
        int tamanoPagina) =>
        _metodos.ListarParticipantesPaginado(idCapacitacion, pagina, tamanoPagina);

    public List<EmpleadoCapacitacionListado> ListarEmpleadosParaAsignacion(
        int idCapacitacion,
        string? busqueda,
        string? departamento) =>
        _metodos.ListarEmpleadosParaAsignacion(idCapacitacion, busqueda, departamento);

    public (List<EmpleadoCapacitacionListado> Empleados, int Total) ListarEmpleadosParaAsignacionPaginado(
        int idCapacitacion,
        string? busqueda,
        string? departamento,
        int pagina,
        int tamanoPagina) =>
        _metodos.ListarEmpleadosParaAsignacionPaginado(
            idCapacitacion,
            busqueda,
            departamento,
            pagina,
            tamanoPagina);

    public int Registrar(string nombreCapacitacion, int idTipoCapacitacion) =>
        _metodos.Registrar(nombreCapacitacion, idTipoCapacitacion);

    public void RegistrarParticipaciones(
        int idCapacitacion,
        IEnumerable<int> idsEmpleados,
        DateOnly fechaInicio,
        DateOnly fechaFin) =>
        _metodos.RegistrarParticipaciones(idCapacitacion, idsEmpleados, fechaInicio, fechaFin);

    public void ActualizarResultado(int idDetalleCapacitacion, decimal resultado) =>
        _metodos.ActualizarResultado(idDetalleCapacitacion, resultado);

    public void ActualizarResultados(
        int idCapacitacion,
        IEnumerable<(int IdDetalleCapacitacion, decimal Resultado)> resultados) =>
        _metodos.ActualizarResultados(idCapacitacion, resultados);
}
