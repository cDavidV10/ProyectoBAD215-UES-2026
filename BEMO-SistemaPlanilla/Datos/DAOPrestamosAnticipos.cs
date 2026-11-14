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

    public (List<Empleado> Empleados, int Total) ListarEmpleadosActivosPaginado(
        int pagina,
        int tamanoPagina,
        string? busqueda = null)
    {
        pagina = Math.Max(1, pagina);
        tamanoPagina = Math.Max(1, tamanoPagina);
        var desplazamiento = (pagina - 1) * tamanoPagina;

        const string consulta = @"
            SELECT
                e.id_empleado,
                e.dui,
                e.nombre,
                e.apellido,
                e.estado,
                COUNT(*) OVER() AS total_registros
            FROM dbo.EMPLEADO e
            WHERE e.estado = 'ACTIVO'
              AND EXISTS
              (
                  SELECT 1
                  FROM dbo.CONTRATO c
                  WHERE c.id_empleado = e.id_empleado
                    AND c.estado = 'VIGENTE'
              )
              AND (@busqueda IS NULL
                   OR e.dui LIKE '%' + @busqueda + '%'
                   OR e.nombre LIKE '%' + @busqueda + '%'
                   OR e.apellido LIKE '%' + @busqueda + '%'
                   OR CONCAT(e.nombre, ' ', e.apellido) LIKE '%' + @busqueda + '%')
            ORDER BY e.apellido, e.nombre, e.id_empleado
            OFFSET @desplazamiento ROWS FETCH NEXT @tamano_pagina ROWS ONLY;";

        var empleados = new List<Empleado>();
        var total = 0;
        using var conexionSql = new SqlConnection(_conexion.Cadena);
        using var cmd = new SqlCommand(consulta, conexionSql);
        cmd.Parameters.Add("@busqueda", SqlDbType.VarChar, 100).Value =
            (object?)Normalizar(busqueda) ?? DBNull.Value;
        cmd.Parameters.Add("@desplazamiento", SqlDbType.Int).Value = desplazamiento;
        cmd.Parameters.Add("@tamano_pagina", SqlDbType.Int).Value = tamanoPagina;

        conexionSql.Open();
        using var dr = cmd.ExecuteReader();
        while (dr.Read())
        {
            total = Convert.ToInt32(dr["total_registros"]);
            empleados.Add(new Empleado
            {
                IdEmpleado = Convert.ToInt32(dr["id_empleado"]),
                Dui = Convert.ToString(dr["dui"])?.Trim() ?? string.Empty,
                Nombre = Convert.ToString(dr["nombre"]) ?? string.Empty,
                Apellido = Convert.ToString(dr["apellido"]) ?? string.Empty,
                Estado = Convert.ToString(dr["estado"]) ?? string.Empty
            });
        }

        return (empleados, total);
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

    public (List<PrestamoListado> Prestamos, int Total) ListarPrestamosPaginado(
        int pagina,
        int tamanoPagina,
        string? busqueda = null,
        string? estado = null)
    {
        pagina = Math.Max(1, pagina);
        tamanoPagina = Math.Max(1, tamanoPagina);
        var desplazamiento = (pagina - 1) * tamanoPagina;

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
                pr.estado,
                COUNT(*) OVER() AS total_registros
            FROM dbo.PRESTAMO pr
            INNER JOIN dbo.EMPLEADO e ON e.id_empleado = pr.id_empleado
            WHERE (@busqueda IS NULL
                   OR e.dui LIKE '%' + @busqueda + '%'
                   OR e.nombre LIKE '%' + @busqueda + '%'
                   OR e.apellido LIKE '%' + @busqueda + '%'
                   OR CONCAT(e.nombre, ' ', e.apellido) LIKE '%' + @busqueda + '%')
              AND (@estado IS NULL OR pr.estado = @estado)
            ORDER BY pr.fecha_otorgamiento DESC, pr.id_prestamo DESC
            OFFSET @desplazamiento ROWS FETCH NEXT @tamano_pagina ROWS ONLY;";

        var prestamos = new List<PrestamoListado>();
        var total = 0;
        using var conexionSql = new SqlConnection(_conexion.Cadena);
        using var cmd = new SqlCommand(consulta, conexionSql);
        cmd.Parameters.Add("@busqueda", SqlDbType.VarChar, 100).Value =
            (object?)Normalizar(busqueda) ?? DBNull.Value;
        cmd.Parameters.Add("@estado", SqlDbType.VarChar, 20).Value =
            (object?)Normalizar(estado) ?? DBNull.Value;
        cmd.Parameters.Add("@desplazamiento", SqlDbType.Int).Value = desplazamiento;
        cmd.Parameters.Add("@tamano_pagina", SqlDbType.Int).Value = tamanoPagina;

        conexionSql.Open();
        using var dr = cmd.ExecuteReader();
        while (dr.Read())
        {
            total = Convert.ToInt32(dr["total_registros"]);
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

        return (prestamos, total);
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

    public (List<AnticipoSalarioListado> Anticipos, int Total) ListarAnticiposPaginado(
        int pagina,
        int tamanoPagina,
        string? busqueda = null,
        string? estado = null)
    {
        pagina = Math.Max(1, pagina);
        tamanoPagina = Math.Max(1, tamanoPagina);
        var desplazamiento = (pagina - 1) * tamanoPagina;

        const string consulta = @"
            SELECT
                a.id_anticipo,
                a.id_empleado,
                CONCAT(e.nombre, ' ', e.apellido) AS nombre_empleado,
                e.dui,
                a.monto_anticipo,
                a.fecha_anticipo,
                a.estado,
                COUNT(*) OVER() AS total_registros
            FROM dbo.ANTICIPO_SALARIO a
            INNER JOIN dbo.EMPLEADO e ON e.id_empleado = a.id_empleado
            WHERE (@busqueda IS NULL
                   OR e.dui LIKE '%' + @busqueda + '%'
                   OR e.nombre LIKE '%' + @busqueda + '%'
                   OR e.apellido LIKE '%' + @busqueda + '%'
                   OR CONCAT(e.nombre, ' ', e.apellido) LIKE '%' + @busqueda + '%')
              AND (@estado IS NULL OR a.estado = @estado)
            ORDER BY a.fecha_anticipo DESC, a.id_anticipo DESC
            OFFSET @desplazamiento ROWS FETCH NEXT @tamano_pagina ROWS ONLY;";

        var anticipos = new List<AnticipoSalarioListado>();
        var total = 0;
        using var conexionSql = new SqlConnection(_conexion.Cadena);
        using var cmd = new SqlCommand(consulta, conexionSql);
        cmd.Parameters.Add("@busqueda", SqlDbType.VarChar, 100).Value =
            (object?)Normalizar(busqueda) ?? DBNull.Value;
        cmd.Parameters.Add("@estado", SqlDbType.VarChar, 20).Value =
            (object?)Normalizar(estado) ?? DBNull.Value;
        cmd.Parameters.Add("@desplazamiento", SqlDbType.Int).Value = desplazamiento;
        cmd.Parameters.Add("@tamano_pagina", SqlDbType.Int).Value = tamanoPagina;

        conexionSql.Open();
        using var dr = cmd.ExecuteReader();
        while (dr.Read())
        {
            total = Convert.ToInt32(dr["total_registros"]);
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

        return (anticipos, total);
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

    public EmpleadoPrestamoInfo? ObtenerInformacionEmpleado(int idEmpleado, DateOnly fecha)
    {
        const string consulta = @"
            SELECT TOP (1)
                e.id_empleado,
                CONCAT(e.nombre, ' ', e.apellido) AS nombre_empleado,
                ca.nombre_cargo,
                c.salario_bruto,
                CAST(c.salario_bruto / 2.0 AS DECIMAL(10,2)) AS salario_quincenal,
                c.fecha_culminacion,
                ISNULL((
                    SELECT SUM(p.cuota_quincenal)
                    FROM dbo.PRESTAMO p
                    WHERE p.id_empleado = e.id_empleado
                      AND p.estado = 'ACTIVO'
                ), 0) AS cuota_prestamo_activa,
                ISNULL((
                    SELECT SUM(a.monto_anticipo)
                    FROM dbo.ANTICIPO_SALARIO a
                    WHERE a.id_empleado = e.id_empleado
                      AND a.estado = 'PENDIENTE'
                ), 0) AS anticipo_pendiente,
                ISNULL(est.liquido_estimado, 0) AS liquido_estimado,
                CAST(CASE WHEN EXISTS (
                    SELECT 1
                    FROM dbo.PRESTAMO p
                    WHERE p.id_empleado = e.id_empleado
                      AND p.estado = 'ACTIVO'
                ) THEN 1 ELSE 0 END AS bit) AS tiene_prestamo_activo,
                CAST(CASE WHEN EXISTS (
                    SELECT 1
                    FROM dbo.ANTICIPO_SALARIO a
                    WHERE a.id_empleado = e.id_empleado
                      AND a.estado = 'PENDIENTE'
                ) THEN 1 ELSE 0 END AS bit) AS tiene_anticipo_pendiente
            FROM dbo.EMPLEADO e
            INNER JOIN dbo.CONTRATO c ON c.id_empleado = e.id_empleado
            INNER JOIN dbo.CARGO ca ON ca.id_cargo = c.id_cargo
            OUTER APPLY dbo.fn_Estimar_Liquido(e.id_empleado, @fecha, 0, 0) est
            WHERE e.id_empleado = @id_empleado
              AND e.estado = 'ACTIVO'
              AND c.estado = 'VIGENTE'
              AND c.fecha_contratacion <= @fecha
              AND (c.fecha_culminacion IS NULL OR c.fecha_culminacion >= @fecha)
            ORDER BY c.fecha_contratacion DESC;";

        using var conexionSql = new SqlConnection(_conexion.Cadena);
        using var cmd = new SqlCommand(consulta, conexionSql);
        cmd.Parameters.Add("@id_empleado", SqlDbType.Int).Value = idEmpleado;
        cmd.Parameters.Add("@fecha", SqlDbType.Date).Value = fecha.ToDateTime(TimeOnly.MinValue);

        conexionSql.Open();
        using var dr = cmd.ExecuteReader();
        if (!dr.Read())
        {
            return null;
        }

        var info = new EmpleadoPrestamoInfo
        {
            IdEmpleado = Convert.ToInt32(dr["id_empleado"]),
            NombreEmpleado = Convert.ToString(dr["nombre_empleado"]) ?? string.Empty,
            NombreCargo = Convert.ToString(dr["nombre_cargo"]) ?? string.Empty,
            SalarioBruto = Convert.ToDecimal(dr["salario_bruto"]),
            SalarioQuincenal = Convert.ToDecimal(dr["salario_quincenal"]),
            FechaCulminacion = FechaNullable(dr["fecha_culminacion"]),
            CuotaPrestamoActiva = Convert.ToDecimal(dr["cuota_prestamo_activa"]),
            AnticipoPendiente = Convert.ToDecimal(dr["anticipo_pendiente"]),
            LiquidoEstimado = Convert.ToDecimal(dr["liquido_estimado"]),
            TienePrestamoActivo = Convert.ToBoolean(dr["tiene_prestamo_activo"]),
            TieneAnticipoPendiente = Convert.ToBoolean(dr["tiene_anticipo_pendiente"])
        };

        CalcularLimites(info, fecha);
        return info;
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

    private static void CalcularLimites(EmpleadoPrestamoInfo info, DateOnly fecha)
    {
        var salarioQuincenal = info.SalarioQuincenal;
        var limiteLiquido = info.LiquidoEstimado - salarioQuincenal * 0.50m;
        var limiteCombinado = salarioQuincenal * 0.35m - info.CuotaPrestamoActiva - info.AnticipoPendiente;

        info.MaximoAnticipo = info.TieneAnticipoPendiente
            ? 0
            : RedondearHaciaAbajo(Math.Max(0, Math.Min(
                salarioQuincenal * 0.30m,
                Math.Min(limiteCombinado, limiteLiquido))));

        if (info.TienePrestamoActivo)
        {
            info.MaximoPrestamo = 0;
            return;
        }

        var limiteCuota = Math.Min(
            salarioQuincenal * 0.20m,
            Math.Min(limiteCombinado, limiteLiquido));

        if (limiteCuota <= 0)
        {
            info.MaximoPrestamo = 0;
            return;
        }

        if (limiteCuota * 48 >= 600.01m && CumpleContrato(info.FechaCulminacion, fecha, 48))
        {
            info.MaximoPrestamo = Math.Min(1200m, RedondearHaciaAbajo(limiteCuota * 48));
        }
        else if (limiteCuota * 24 >= 300.01m && CumpleContrato(info.FechaCulminacion, fecha, 24))
        {
            info.MaximoPrestamo = Math.Min(600m, RedondearHaciaAbajo(limiteCuota * 24));
        }
        else if (limiteCuota * 12 >= 0.01m && CumpleContrato(info.FechaCulminacion, fecha, 12))
        {
            info.MaximoPrestamo = Math.Min(300m, RedondearHaciaAbajo(limiteCuota * 12));
        }
    }

    private static bool CumpleContrato(DateOnly? fechaCulminacion, DateOnly fecha, int cuotas)
    {
        return !fechaCulminacion.HasValue || fecha.AddDays(cuotas * 15) <= fechaCulminacion.Value;
    }

    private static decimal RedondearHaciaAbajo(decimal valor)
    {
        return Math.Floor(Math.Max(0, valor) * 100m) / 100m;
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
