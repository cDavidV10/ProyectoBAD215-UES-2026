using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BEMO.Negocio;

namespace BEMOWeb.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "ADMINISTRADOR NOMINA")]
public class UsuariosController : Controller
{
    private readonly CN_Empleado _empleado;
    private readonly CN_Usuario _usuarios;

    public UsuariosController(CN_Empleado empleado, CN_Usuario usuarios)
    {
        _empleado = empleado;
        _usuarios = usuarios;
    }

    public IActionResult Index(
        int pagina = 1,
        string? busqueda = null,
        string? rol = null,
        string? estado = null)
    {
        const int tamanoPagina = 5;
        pagina = Math.Max(1, pagina);

        var resultado = _usuarios.ListarPaginado(
            pagina,
            tamanoPagina,
            busqueda,
            rol,
            estado);
        var totalPaginas = Math.Max(1, (int)Math.Ceiling(resultado.Total / (double)tamanoPagina));

        if (pagina > totalPaginas)
        {
            pagina = totalPaginas;
            resultado = _usuarios.ListarPaginado(
                pagina,
                tamanoPagina,
                busqueda,
                rol,
                estado);
        }

        var modelo = new Models.UsuariosModel
        {
            Usuarios = resultado.Usuarios,
            Roles = _usuarios.ListarRoles(),
            Busqueda = busqueda,
            RolSeleccionado = rol,
            EstadoSeleccionado = estado,
            PaginaActual = pagina,
            TotalPaginas = totalPaginas,
            TotalRegistros = resultado.Total,
            TamanoPagina = tamanoPagina
        };

        return View(modelo);
    }

    [HttpGet]
    public IActionResult Nuevo()
    {
        var empleadosDisponibles = _empleado.ListarActivosSinUsuarioDeRecursosHumanos();
        return View("~/Areas/Admin/Views/Usuarios/Nuevo.cshtml", empleadosDisponibles);
    }
}
