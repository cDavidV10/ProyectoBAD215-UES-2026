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

    public List<Rol> ListarRolesDisponibles()
    {
        return _metodos.ListarRolesDisponibles();
    }

    public string Crear(int idEmpleado, int idRol, string password)
    {
        return _metodos.Crear(idEmpleado, idRol, password);
    }

    public void Desactivar(int idUsuario)
    {
        _metodos.Desactivar(idUsuario);
    }

    public void Activar(int idUsuario)
    {
        _metodos.Activar(idUsuario);
    }

    public void CambiarPasswordAdministrador(int idUsuario, string passwordNueva)
    {
        _metodos.CambiarPasswordAdministrador(idUsuario, passwordNueva);
    }
}
