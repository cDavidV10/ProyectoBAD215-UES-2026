using BEMO.Datos;
using BEMO.Entidades;

namespace BEMO.Negocio;

public class CN_TipoContrato
{
    private readonly DAOTipoContrato _metodos;

    public CN_TipoContrato(DAOTipoContrato metodos)
    {
        _metodos = metodos;
    }

    public List<TipoContrato> Listar()
    {
        return _metodos.Listar();
    }
}
