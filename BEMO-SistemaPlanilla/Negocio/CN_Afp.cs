using BEMO.Datos;
using BEMO.Entidades;

namespace BEMO.Negocio;

public class CN_Afp
{
    private readonly DAOAfp _metodos;

    public CN_Afp(DAOAfp metodos)
    {
        _metodos = metodos;
    }

    public List<Afp> Listar()
    {
        return _metodos.Listar();
    }
}
