using BEMO.Datos;
using BEMO.Entidades;

namespace BEMO.Negocio;

public class CN_MunicipioUbicacion
{
    private readonly DAOMunicipioUbicacion _metodos;

    public CN_MunicipioUbicacion(DAOMunicipioUbicacion metodos)
    {
        _metodos = metodos;
    }

    public List<MunicipioUbicacion> Listar()
    {
        return _metodos.Listar();
    }
}
