using BEMO.Datos;
using BEMO.Entidades;

namespace BEMO.Negocio;

public class CN_Contrato
{
    private readonly DAOContrato _metodos;

    public CN_Contrato(DAOContrato metodos)
    {
        _metodos = metodos;
    }

    public List<ContratoListado> ListarVigentes()
    {
        return _metodos.ListarVigentes();
    }

    public (List<ContratoListado> Contratos, int Total) ListarPaginado(
        int pagina,
        int tamanoPagina,
        string? busqueda,
        int? idEmpleado,
        string? tipoContrato,
        string? situacion)
    {
        return _metodos.ListarPaginado(
            pagina,
            tamanoPagina,
            busqueda,
            idEmpleado,
            tipoContrato,
            situacion);
    }
}
