using BEMO.Entidades;
using Microsoft.Data.SqlClient;
using System.Data;

namespace BEMO.Datos;

public class DAOCapacitacion
{
    private readonly Conexion _conexion;

    public DAOCapacitacion(Conexion conexion)
    {
        _conexion = conexion;
    }

    public List<TipoCapacitacion> ListarTipos()
    {
        const string consulta = @"
            SELECT id_tipo_capacitacion, nombre_tipo_capacitacion
            FROM dbo.TIPO_CAPACITACION
            ORDER BY nombre_tipo_capacitacion;";

        var tipos = new List<TipoCapacitacion>();
        using var conexionSql = new SqlConnection(_conexion.Cadena);
        using var cmd = new SqlCommand(consulta, conexionSql);

        conexionSql.Open();
        using var dr = cmd.ExecuteReader();
        while (dr.Read())
        {
            tipos.Add(new TipoCapacitacion
            {
                IdTipoCapacitacion = Convert.ToInt32(dr["id_tipo_capacitacion"]),
                NombreTipoCapacitacion = Convert.ToString(dr["nombre_tipo_capacitacion"]) ?? string.Empty
            });
        }

        return tipos;
    }

    public (List<CapacitacionListado> Capacitaciones, int Total) ListarPaginado(
        int pagina,
        int tamanoPagina,
        string? busqueda,
        int? idTipoCapacitacion,
        string? estado)
    {
        pagina = Math.Max(1, pagina);
        tamanoPagina = Math.Max(1, tamanoPagina);
        busqueda = NormalizarFiltro(busqueda);
        estado = NormalizarFiltro(estado)?.ToUpperInvariant();
        var desplazamiento = (pagina - 1) * tamanoPagina;

        const string consulta = @"
            WITH Resumen AS
            (
                SELECT
                    c.id_capacitacion,
                    c.id_tipo_capacitacion,
                    c.nombre_capacitacion,
                    tc.nombre_tipo_capacitacion,
                    MIN(dc.fecha_inicio) AS fecha_inicio,
                    MAX(dc.fecha_fin) AS fecha_fin,
                    COUNT(DISTINCT dc.id_empleado) AS empleados_participantes,
                    COUNT(dc.id_detalle_capacitacion) AS participaciones,
                    AVG(CAST(dc.resultado AS DECIMAL(10,2))) AS promedio_resultados,
                    MAX(dc.resultado) AS mejor_resultado,
                    MIN(dc.resultado) AS menor_resultado
                FROM dbo.CAPACITACION c
                INNER JOIN dbo.TIPO_CAPACITACION tc
                    ON tc.id_tipo_capacitacion = c.id_tipo_capacitacion
                LEFT JOIN dbo.DETALLE_CAPACITACION dc
                    ON dc.id_capacitacion = c.id_capacitacion
                GROUP BY
                    c.id_capacitacion,
                    c.id_tipo_capacitacion,
                    c.nombre_capacitacion,
                    tc.nombre_tipo_capacitacion
            ), ConEstado AS
            (
                SELECT *,
                    CASE
                        WHEN fecha_inicio IS NULL THEN 'SIN ASIGNAR'
                        WHEN fecha_fin < CAST(GETDATE() AS DATE) THEN 'FINALIZADA'
                        WHEN fecha_inicio > CAST(GETDATE() AS DATE) THEN 'PROGRAMADA'
                        ELSE 'EN CURSO'
                    END AS estado
                FROM Resumen
            )
            SELECT *, COUNT(*) OVER() AS total_registros
            FROM ConEstado
            WHERE (@busqueda IS NULL
                   OR nombre_capacitacion LIKE '%' + @busqueda + '%'
                   OR nombre_tipo_capacitacion LIKE '%' + @busqueda + '%')
              AND (@id_tipo_capacitacion IS NULL OR id_tipo_capacitacion = @id_tipo_capacitacion)
              AND (@estado IS NULL OR estado = @estado)
            ORDER BY
                CASE estado
                    WHEN 'EN CURSO' THEN 1
                    WHEN 'PROGRAMADA' THEN 2
                    WHEN 'SIN ASIGNAR' THEN 3
                    ELSE 4
                END,
                COALESCE(fecha_inicio, CAST('9999-12-31' AS DATE)),
                id_capacitacion DESC
            OFFSET @desplazamiento ROWS
            FETCH NEXT @tamano_pagina ROWS ONLY
            OPTION (RECOMPILE);";

        var capacitaciones = new List<CapacitacionListado>();
        var total = 0;
        using var conexionSql = new SqlConnection(_conexion.Cadena);
        using var cmd = new SqlCommand(consulta, conexionSql);
        cmd.Parameters.Add("@busqueda", SqlDbType.VarChar, 100).Value =
            (object?)busqueda ?? DBNull.Value;
        cmd.Parameters.Add("@id_tipo_capacitacion", SqlDbType.Int).Value =
            (object?)idTipoCapacitacion ?? DBNull.Value;
        cmd.Parameters.Add("@estado", SqlDbType.VarChar, 20).Value =
            (object?)estado ?? DBNull.Value;
        cmd.Parameters.Add("@desplazamiento", SqlDbType.Int).Value = desplazamiento;
        cmd.Parameters.Add("@tamano_pagina", SqlDbType.Int).Value = tamanoPagina;

        conexionSql.Open();
        using var dr = cmd.ExecuteReader();
        while (dr.Read())
        {
            if (total == 0)
            {
                total = Convert.ToInt32(dr["total_registros"]);
            }

            capacitaciones.Add(MapearListado(dr));
        }

        return (capacitaciones, total);
    }

