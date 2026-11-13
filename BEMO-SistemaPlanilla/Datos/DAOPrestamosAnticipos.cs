using BEMO.Entidades;
using Microsoft.Data.SqlClient;
using System.Data;

namespace BEMO.Datos;

public class DAOPrestamosAnticipos
{
    private readonly Conexion _conexion;

    public DAOPrestamosAnticipos(Conexion conexion)
    {
        _conexion = conexion;
    }

    public List<Empleado> ListarEmpleadosActivos()
    {
        const string consulta = @"
            SELECT e.id_empleado, e.dui, e.nombre, e.apellido, e.estado
            FROM dbo.EMPLEADO e
            WHERE e.estado = 'ACTIVO'
              AND EXISTS
              (
                  SELECT 1
                  FROM dbo.CONTRATO c
                  WHERE c.id_empleado = e.id_empleado
                    AND c.estado = 'VIGENTE'
              )
            ORDER BY e.apellido, e.nombre, e.id_empleado;";

        var empleados = new List<Empleado>();
        using var conexionSql = new SqlConnection(_conexion.Cadena);
        using var cmd = new SqlCommand(consulta, conexionSql);

        conexionSql.Open();
        using var dr = cmd.ExecuteReader();
        while (dr.Read())
        {
            empleados.Add(new Empleado
            {
                IdEmpleado = Convert.ToInt32(dr["id_empleado"]),
                Dui = Convert.ToString(dr["dui"])?.Trim() ?? string.Empty,
                Nombre = Convert.ToString(dr["nombre"]) ?? string.Empty,
                Apellido = Convert.ToString(dr["apellido"]) ?? string.Empty,
                Estado = Convert.ToString(dr["estado"]) ?? string.Empty
            });
        }

        return empleados;
    }

    public List<PrestamoListado> ListarPrestamos(string? busqueda = null, string? estado = null)
    {
        const string consulta = @"
            SELECT
                pr.id_prestamo,
                pr.id_empleado,
                CONCAT(e.nombre, ' ', e.apellido) AS nombre_empleado,
                e.dui,
                pr.fecha_otorgamiento,
                pr.fecha_finalizacion,
                pr.monto,
                pr.num_cuotas,
                pr.cuota_quincenal,
                pr.saldo_pendiente,
                CAST(CEILING(pr.saldo_pendiente / NULLIF(pr.cuota_quincenal, 0)) AS INT) AS cuotas_restantes,
                pr.estado
            FROM dbo.PRESTAMO pr
            INNER JOIN dbo.EMPLEADO e ON e.id_empleado = pr.id_empleado
            WHERE (@busqueda IS NULL
                   OR e.dui LIKE '%' + @busqueda + '%'
                   OR e.nombre LIKE '%' + @busqueda + '%'
                   OR e.apellido LIKE '%' + @busqueda + '%'
                   OR CONCAT(e.nombre, ' ', e.apellido) LIKE '%' + @busqueda + '%')
              AND (@estado IS NULL OR pr.estado = @estado)
            ORDER BY pr.fecha_otorgamiento DESC, pr.id_prestamo DESC;";

        var prestamos = new List<PrestamoListado>();
        using var conexionSql = new SqlConnection(_conexion.Cadena);
        using var cmd = new SqlCommand(consulta, conexionSql);
        cmd.Parameters.Add("@busqueda", SqlDbType.VarChar, 100).Value =
            (object?)Normalizar(busqueda) ?? DBNull.Value;
        cmd.Parameters.Add("@estado", SqlDbType.VarChar, 20).Value =
            (object?)Normalizar(estado) ?? DBNull.Value;

        conexionSql.Open();
        using var dr = cmd.ExecuteReader();
        while (dr.Read())
        {
            prestamos.Add(new PrestamoListado
            {
                IdPrestamo = Convert.ToInt32(dr["id_prestamo"]),
                IdEmpleado = Convert.ToInt32(dr["id_empleado"]),
                NombreEmpleado = Convert.ToString(dr["nombre_empleado"]) ?? string.Empty,
                Dui = Convert.ToString(dr["dui"])?.Trim() ?? string.Empty,
                FechaOtorgamiento = Fecha(dr["fecha_otorgamiento"]),
                FechaFinalizacion = FechaNullable(dr["fecha_finalizacion"]),
                Monto = Convert.ToDecimal(dr["monto"]),
                NumCuotas = Convert.ToInt32(dr["num_cuotas"]),
                CuotaQuincenal = Convert.ToDecimal(dr["cuota_quincenal"]),
                SaldoPendiente = Convert.ToDecimal(dr["saldo_pendiente"]),
                CuotasRestantes = Convert.ToInt32(dr["cuotas_restantes"]),
                Estado = Convert.ToString(dr["estado"]) ?? string.Empty
            });
        }

        return prestamos;
    }

