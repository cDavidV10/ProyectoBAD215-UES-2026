using BEMO.Entidades;
using Microsoft.Data.SqlClient;
using System.Data;

namespace BEMO.Datos;

public class DAOUsuario
{
    private readonly Conexion _conexion;

    public DAOUsuario(Conexion conexion)
    {
        _conexion = conexion;
    }

    public (List<UsuarioListado> Usuarios, int Total) ListarPaginado(
        int pagina,
        int tamanoPagina,
        string? busqueda,
        string? rol,
        string? estado)
    {
        pagina = Math.Max(1, pagina);
        tamanoPagina = Math.Max(1, tamanoPagina);
        busqueda = NormalizarFiltro(busqueda);
        rol = NormalizarFiltro(rol);
        estado = NormalizarFiltro(estado);
        var desplazamiento = (pagina - 1) * tamanoPagina;

        const string consulta = @"
            SELECT
                u.id_usuario,
                u.id_empleado,
                u.nombre_usuario,
                CONCAT(e.nombre, ' ', e.apellido) AS nombre_empleado,
                r.nombre_rol,
                u.estado,
                COUNT(*) OVER() AS total_registros
            FROM dbo.USUARIO u
            INNER JOIN dbo.EMPLEADO e ON e.id_empleado = u.id_empleado
            INNER JOIN dbo.ROL r ON r.id_rol = u.id_rol
            WHERE (@busqueda IS NULL
                   OR u.nombre_usuario LIKE '%' + @busqueda + '%'
                   OR e.dui LIKE '%' + @busqueda + '%'
                   OR e.nombre LIKE '%' + @busqueda + '%'
                   OR e.apellido LIKE '%' + @busqueda + '%'
                   OR CONCAT(e.nombre, ' ', e.apellido) LIKE '%' + @busqueda + '%')
              AND (@rol IS NULL OR r.nombre_rol = @rol)
              AND (@estado IS NULL OR u.estado = @estado)
            ORDER BY u.id_usuario
            OFFSET @desplazamiento ROWS
            FETCH NEXT @tamanoPagina ROWS ONLY
            OPTION (RECOMPILE);";

        var usuarios = new List<UsuarioListado>();
        var total = 0;
        using var conexionSql = new SqlConnection(_conexion.Cadena);
        using var cmd = new SqlCommand(consulta, conexionSql);
        cmd.Parameters.Add("@busqueda", SqlDbType.VarChar, 100).Value =
            (object?)busqueda ?? DBNull.Value;
        cmd.Parameters.Add("@rol", SqlDbType.VarChar, 50).Value =
            (object?)rol ?? DBNull.Value;
        cmd.Parameters.Add("@estado", SqlDbType.VarChar, 20).Value =
            (object?)estado ?? DBNull.Value;
        cmd.Parameters.Add("@desplazamiento", SqlDbType.Int).Value = desplazamiento;
        cmd.Parameters.Add("@tamanoPagina", SqlDbType.Int).Value = tamanoPagina;

        conexionSql.Open();
        using var dr = cmd.ExecuteReader();
        while (dr.Read())
        {
            if (total == 0)
            {
                total = Convert.ToInt32(dr["total_registros"]);
            }

            usuarios.Add(new UsuarioListado
            {
                IdUsuario = Convert.ToInt32(dr["id_usuario"]),
                IdEmpleado = Convert.ToInt32(dr["id_empleado"]),
                NombreUsuario = Convert.ToString(dr["nombre_usuario"]) ?? string.Empty,
                NombreEmpleado = Convert.ToString(dr["nombre_empleado"]) ?? string.Empty,
                NombreRol = Convert.ToString(dr["nombre_rol"]) ?? string.Empty,
                Estado = Convert.ToString(dr["estado"]) ?? string.Empty
            });
        }

        return (usuarios, total);
    }

    public List<string> ListarRoles()
    {
        const string consulta = @"
            SELECT nombre_rol
            FROM dbo.ROL
            ORDER BY nombre_rol;";

        var roles = new List<string>();
        using var conexionSql = new SqlConnection(_conexion.Cadena);
        using var cmd = new SqlCommand(consulta, conexionSql);

        conexionSql.Open();
        using var dr = cmd.ExecuteReader();
        while (dr.Read())
        {
            roles.Add(Convert.ToString(dr["nombre_rol"]) ?? string.Empty);
        }

        return roles;
    }

    public List<Rol> ListarRolesDisponibles()
    {
        const string consulta = @"
            SELECT id_rol, nombre_rol
            FROM dbo.ROL
            ORDER BY nombre_rol;";

        var roles = new List<Rol>();
        using var conexionSql = new SqlConnection(_conexion.Cadena);
        using var cmd = new SqlCommand(consulta, conexionSql);

        conexionSql.Open();
        using var dr = cmd.ExecuteReader();
        while (dr.Read())
        {
            roles.Add(new Rol
            {
                IdRol = Convert.ToInt32(dr["id_rol"]),
                NombreRol = Convert.ToString(dr["nombre_rol"]) ?? string.Empty
            });
        }

        return roles;
    }

    public string Crear(int idEmpleado, int idRol, string password)
    {
        using var conexionSql = new SqlConnection(_conexion.Cadena);
        using var cmd = new SqlCommand("dbo.sp_Crear_Usuario", conexionSql)
        {
            CommandType = CommandType.StoredProcedure
        };

        cmd.Parameters.Add("@id_empleado", SqlDbType.Int).Value = idEmpleado;
        cmd.Parameters.Add("@id_rol", SqlDbType.Int).Value = idRol;
        cmd.Parameters.Add("@password_normal", SqlDbType.VarChar, 255).Value = password;
        var nombreUsuario = cmd.Parameters.Add("@nombre_usuario", SqlDbType.VarChar, 50);
        nombreUsuario.Direction = ParameterDirection.Output;

        conexionSql.Open();
        cmd.ExecuteNonQuery();

        return Convert.ToString(nombreUsuario.Value) ?? string.Empty;
    }

    private static string? NormalizarFiltro(string? filtro)
    {
        return string.IsNullOrWhiteSpace(filtro) ? null : filtro.Trim();
    }
}
