using BEMO.Datos;
using BEMO.Entidades;

namespace BEMO.Negocio;

public class CN_Auditoria
{
    private readonly DAOAuditoria _metodos;

    public CN_Auditoria(DAOAuditoria metodos)
    {
        _metodos = metodos;
    }

    public (List<AuditoriaListado> Auditorias, int Total) ListarPaginado(
        int pagina,
        int tamanoPagina,
        DateOnly? fechaDesde,
        DateOnly? fechaHasta,
        string? nombreUsuario,
        string? modulo,
        string? operacion) =>
        _metodos.ListarPaginado(
            pagina,
            tamanoPagina,
            fechaDesde,
            fechaHasta,
            nombreUsuario,
            modulo,
            operacion);

    public AuditoriaListado? ObtenerPorId(int idAuditoria) => _metodos.ObtenerPorId(idAuditoria);

    public List<string> ListarUsuarios() => _metodos.ListarUsuarios();

    public List<string> ListarModulos() => _metodos.ListarModulos();

    public List<string> ListarOperaciones() => _metodos.ListarOperaciones();
}
