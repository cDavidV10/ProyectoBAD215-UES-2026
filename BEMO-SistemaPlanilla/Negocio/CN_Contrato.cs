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

    public ContratoListado? ObtenerPorId(int idContrato)
    {
        return _metodos.ObtenerPorId(idContrato);
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

    public int Registrar(
        int idEmpleado,
        int idCargo,
        int idDepartamento,
        int idTipoContrato,
        decimal salarioBruto,
        DateOnly fechaContratacion,
        DateOnly? fechaCulminacion)
    {
        return _metodos.Registrar(
            idEmpleado,
            idCargo,
            idDepartamento,
            idTipoContrato,
            salarioBruto,
            fechaContratacion,
            fechaCulminacion);
    }

    public void Actualizar(
        int idContrato,
        int idCargo,
        int idDepartamento,
        int idTipoContrato,
        decimal salarioBruto,
        DateOnly fechaContratacion,
        DateOnly? fechaCulminacion)
    {
        _metodos.Actualizar(
            idContrato,
            idCargo,
            idDepartamento,
            idTipoContrato,
            salarioBruto,
            fechaContratacion,
            fechaCulminacion);
    }

    public void Finalizar(int idContrato)
    {
        _metodos.Finalizar(idContrato);
    }
}