    public PrestamoListado? ObtenerPrestamo(int idPrestamo)
    {
        return ListarPrestamos().FirstOrDefault(item => item.IdPrestamo == idPrestamo);
    }

    public List<AnticipoSalarioListado> ListarAnticipos(string? busqueda = null, string? estado = null)
    {
        const string consulta = @"
            SELECT
                a.id_anticipo,
                a.id_empleado,
                CONCAT(e.nombre, ' ', e.apellido) AS nombre_empleado,
                e.dui,
                a.monto_anticipo,
                a.fecha_anticipo,
                a.estado
            FROM dbo.ANTICIPO_SALARIO a
            INNER JOIN dbo.EMPLEADO e ON e.id_empleado = a.id_empleado
            WHERE (@busqueda IS NULL
                   OR e.dui LIKE '%' + @busqueda + '%'
                   OR e.nombre LIKE '%' + @busqueda + '%'
                   OR e.apellido LIKE '%' + @busqueda + '%'
                   OR CONCAT(e.nombre, ' ', e.apellido) LIKE '%' + @busqueda + '%')
              AND (@estado IS NULL OR a.estado = @estado)
            ORDER BY a.fecha_anticipo DESC, a.id_anticipo DESC;";

        var anticipos = new List<AnticipoSalarioListado>();
        using var conexionSql = new SqlConnection(_conexion.Cadena);
        using var cmd = new SqlCommand(consulta, conexionSql);
        cmd.Parameters.Add("@busqueda", SqlDbType.VarChar, 100).Value =
            (object?)Normalizar(busqueda) ?? DBNull.Value;
        cmd.Parameters.Add("@estado", SqlDbType.VarChar, 20).Value =
            (object?)Normalizar(estado) ?? DBNull.Value;

        conexionSql.Open();
        using var dr = cmd.ExecuteReader();
        while (dr.Read())
        {
            anticipos.Add(new AnticipoSalarioListado
            {
                IdAnticipo = Convert.ToInt32(dr["id_anticipo"]),
                IdEmpleado = Convert.ToInt32(dr["id_empleado"]),
                NombreEmpleado = Convert.ToString(dr["nombre_empleado"]) ?? string.Empty,
                Dui = Convert.ToString(dr["dui"])?.Trim() ?? string.Empty,
                MontoAnticipo = Convert.ToDecimal(dr["monto_anticipo"]),
                FechaAnticipo = Fecha(dr["fecha_anticipo"]),
                Estado = Convert.ToString(dr["estado"]) ?? string.Empty
            });
        }

        return anticipos;
    }

    public AnticipoSalarioListado? ObtenerAnticipo(int idAnticipo)
    {
        return ListarAnticipos().FirstOrDefault(item => item.IdAnticipo == idAnticipo);
    }

    public PrestamosAnticiposResumen ObtenerResumen()
    {
        const string consulta = @"
            SELECT
                (SELECT COUNT(*) FROM dbo.PRESTAMO WHERE estado = 'ACTIVO') AS prestamos_activos,
                (SELECT COUNT(*) FROM dbo.PRESTAMO WHERE estado = 'PAGADO') AS prestamos_pagados,
                (SELECT ISNULL(SUM(saldo_pendiente), 0) FROM dbo.PRESTAMO WHERE estado = 'ACTIVO') AS saldo_prestamos,
                (SELECT COUNT(*) FROM dbo.ANTICIPO_SALARIO WHERE estado = 'PENDIENTE') AS anticipos_pendientes,
                (SELECT COUNT(*) FROM dbo.ANTICIPO_SALARIO WHERE estado = 'DESCONTADO') AS anticipos_descontados,
                (SELECT ISNULL(SUM(monto_anticipo), 0) FROM dbo.ANTICIPO_SALARIO WHERE estado = 'PENDIENTE') AS monto_anticipos_pendientes;";

        using var conexionSql = new SqlConnection(_conexion.Cadena);
        using var cmd = new SqlCommand(consulta, conexionSql);
        conexionSql.Open();
        using var dr = cmd.ExecuteReader();
        if (!dr.Read())
        {
            return new PrestamosAnticiposResumen();
        }

        return new PrestamosAnticiposResumen
        {
            PrestamosActivos = Convert.ToInt32(dr["prestamos_activos"]),
            PrestamosPagados = Convert.ToInt32(dr["prestamos_pagados"]),
            SaldoPrestamos = Convert.ToDecimal(dr["saldo_prestamos"]),
            AnticiposPendientes = Convert.ToInt32(dr["anticipos_pendientes"]),
            AnticiposDescontados = Convert.ToInt32(dr["anticipos_descontados"]),
            MontoAnticiposPendientes = Convert.ToDecimal(dr["monto_anticipos_pendientes"])
        };
    }

