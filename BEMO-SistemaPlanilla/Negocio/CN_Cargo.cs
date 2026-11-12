using BEMO.Datos;
using BEMO.Entidades;

namespace BEMO.Negocio;

public class CN_Cargo
{
    private readonly DAOCargo _metodos;

    public CN_Cargo(DAOCargo metodos)
    {
        _metodos = metodos;
    }

    public List<Cargo> Listar()
    {
        return _metodos.Listar();
    }
}