    public CapacitacionListado? ObtenerPorId(int idCapacitacion)
    {
        const string consulta = @"
            WITH Resumen AS
            (
                SELECT
                    c.id_capacitacion,
                    c.id_tipo_capacitacion,
                    c.nombre_capacitacion,
                    tc.nombre_tipo_capacitacion,
                    MIN(dc.fecha_inicio) AS fecha_inicio,
                    MAX(dc.fecha_fin) AS fecha_fin,
                    COUNT(DISTINCT dc.id_empleado) AS empleados_participantes,
                    COUNT(dc.id_detalle_capacitacion) AS participaciones,
                    AVG(CAST(dc.resultado AS DECIMAL(10,2))) AS promedio_resultados,
                    MAX(dc.resultado) AS mejor_resultado,
                    MIN(dc.resultado) AS menor_resultado
                FROM dbo.CAPACITACION c
                INNER JOIN dbo.TIPO_CAPACITACION tc
                    ON tc.id_tipo_capacitacion = c.id_tipo_capacitacion
                LEFT JOIN dbo.DETALLE_CAPACITACION dc
                    ON dc.id_capacitacion = c.id_capacitacion
                WHERE c.id_capacitacion = @id_capacitacion
                GROUP BY
                    c.id_capacitacion,
                    c.id_tipo_capacitacion,
                    c.nombre_capacitacion,
                    tc.nombre_tipo_capacitacion
            )
            SELECT *,
                CASE
                    WHEN fecha_inicio IS NULL THEN 'SIN ASIGNAR'
                    WHEN fecha_fin < CAST(GETDATE() AS DATE) THEN 'FINALIZADA'
                    WHEN fecha_inicio > CAST(GETDATE() AS DATE) THEN 'PROGRAMADA'
                    ELSE 'EN CURSO'
                END AS estado
            FROM Resumen;";

        using var conexionSql = new SqlConnection(_conexion.Cadena);
        using var cmd = new SqlCommand(consulta, conexionSql);
        cmd.Parameters.Add("@id_capacitacion", SqlDbType.Int).Value = idCapacitacion;

        conexionSql.Open();
        using var dr = cmd.ExecuteReader();
        return dr.Read() ? MapearListado(dr) : null;
    }

    public List<CapacitacionParticipante> ListarParticipantes(int idCapacitacion)
    {
        return ListarParticipantesPaginado(idCapacitacion, 1, int.MaxValue).Participantes;
    }