    public int? ObtenerPlazoPrestamo(decimal monto)
    {
        const string consulta = "SELECT dbo.fn_Plazo_Prestamo(@monto);";
        using var conexionSql = new SqlConnection(_conexion.Cadena);
        using var cmd = new SqlCommand(consulta, conexionSql);
        AgregarDecimal(cmd, "@monto", monto);
        conexionSql.Open();
        var resultado = cmd.ExecuteScalar();
        return resultado is null || resultado == DBNull.Value ? null : Convert.ToInt32(resultado);
    }

    public decimal? ObtenerMaximoAnticipo(int idEmpleado, DateOnly fecha)
    {
        const string consulta = @"
            SELECT maximo_anticipo
            FROM dbo.fn_Maximo_Anticipo(@id_empleado, @fecha);";
        using var conexionSql = new SqlConnection(_conexion.Cadena);
        using var cmd = new SqlCommand(consulta, conexionSql);
        cmd.Parameters.Add("@id_empleado", SqlDbType.Int).Value = idEmpleado;
        cmd.Parameters.Add("@fecha", SqlDbType.Date).Value = fecha.ToDateTime(TimeOnly.MinValue);
        conexionSql.Open();
        var resultado = cmd.ExecuteScalar();
        return resultado is null || resultado == DBNull.Value ? null : Convert.ToDecimal(resultado);
    }

    public int RegistrarPrestamo(int idEmpleado, decimal monto, DateOnly fechaOtorgamiento)
    {
        using var conexionSql = new SqlConnection(_conexion.Cadena);
        using var cmd = new SqlCommand("dbo.usp_Registrar_Prestamo", conexionSql)
        {
            CommandType = CommandType.StoredProcedure
        };

        cmd.Parameters.Add("@id_empleado", SqlDbType.Int).Value = idEmpleado;
        AgregarDecimal(cmd, "@monto", monto);
        cmd.Parameters.Add("@fecha_otorgamiento", SqlDbType.Date).Value =
            fechaOtorgamiento.ToDateTime(TimeOnly.MinValue);
        var idPrestamo = cmd.Parameters.Add("@id_prestamo", SqlDbType.Int);
        idPrestamo.Direction = ParameterDirection.Output;

        conexionSql.Open();
        cmd.ExecuteNonQuery();
        return Convert.ToInt32(idPrestamo.Value);
    }

    public int RegistrarAnticipo(int idEmpleado, decimal monto, DateOnly fechaAnticipo)
    {
        using var conexionSql = new SqlConnection(_conexion.Cadena);
        using var cmd = new SqlCommand("dbo.usp_Registrar_Anticipo", conexionSql)
        {
            CommandType = CommandType.StoredProcedure
        };

        cmd.Parameters.Add("@id_empleado", SqlDbType.Int).Value = idEmpleado;
        AgregarDecimal(cmd, "@monto", monto);
        cmd.Parameters.Add("@fecha_anticipo", SqlDbType.Date).Value =
            fechaAnticipo.ToDateTime(TimeOnly.MinValue);
        var idAnticipo = cmd.Parameters.Add("@id_anticipo", SqlDbType.Int);
        idAnticipo.Direction = ParameterDirection.Output;

        conexionSql.Open();
        cmd.ExecuteNonQuery();
        return Convert.ToInt32(idAnticipo.Value);
    }

    private static void AgregarDecimal(SqlCommand cmd, string nombre, decimal valor)
    {
        var parametro = cmd.Parameters.Add(nombre, SqlDbType.Decimal);
        parametro.Precision = 10;
        parametro.Scale = 2;
        parametro.Value = valor;
    }

    private static string? Normalizar(string? valor)
    {
        return string.IsNullOrWhiteSpace(valor) ? null : valor.Trim().ToUpperInvariant();
    }

    private static DateOnly Fecha(object valor)
    {
        return DateOnly.FromDateTime(Convert.ToDateTime(valor));
    }

    private static DateOnly? FechaNullable(object valor)
    {
        return valor == DBNull.Value ? null : Fecha(valor);
    }
}
