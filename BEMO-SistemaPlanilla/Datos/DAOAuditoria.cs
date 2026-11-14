using BEMO.Entidades;
using Microsoft.Data.SqlClient;
using System.Data;

namespace BEMO.Datos;

public class DAOAuditoria
{
    private readonly Conexion _conexion;

    public DAOAuditoria(Conexion conexion)
    {
        _conexion = conexion;
    }

    public (List<AuditoriaListado> Auditorias, int Total) ListarPaginado(
        int pagina,
        int tamanoPagina,
        DateOnly? fechaDesde,
        DateOnly? fechaHasta,
        string? nombreUsuario,
        string? modulo,
        string? operacion)
    {
        pagina = Math.Max(1, pagina);
        tamanoPagina = Math.Max(1, tamanoPagina);
        nombreUsuario = NormalizarFiltro(nombreUsuario);
        modulo = NormalizarFiltro(modulo);
        operacion = NormalizarFiltro(operacion);
        var desplazamiento = (pagina - 1) * tamanoPagina;

        var condiciones = new List<string>();
        if (fechaDesde.HasValue)
        {
            condiciones.Add("a.fecha_aplicacion >= @fecha_desde");
        }

        if (fechaHasta.HasValue)
        {
            condiciones.Add("a.fecha_aplicacion < DATEADD(DAY, 1, @fecha_hasta)");
        }

        if (nombreUsuario is not null)
        {
            condiciones.Add("ISNULL(u.nombre_usuario, 'SISTEMA') = @nombre_usuario");
        }

        var filtroModulo = FiltroModulo(modulo);
        if (filtroModulo is not null)
        {
            condiciones.Add(filtroModulo);
        }

        var filtroOperacion = FiltroOperacion(operacion);
        if (filtroOperacion is not null)
        {
            condiciones.Add(filtroOperacion);
        }

        const string origen = @"
            FROM dbo.AUDITORIA a
            LEFT JOIN dbo.USUARIO u ON u.id_usuario = a.codigo_usuario";
        var where = condiciones.Count == 0
            ? string.Empty
            : " WHERE " + string.Join(" AND ", condiciones);
        var consultaTotal = "SELECT COUNT(*) " + origen + where + " OPTION (RECOMPILE);";

        var auditorias = new List<AuditoriaListado>();
        using var conexionSql = new SqlConnection(_conexion.Cadena);
        conexionSql.Open();

        int total;
        using (var cmdTotal = new SqlCommand(consultaTotal, conexionSql))
        {
            AgregarFiltros(cmdTotal, fechaDesde, fechaHasta, nombreUsuario, modulo, operacion);
            var resultadoTotal = cmdTotal.ExecuteScalar();
            total = resultadoTotal == null || resultadoTotal == DBNull.Value
                ? 0
                : Convert.ToInt32(resultadoTotal);
        }

        if (total == 0 || desplazamiento >= total)
        {
            return (auditorias, total);
        }

        // Para la última página se consulta desde el extremo más cercano y se invierte
        // el resultado. Así no se obliga a SQL Server a recorrer todo el OFFSET.
        var invertirOrden = desplazamiento > total / 2;
        var desplazamientoConsulta = invertirOrden
            ? Math.Max(0, total - desplazamiento - tamanoPagina)
            : desplazamiento;
        var registrosSolicitados = Math.Min(tamanoPagina, total - desplazamiento);
        var direccion = invertirOrden ? "ASC" : "DESC";

        var consulta = @"
            SELECT
                a.id_auditoria,
                a.fecha_aplicacion AS fecha_hora,
                a.codigo_usuario AS id_usuario,
                ISNULL(u.nombre_usuario, 'SISTEMA') AS nombre_usuario,
                CASE UPPER(a.entidad_afectada)
                    WHEN 'EMPLEADO' THEN 'Empleados'
                    WHEN 'CONTRATO' THEN 'Contratos'
                    WHEN 'PLANILLA' THEN 'Planilla'
                    WHEN 'DETALLE_PLANILLA' THEN 'Planilla'
                    WHEN 'DETALLE_DESCUENTO_PLANILLA' THEN 'Planilla'
                    WHEN 'PRESTAMO' THEN 'Prestamos y anticipos'
                    WHEN 'ANTICIPO_SALARIO' THEN 'Prestamos y anticipos'
                    WHEN 'DESCUENTO_LEY' THEN 'Parametros legales'
                    WHEN 'RENTA' THEN 'Parametros legales'
                    WHEN 'CAPACITACION' THEN 'Capacitaciones'
                    WHEN 'DETALLE_CAPACITACION' THEN 'Capacitaciones'
                    WHEN 'USUARIO' THEN 'Usuarios y roles'
                    ELSE a.entidad_afectada
                END AS modulo,
                CASE UPPER(a.accion)
                    WHEN 'INSERT' THEN 'Insercion'
                    WHEN 'UPDATE' THEN 'Actualizacion'
                    WHEN 'DELETE' THEN 'Eliminacion'
                    WHEN 'CAMBIO DE ESTADO' THEN 'Cambio de estado'
                    WHEN 'REAPERTURA' THEN 'Reapertura'
                    ELSE a.accion
                END AS operacion,
                a.entidad_afectada + ' CON ID: ' + CAST(a.registro_afectado AS VARCHAR(20)) AS registro,
                a.campo_modificado,
                a.valor_viejo,
                a.valor_nuevo
            " + origen + where + $@"
            ORDER BY a.fecha_aplicacion {direccion}, a.id_auditoria {direccion}
            OFFSET @desplazamiento ROWS
            FETCH NEXT @tamano_pagina ROWS ONLY
            OPTION (RECOMPILE);";

        using var cmd = new SqlCommand(consulta, conexionSql);
        AgregarFiltros(cmd, fechaDesde, fechaHasta, nombreUsuario, modulo, operacion);
        cmd.Parameters.Add("@desplazamiento", SqlDbType.Int).Value = desplazamientoConsulta;
        cmd.Parameters.Add("@tamano_pagina", SqlDbType.Int).Value = registrosSolicitados;

        using var dr = cmd.ExecuteReader();
        while (dr.Read())
        {
            auditorias.Add(Mapear(dr));
        }

        if (invertirOrden)
        {
            auditorias.Reverse();
        }

        return (auditorias, total);
    }