    public (List<CapacitacionParticipante> Participantes, int Total) ListarParticipantesPaginado(
        int idCapacitacion,
        int pagina,
        int tamanoPagina)
    {
        pagina = Math.Max(1, pagina);
        tamanoPagina = Math.Max(1, tamanoPagina);
        var desplazamiento = (pagina - 1) * tamanoPagina;

        const string consulta = @"
            SELECT
                dc.id_detalle_capacitacion,
                dc.id_empleado,
                dc.id_capacitacion,
                e.dui,
                CONCAT(e.nombre, ' ', e.apellido) AS nombre_empleado,
                COALESCE(departamento.nombre_departamento, 'Sin departamento') AS nombre_departamento,
                dc.fecha_inicio,
                dc.fecha_fin,
                dc.resultado,
                COUNT(*) OVER() AS total_registros
            FROM dbo.DETALLE_CAPACITACION dc
            INNER JOIN dbo.EMPLEADO e
                ON e.id_empleado = dc.id_empleado
            OUTER APPLY
            (
                SELECT TOP (1) d.nombre_departamento
                FROM dbo.CONTRATO contrato
                INNER JOIN dbo.DEPARTAMENTO d
                    ON d.id_departamento = contrato.id_departamento
                WHERE contrato.id_empleado = e.id_empleado
                ORDER BY CASE WHEN contrato.estado = 'VIGENTE' THEN 0 ELSE 1 END,
                         contrato.fecha_contratacion DESC,
                         contrato.id_contrato DESC
            ) departamento
            WHERE dc.id_capacitacion = @id_capacitacion
            ORDER BY e.apellido, e.nombre, dc.id_detalle_capacitacion
            OFFSET @desplazamiento ROWS
            FETCH NEXT @tamano_pagina ROWS ONLY;";

        var participantes = new List<CapacitacionParticipante>();
        var total = 0;
        using var conexionSql = new SqlConnection(_conexion.Cadena);
        using var cmd = new SqlCommand(consulta, conexionSql);
        cmd.Parameters.Add("@id_capacitacion", SqlDbType.Int).Value = idCapacitacion;
        cmd.Parameters.Add("@desplazamiento", SqlDbType.Int).Value = desplazamiento;
        cmd.Parameters.Add("@tamano_pagina", SqlDbType.Int).Value = tamanoPagina;

        conexionSql.Open();
        using var dr = cmd.ExecuteReader();
        while (dr.Read())
        {
            if (total == 0)
            {
                total = Convert.ToInt32(dr["total_registros"]);
            }

            participantes.Add(new CapacitacionParticipante
            {
                IdDetalleCapacitacion = Convert.ToInt32(dr["id_detalle_capacitacion"]),
                IdEmpleado = Convert.ToInt32(dr["id_empleado"]),
                IdCapacitacion = Convert.ToInt32(dr["id_capacitacion"]),
                Dui = Convert.ToString(dr["dui"])?.Trim() ?? string.Empty,
                NombreEmpleado = Convert.ToString(dr["nombre_empleado"]) ?? string.Empty,
                NombreDepartamento = Convert.ToString(dr["nombre_departamento"]) ?? string.Empty,
                FechaInicio = Fecha(dr["fecha_inicio"]),
                FechaFin = Fecha(dr["fecha_fin"]),
                Resultado = Convert.ToDecimal(dr["resultado"])
            });
        }

        return (participantes, total);
    }

    public List<EmpleadoCapacitacionListado> ListarEmpleadosParaAsignacion(
        int idCapacitacion,
        string? busqueda,
        string? departamento)
    {
        return ListarEmpleadosParaAsignacionPaginado(
            idCapacitacion,
            busqueda,
            departamento,
            1,
            int.MaxValue).Empleados;
    }

