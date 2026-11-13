using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
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
        return View("~/Areas/Admin/Views/Usuarios/Nuevo.cshtml", CargarModeloNuevoUsuario());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Nuevo(Models.NuevoUsuarioModel modelo)
    {
        CargarCatalogosNuevoUsuario(modelo);
        ValidarNuevoUsuario(modelo);

        if (!ModelState.IsValid)
        {
            return View("~/Areas/Admin/Views/Usuarios/Nuevo.cshtml", modelo);
        }

        try
        {
            var nombreUsuario = _usuarios.Crear(
                modelo.IdEmpleado!.Value,
                modelo.IdRol!.Value,
                modelo.Password);

            TempData["UsuarioCreado"] = nombreUsuario;
            return RedirectToAction(nameof(Index));
        }
        catch (SqlException ex)
        {
            ModelState.AddModelError(string.Empty, MensajeErrorNuevoUsuario(ex));
            return View("~/Areas/Admin/Views/Usuarios/Nuevo.cshtml", modelo);
        }
    }

    private Models.NuevoUsuarioModel CargarModeloNuevoUsuario()
    {
        var modelo = new Models.NuevoUsuarioModel();
        return CargarCatalogosNuevoUsuario(modelo);
    }

    private Models.NuevoUsuarioModel CargarCatalogosNuevoUsuario(Models.NuevoUsuarioModel modelo)
    {
        modelo.EmpleadosDisponibles = _empleado.ListarActivosSinUsuarioDeRecursosHumanos();
        modelo.Roles = CargarRoles();
        return modelo;
    }

    private List<BEMO.Entidades.Rol> CargarRoles()
    {
        return _usuarios.ListarRolesDisponibles();
    }

    private void ValidarNuevoUsuario(Models.NuevoUsuarioModel modelo)
    {
        if (modelo.IdEmpleado.HasValue
            && !modelo.EmpleadosDisponibles.Any(empleado => empleado.IdEmpleado == modelo.IdEmpleado.Value))
        {
            ModelState.AddModelError(nameof(modelo.IdEmpleado), "Seleccione un empleado activo sin usuario.");
        }

        if (modelo.IdRol.HasValue
            && !modelo.Roles.Any(rol => rol.IdRol == modelo.IdRol.Value))
        {
            ModelState.AddModelError(nameof(modelo.IdRol), "Seleccione un rol válido.");
        }

        if (!string.IsNullOrWhiteSpace(modelo.Password)
            && (modelo.Password.Length < 8
                || !modelo.Password.Any(char.IsUpper)
                || !modelo.Password.Any(char.IsLower)
                || !modelo.Password.Any(char.IsDigit)
                || modelo.Password.All(char.IsLetterOrDigit)))
        {
            ModelState.AddModelError(nameof(modelo.Password), "La contraseña debe tener mínimo 8 caracteres, una mayúscula, una minúscula, un número y un símbolo.");
        }
    }

    private static string MensajeErrorNuevoUsuario(SqlException ex)
    {
        if (ex.Message.Contains("ya posee una cuenta", StringComparison.OrdinalIgnoreCase))
        {
            return "El empleado seleccionado ya posee una cuenta.";
        }

        if (ex.Message.Contains("contraseña no cumple", StringComparison.OrdinalIgnoreCase))
        {
            return "La contraseña debe tener mínimo 8 caracteres, una mayúscula, una minúscula, un número y un símbolo.";
        }

        if (ex.Number is 2627 or 2601)
        {
            return "No se pudo crear el usuario porque ya existe un registro con esos datos.";
        }

        return "No se pudo crear el usuario. Verifica la información ingresada.";
    }
}
