using Microsoft.Data.SqlClient;

namespace BEMO.Datos;

/// <summary>
/// Almacena la cadena de conexión a SQL Server y ofrece utilidades de diagnóstico seguro.
/// </summary>
public class Conexion
{
    public string Cadena { get; }

    public Conexion(string cadena)
    {
        Cadena = cadena;
    }

    /// <summary>
    /// Verifica la conexión a SQL Server y comprueba si el usuario SQL actual puede ejecutar dbo.sp_Autenticar.
    /// </summary>
    public bool ProbarConexion(out string? errorConexion, out string? advertenciaPermisos)
    {
        errorConexion = null;
        advertenciaPermisos = null;

        try
        {
            using var conexionSql = new SqlConnection(Cadena);
            conexionSql.Open();

            VerificarPermisoAutenticacion(conexionSql, out advertenciaPermisos);
            return true;
        }
        catch (SqlException ex)
        {
            errorConexion = FormatearErrorConexion(ex);
            return false;
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            errorConexion = $"La cadena ConnectionStrings__Bemo en .env tiene un formato inválido: {PrimeraLinea(ex.Message)}";
            return false;
        }
    }

    public static string PrimeraLinea(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
        {
            return string.Empty;
        }

        return texto
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault()
            ?.Trim() ?? string.Empty;
    }

    private static void VerificarPermisoAutenticacion(SqlConnection conexionSql, out string? advertenciaPermisos)
    {
        advertenciaPermisos = null;

        try
        {
            using var cmd = new SqlCommand(
                "SELECT CASE WHEN OBJECT_ID(N'dbo.sp_Autenticar', N'P') IS NULL THEN 0 ELSE 1 END, " +
                "ISNULL(HAS_PERMS_BY_NAME(N'dbo.sp_Autenticar', N'OBJECT', N'EXECUTE'), 0);",
                conexionSql);
            using var dr = cmd.ExecuteReader();
            if (!dr.Read())
            {
                return;
            }

            var existeSp = dr.GetInt32(0) == 1;
            var puedeEjecutar = dr.GetInt32(1) == 1;

            if (!puedeEjecutar)
            {
                advertenciaPermisos = existeSp
                    ? "Tu usuario de SQL Server se conectó, pero no tiene permiso EXECUTE sobre dbo.sp_Autenticar. Ejecuta scripts/permisos_bd_remota.sql en la BD remota."
                    : "Tu usuario de SQL Server se conectó, pero no tiene permiso sobre dbo.sp_Autenticar (o no existe en esta base de datos). Revisa Database en .env y ejecuta scripts/permisos_bd_remota.sql.";
            }
        }
        catch (SqlException)
        {
            // Si la consulta de metadatos falla, el intento real de login reportará el error específico.
        }
    }

    private static string FormatearErrorConexion(SqlException ex)
    {
        return ex.Number switch
        {
            18456 => "SQL Server rechazó el inicio de sesión (código 18456). Verifica User Id y Password en tu archivo .env.",
            4060 => "Se alcanzó el servidor SQL, pero no se pudo abrir la base de datos configurada (código 4060) o el usuario SQL no tiene acceso a ella.",
            18452 => "Se intentó usar autenticación de Windows (Trusted_Connection), que no funciona de forma remota sin dominio. Usa User Id y Password en .env.",
            -2146893019 => "Error de certificado TLS al conectar con SQL Server. Agrega TrustServerCertificate=True en ConnectionStrings__Bemo dentro de .env.",
            -1 or 2 or 40 or 53 or 258 or 10060 or 10061 =>
                "No fue posible alcanzar el servidor SQL. Verifica Tailscale, la IP y puerto (,1433) en .env, TCP/IP habilitado y el firewall del servidor.",
            _ => $"Error de SQL Server al conectar (código {ex.Number}): {PrimeraLinea(ex.Message)}"
        };
    }
}