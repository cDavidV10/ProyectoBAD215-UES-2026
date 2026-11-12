using BEMO.Datos;
using BEMO.Entidades;

namespace BEMO.Negocio;

public class CN_Usuario
{
    private readonly DAOUsuario _metodos;

    public CN_Usuario(DAOUsuario metodos)
    {
        _metodos = metodos;
    }

    public (List<UsuarioListado> Usuarios, int Total) ListarPaginado(
        int pagina,
        int tamanoPagina,
        string? busqueda,
        string? rol,
        string? estado)
    {
        return _metodos.ListarPaginado(pagina, tamanoPagina, busqueda, rol, estado);
    }

    public List<string> ListarRoles()
    {
        return _metodos.ListarRoles();
    }
}
