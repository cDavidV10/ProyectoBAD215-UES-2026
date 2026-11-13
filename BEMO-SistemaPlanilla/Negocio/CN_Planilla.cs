using BEMO.Datos;
using BEMO.Entidades;

namespace BEMO.Negocio;

public class CN_Planilla
{
    private readonly DAOPlanilla _metodos;

    public CN_Planilla(DAOPlanilla metodos)
    {
        _metodos = metodos;
    }

    public (List<PlanillaListado> Planillas, int Total) ListarPaginado(int pagina, int tamanoPagina, string? estado)
    {
        return _metodos.ListarPaginado(pagina, tamanoPagina, estado);
    }

    public PlanillaListado? Obtener(int idPlanilla)
    {
        return _metodos.Obtener(idPlanilla);
    }

    public List<RegistroAsistenciaPlanilla> ListarAsistencia(int idPlanilla)
    {
        return _metodos.ListarAsistencia(idPlanilla);
    }

    public (List<RegistroAsistenciaPlanilla> Asistencias, int Total) ListarAsistenciaPaginado(
        int idPlanilla,
        int pagina,
        int tamanoPagina)
    {
        return _metodos.ListarAsistenciaPaginado(idPlanilla, pagina, tamanoPagina);
    }

    public (List<DetallePlanillaListado> Detalles, int Total) ListarDetalle(
        int idPlanilla,
        int pagina,
        int tamanoPagina)
    {
        return _metodos.ListarDetalle(idPlanilla, pagina, tamanoPagina);
    }

    public int Crear(DateOnly periodoInicio, DateOnly periodoFin, DateOnly fechaPago, int? idUsuario = null)
    {
        return _metodos.Crear(periodoInicio, periodoFin, fechaPago, idUsuario);
    }

    public void CapturarAsistencia(int idDetallePlanilla, int diasTrabajados, int diasNoTrabajados, int horasExtras, decimal bonificaciones, int? idUsuario = null)
    {
        _metodos.CapturarAsistencia(idDetallePlanilla, diasTrabajados, diasNoTrabajados, horasExtras, bonificaciones, idUsuario);
    }

    public void Calcular(int idPlanilla, bool incluirAguinaldo, int? idUsuario = null)
    {
        _metodos.Calcular(idPlanilla, incluirAguinaldo, idUsuario);
    }

    public void Cerrar(int idPlanilla, int? idUsuario = null)
    {
        _metodos.Cerrar(idPlanilla, idUsuario);
    }

    public void Reabrir(int idPlanilla, string motivo, int? idUsuario = null)
    {
        _metodos.Reabrir(idPlanilla, motivo, idUsuario);
    }
}