    public AuditoriaListado? ObtenerPorId(int idAuditoria)
    {
        const string consulta = @"
            SELECT TOP (1)
                id_auditoria,
                fecha_hora,
                id_usuario,
                nombre_usuario,
                modulo,
                operacion,
                registro,
                campo_modificado,
                valor_viejo,
                valor_nuevo
            FROM dbo.vw_auditoria_general
            WHERE id_auditoria = @id_auditoria;";

        using var conexionSql = new SqlConnection(_conexion.Cadena);
        using var cmd = new SqlCommand(consulta, conexionSql);
        cmd.Parameters.Add("@id_auditoria", SqlDbType.Int).Value = idAuditoria;

        conexionSql.Open();
        using var dr = cmd.ExecuteReader();
        return dr.Read() ? Mapear(dr) : null;
    }

    public List<string> ListarUsuarios()
    {
        return ListarValores("nombre_usuario");
    }

    public List<string> ListarModulos()
    {
        return ListarValores("modulo");
    }

    public List<string> ListarOperaciones()
    {
        return ListarValores("operacion");
    }

    private static string? FiltroModulo(string? modulo)
    {
        return modulo switch
        {
            null => null,
            "Empleados" => "UPPER(a.entidad_afectada) = 'EMPLEADO'",
            "Contratos" => "UPPER(a.entidad_afectada) = 'CONTRATO'",
            "Planilla" => "UPPER(a.entidad_afectada) IN ('PLANILLA', 'DETALLE_PLANILLA', 'DETALLE_DESCUENTO_PLANILLA')",
            "Prestamos y anticipos" => "UPPER(a.entidad_afectada) IN ('PRESTAMO', 'ANTICIPO_SALARIO')",
            "Parametros legales" => "UPPER(a.entidad_afectada) IN ('DESCUENTO_LEY', 'RENTA')",
            "Capacitaciones" => "UPPER(a.entidad_afectada) IN ('CAPACITACION', 'DETALLE_CAPACITACION')",
            "Usuarios y roles" => "UPPER(a.entidad_afectada) = 'USUARIO'",
            _ => "a.entidad_afectada = @modulo"
        };
    }