    public (List<EmpleadoCapacitacionListado> Empleados, int Total) ListarEmpleadosParaAsignacionPaginado(
        int idCapacitacion,
        string? busqueda,
        string? departamento,
        int pagina,
        int tamanoPagina)
    {
        pagina = Math.Max(1, pagina);
        tamanoPagina = Math.Max(1, tamanoPagina);
        var desplazamiento = (pagina - 1) * tamanoPagina;
        busqueda = NormalizarFiltro(busqueda);
        departamento = NormalizarFiltro(departamento);

        const string consulta = @"
            WITH ContratosActuales AS
            (
                SELECT
                    contrato.id_empleado,
                    departamento.nombre_departamento,
                    ROW_NUMBER() OVER
                    (
                        PARTITION BY contrato.id_empleado
                        ORDER BY CASE WHEN contrato.estado = 'VIGENTE' THEN 0 ELSE 1 END,
                                 contrato.fecha_contratacion DESC,
                                 contrato.id_contrato DESC
                    ) AS fila
                FROM dbo.CONTRATO contrato
                INNER JOIN dbo.DEPARTAMENTO departamento
                    ON departamento.id_departamento = contrato.id_departamento
            )
            SELECT
                e.id_empleado,
                e.dui,
                CONCAT(e.nombre, ' ', e.apellido) AS nombre_empleado,
                COALESCE(contrato.nombre_departamento, 'Sin departamento') AS nombre_departamento,
                CAST(CASE WHEN EXISTS
                (
                    SELECT 1
                    FROM dbo.DETALLE_CAPACITACION detalle
                    WHERE detalle.id_capacitacion = @id_capacitacion
                      AND detalle.id_empleado = e.id_empleado
                ) THEN 1 ELSE 0 END AS bit) AS asignado,
                COUNT(*) OVER() AS total_registros
            FROM dbo.EMPLEADO e
            LEFT JOIN ContratosActuales contrato
                ON contrato.id_empleado = e.id_empleado
               AND contrato.fila = 1
            WHERE e.estado = 'ACTIVO'
              AND (@busqueda IS NULL
                   OR e.dui LIKE '%' + @busqueda + '%'
                   OR e.nombre LIKE '%' + @busqueda + '%'
                   OR e.apellido LIKE '%' + @busqueda + '%'
                   OR CONCAT(e.nombre, ' ', e.apellido) LIKE '%' + @busqueda + '%')
              AND (@departamento IS NULL OR contrato.nombre_departamento = @departamento)
            ORDER BY e.apellido, e.nombre, e.id_empleado
            OFFSET @desplazamiento ROWS
            FETCH NEXT @tamano_pagina ROWS ONLY;";

        var empleados = new List<EmpleadoCapacitacionListado>();
        var total = 0;
        using var conexionSql = new SqlConnection(_conexion.Cadena);
        using var cmd = new SqlCommand(consulta, conexionSql);
        cmd.Parameters.Add("@id_capacitacion", SqlDbType.Int).Value = idCapacitacion;
        cmd.Parameters.Add("@busqueda", SqlDbType.VarChar, 100).Value =
            (object?)busqueda ?? DBNull.Value;
        cmd.Parameters.Add("@departamento", SqlDbType.VarChar, 50).Value =
            (object?)departamento ?? DBNull.Value;
        cmd.Parameters.Add("@desplazamiento", SqlDbType.Int).Value = desplazamiento;
        cmd.Parameters.Add("@tamano_pagina", SqlDbType.Int).Value = tamanoPagina;

        conexionSql.Open();
        using var dr = cmd.ExecuteReader();
        while (dr.Read())
        {
            if (total == 0)
            {
                total = Convert.ToInt32(dr["total_registros"]);
            }

            empleados.Add(new EmpleadoCapacitacionListado
            {
                IdEmpleado = Convert.ToInt32(dr["id_empleado"]),
                Dui = Convert.ToString(dr["dui"])?.Trim() ?? string.Empty,
                NombreEmpleado = Convert.ToString(dr["nombre_empleado"]) ?? string.Empty,
                NombreDepartamento = Convert.ToString(dr["nombre_departamento"]) ?? string.Empty,
                Asignado = Convert.ToBoolean(dr["asignado"])
            });
        }

        return (empleados, total);
    }

    public int Registrar(string nombreCapacitacion, int idTipoCapacitacion)
    {
        using var conexionSql = new SqlConnection(_conexion.Cadena);
        using var cmd = new SqlCommand("dbo.usp_Crear_Capacitacion", conexionSql)
        {
            CommandType = CommandType.StoredProcedure
        };

        cmd.Parameters.Add("@nombre_capacitacion", SqlDbType.VarChar, 50).Value = nombreCapacitacion.Trim();
        cmd.Parameters.Add("@id_tipo_capacitacion", SqlDbType.Int).Value = idTipoCapacitacion;
        var idCapacitacion = cmd.Parameters.Add("@id_capacitacion", SqlDbType.Int);
        idCapacitacion.Direction = ParameterDirection.Output;

        conexionSql.Open();
        cmd.ExecuteNonQuery();
        return Convert.ToInt32(idCapacitacion.Value);
    }

    public void RegistrarParticipaciones(
        int idCapacitacion,
        IEnumerable<int> idsEmpleados,
        DateOnly fechaInicio,
        DateOnly fechaFin,
        decimal resultado = 0)
    {
        var ids = idsEmpleados.Distinct().ToList();
        if (ids.Count == 0)
        {
            return;
        }

        using var conexionSql = new SqlConnection(_conexion.Cadena);
        conexionSql.Open();
        using var transaccion = conexionSql.BeginTransaction();

        try
        {
            foreach (var idEmpleado in ids)
            {
                using var cmd = new SqlCommand("dbo.usp_Crear_Detalle_Capacitacion", conexionSql, transaccion)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.Add("@id_empleado", SqlDbType.Int).Value = idEmpleado;
                cmd.Parameters.Add("@id_capacitacion", SqlDbType.Int).Value = idCapacitacion;
                cmd.Parameters.Add("@fecha_inicio", SqlDbType.Date).Value = fechaInicio.ToDateTime(TimeOnly.MinValue);
                cmd.Parameters.Add("@fecha_fin", SqlDbType.Date).Value = fechaFin.ToDateTime(TimeOnly.MinValue);
                var resultadoParametro = cmd.Parameters.Add("@resultado", SqlDbType.Decimal);
                resultadoParametro.Precision = 5;
                resultadoParametro.Scale = 2;
                resultadoParametro.Value = resultado;
                var idDetalle = cmd.Parameters.Add("@id_detalle_capacitacion", SqlDbType.Int);
                idDetalle.Direction = ParameterDirection.Output;

                cmd.ExecuteNonQuery();
            }

            transaccion.Commit();
        }
        catch
        {
            transaccion.Rollback();
            throw;
        }
    }

    public void ActualizarResultado(int idDetalleCapacitacion, decimal resultado)
    {
        const string consulta = @"
            SELECT id_empleado, id_capacitacion, fecha_inicio, fecha_fin
            FROM dbo.DETALLE_CAPACITACION
            WHERE id_detalle_capacitacion = @id_detalle_capacitacion;";

        using var conexionSql = new SqlConnection(_conexion.Cadena);
        conexionSql.Open();

        int idEmpleado;
        int idCapacitacion;
        DateOnly fechaInicio;
        DateOnly fechaFin;
        using (var cmdObtener = new SqlCommand(consulta, conexionSql))
        {
            cmdObtener.Parameters.Add("@id_detalle_capacitacion", SqlDbType.Int).Value = idDetalleCapacitacion;
            using var dr = cmdObtener.ExecuteReader();
            if (!dr.Read())
            {
                throw new InvalidOperationException("La participación indicada no existe.");
            }

            idEmpleado = Convert.ToInt32(dr["id_empleado"]);
            idCapacitacion = Convert.ToInt32(dr["id_capacitacion"]);
            fechaInicio = Fecha(dr["fecha_inicio"]);
            fechaFin = Fecha(dr["fecha_fin"]);
        }

        using var cmd = new SqlCommand("dbo.usp_Actualizar_Detalle_Capacitacion", conexionSql)
        {
            CommandType = CommandType.StoredProcedure
        };
        cmd.Parameters.Add("@id_detalle_capacitacion", SqlDbType.Int).Value = idDetalleCapacitacion;
        cmd.Parameters.Add("@id_empleado", SqlDbType.Int).Value = idEmpleado;
        cmd.Parameters.Add("@id_capacitacion", SqlDbType.Int).Value = idCapacitacion;
        cmd.Parameters.Add("@fecha_inicio", SqlDbType.Date).Value = fechaInicio.ToDateTime(TimeOnly.MinValue);
        cmd.Parameters.Add("@fecha_fin", SqlDbType.Date).Value = fechaFin.ToDateTime(TimeOnly.MinValue);
        var resultadoParametro = cmd.Parameters.Add("@resultado", SqlDbType.Decimal);
        resultadoParametro.Precision = 5;
        resultadoParametro.Scale = 2;
        resultadoParametro.Value = resultado;

        cmd.ExecuteNonQuery();
    }

    private static CapacitacionListado MapearListado(SqlDataReader dr)
    {
        return new CapacitacionListado
        {
            IdCapacitacion = Convert.ToInt32(dr["id_capacitacion"]),
            IdTipoCapacitacion = Convert.ToInt32(dr["id_tipo_capacitacion"]),
            NombreCapacitacion = Convert.ToString(dr["nombre_capacitacion"]) ?? string.Empty,
            NombreTipoCapacitacion = Convert.ToString(dr["nombre_tipo_capacitacion"]) ?? string.Empty,
            FechaInicio = FechaNullable(dr["fecha_inicio"]),
            FechaFin = FechaNullable(dr["fecha_fin"]),
            EmpleadosParticipantes = Convert.ToInt32(dr["empleados_participantes"]),
            Participaciones = Convert.ToInt32(dr["participaciones"]),
            PromedioResultados = DecimalNullable(dr["promedio_resultados"]),
            MejorResultado = DecimalNullable(dr["mejor_resultado"]),
            MenorResultado = DecimalNullable(dr["menor_resultado"]),
            Estado = Convert.ToString(dr["estado"]) ?? string.Empty
        };
    }

    private static string? NormalizarFiltro(string? valor)
    {
        return string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
    }

    private static DateOnly Fecha(object valor)
    {
        return DateOnly.FromDateTime(Convert.ToDateTime(valor));
    }

    private static DateOnly? FechaNullable(object valor)
    {
        return valor == DBNull.Value ? null : Fecha(valor);
    }

    private static decimal? DecimalNullable(object valor)
    {
        return valor == DBNull.Value ? null : Convert.ToDecimal(valor);
    }
}