    private static string? FiltroOperacion(string? operacion)
    {
        return operacion switch
        {
            null => null,
            "Insercion" => "UPPER(LTRIM(RTRIM(a.accion))) = 'INSERT'",
            "Actualizacion" => "UPPER(LTRIM(RTRIM(a.accion))) = 'UPDATE'",
            "Eliminacion" => "UPPER(LTRIM(RTRIM(a.accion))) = 'DELETE'",
            "Cambio de estado" => "UPPER(LTRIM(RTRIM(a.accion))) = 'CAMBIO DE ESTADO'",
            "Reapertura" => "UPPER(LTRIM(RTRIM(a.accion))) = 'REAPERTURA'",
            _ => "UPPER(LTRIM(RTRIM(a.accion))) = UPPER(@operacion)"
        };
    }

    private List<string> ListarValores(string columna)
    {
        var valores = new List<string>();
        var consulta = $@"
            SELECT DISTINCT {columna}
            FROM dbo.vw_auditoria_general
            WHERE {columna} IS NOT NULL
              AND LTRIM(RTRIM({columna})) <> ''
            ORDER BY {columna};";

        using var conexionSql = new SqlConnection(_conexion.Cadena);
        using var cmd = new SqlCommand(consulta, conexionSql);

        conexionSql.Open();
        using var dr = cmd.ExecuteReader();
        while (dr.Read())
        {
            var valor = Convert.ToString(dr[0])?.Trim();
            if (!string.IsNullOrWhiteSpace(valor))
            {
                valores.Add(valor);
            }
        }

        return valores;
    }

    private static void AgregarFiltros(
        SqlCommand cmd,
        DateOnly? fechaDesde,
        DateOnly? fechaHasta,
        string? nombreUsuario,
        string? modulo,
        string? operacion)
    {
        cmd.Parameters.Add("@fecha_desde", SqlDbType.Date).Value =
            (object?)fechaDesde?.ToDateTime(TimeOnly.MinValue) ?? DBNull.Value;
        cmd.Parameters.Add("@fecha_hasta", SqlDbType.Date).Value =
            (object?)fechaHasta?.ToDateTime(TimeOnly.MinValue) ?? DBNull.Value;
        cmd.Parameters.Add("@nombre_usuario", SqlDbType.VarChar, 50).Value =
            (object?)nombreUsuario ?? DBNull.Value;
        cmd.Parameters.Add("@modulo", SqlDbType.VarChar, 50).Value =
            (object?)modulo ?? DBNull.Value;
        cmd.Parameters.Add("@operacion", SqlDbType.VarChar, 100).Value =
            (object?)operacion ?? DBNull.Value;
    }

    private static AuditoriaListado Mapear(SqlDataReader dr)
    {
        return new AuditoriaListado
        {
            IdAuditoria = Convert.ToInt32(dr["id_auditoria"]),
            FechaHora = Convert.ToDateTime(dr["fecha_hora"]),
            IdUsuario = Convert.ToInt32(dr["id_usuario"]),
            NombreUsuario = Texto(dr["nombre_usuario"]),
            Modulo = Texto(dr["modulo"]),
            Operacion = Texto(dr["operacion"]),
            Registro = Texto(dr["registro"]),
            CampoModificado = Texto(dr["campo_modificado"]),
            ValorViejo = TextoNullable(dr["valor_viejo"]),
            ValorNuevo = Texto(dr["valor_nuevo"])
        };
    }

    private static string? NormalizarFiltro(string? valor)
    {
        return string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
    }

    private static string Texto(object valor)
    {
        return valor == DBNull.Value ? string.Empty : Convert.ToString(valor)?.Trim() ?? string.Empty;
    }

    private static string? TextoNullable(object valor)
    {
        return valor == DBNull.Value ? null : Convert.ToString(valor)?.Trim();
    }
}
