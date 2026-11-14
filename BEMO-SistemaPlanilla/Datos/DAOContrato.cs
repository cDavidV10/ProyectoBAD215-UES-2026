using BEMO.Entidades;
using Microsoft.Data.SqlClient;
using System.Data;

namespace BEMO.Datos;

public class DAOContrato
{
    private readonly Conexion _conexion;

    public DAOContrato(Conexion conexion)
    {
        _conexion = conexion;
    }

    public List<ContratoListado> ListarVigentes()
    {
        const string consulta = @"
            SELECT
                c.id_contrato,
                c.id_empleado,
                c.id_cargo,
                c.id_departamento,
                c.id_tipo_contrato,
                CONCAT(e.nombre, ' ', e.apellido) AS nombre_empleado,
                ca.nombre_cargo,
                tc.nombre_tipo_contrato,
                d.nombre_departamento,
                c.salario_bruto,
                c.fecha_contratacion,
                c.fecha_culminacion,
                c.estado,
                CASE
                    WHEN c.fecha_culminacion IS NULL THEN NULL
                    ELSE DATEDIFF(DAY, CAST(GETDATE() AS DATE), c.fecha_culminacion)
                END AS dias_restantes
            FROM dbo.CONTRATO c
            INNER JOIN dbo.EMPLEADO e ON e.id_empleado = c.id_empleado
            INNER JOIN dbo.CARGO ca ON ca.id_cargo = c.id_cargo
            INNER JOIN dbo.TIPO_CONTRATO tc ON tc.id_tipo_contrato = c.id_tipo_contrato
            INNER JOIN dbo.DEPARTAMENTO d ON d.id_departamento = c.id_departamento
            WHERE c.estado = 'VIGENTE'
              AND (c.fecha_culminacion IS NULL OR c.fecha_culminacion >= CAST(GETDATE() AS DATE))
            ORDER BY
                CASE WHEN c.fecha_culminacion IS NULL THEN 1 ELSE 0 END,
                c.fecha_culminacion,
                e.apellido,
                e.nombre;";

        var contratos = new List<ContratoListado>();
        using var conexionSql = new SqlConnection(_conexion.Cadena);
        using var cmd = new SqlCommand(consulta, conexionSql);

        conexionSql.Open();
        using var dr = cmd.ExecuteReader();
        while (dr.Read())
        {
            contratos.Add(MapearContrato(dr));
        }

        return contratos;
    }

    public ContratoListado? ObtenerPorId(int idContrato)
    {
        const string consulta = @"
            SELECT TOP (1)
                c.id_contrato,
                c.id_empleado,
                c.id_cargo,
                c.id_departamento,
                c.id_tipo_contrato,
                CONCAT(e.nombre, ' ', e.apellido) AS nombre_empleado,
                ca.nombre_cargo,
                tc.nombre_tipo_contrato,
                d.nombre_departamento,
                c.salario_bruto,
                c.fecha_contratacion,
                c.fecha_culminacion,
                c.estado,
                CASE
                    WHEN c.fecha_culminacion IS NULL THEN NULL
                    ELSE DATEDIFF(DAY, CAST(GETDATE() AS DATE), c.fecha_culminacion)
                END AS dias_restantes
            FROM dbo.CONTRATO c
            INNER JOIN dbo.EMPLEADO e ON e.id_empleado = c.id_empleado
            INNER JOIN dbo.CARGO ca ON ca.id_cargo = c.id_cargo
            INNER JOIN dbo.TIPO_CONTRATO tc ON tc.id_tipo_contrato = c.id_tipo_contrato
            INNER JOIN dbo.DEPARTAMENTO d ON d.id_departamento = c.id_departamento
            WHERE c.id_contrato = @id_contrato;";

        using var conexionSql = new SqlConnection(_conexion.Cadena);
        using var cmd = new SqlCommand(consulta, conexionSql);
        cmd.Parameters.Add("@id_contrato", SqlDbType.Int).Value = idContrato;

        conexionSql.Open();
        using var dr = cmd.ExecuteReader();
        return dr.Read() ? MapearContrato(dr) : null;
    }

    public (List<ContratoListado> Contratos, int Total) ListarPaginado(
        int pagina,
        int tamanoPagina,
        string? busqueda,
        int? idEmpleado,
        string? tipoContrato,
        string? situacion)
    {
        pagina = Math.Max(1, pagina);
        tamanoPagina = Math.Max(1, tamanoPagina);
        busqueda = NormalizarFiltro(busqueda);
        tipoContrato = NormalizarFiltro(tipoContrato);
        situacion = NormalizarFiltro(situacion);
        var desplazamiento = (pagina - 1) * tamanoPagina;

        const string consulta = @"
            WITH ContratosVigentes AS
            (
                SELECT
                    c.id_contrato,
                    c.id_empleado,
                    c.id_cargo,
                    c.id_departamento,
                    c.id_tipo_contrato,
                    CONCAT(e.nombre, ' ', e.apellido) AS nombre_empleado,
                    ca.nombre_cargo,
                    tc.nombre_tipo_contrato,
                    d.nombre_departamento,
                    c.salario_bruto,
                    c.fecha_contratacion,
                    c.fecha_culminacion,
                    c.estado,
                    CASE
                        WHEN c.fecha_culminacion IS NULL THEN NULL
                        ELSE DATEDIFF(DAY, CAST(GETDATE() AS DATE), c.fecha_culminacion)
                    END AS dias_restantes
                FROM dbo.CONTRATO c
                INNER JOIN dbo.EMPLEADO e ON e.id_empleado = c.id_empleado
                INNER JOIN dbo.CARGO ca ON ca.id_cargo = c.id_cargo
                INNER JOIN dbo.TIPO_CONTRATO tc ON tc.id_tipo_contrato = c.id_tipo_contrato
                INNER JOIN dbo.DEPARTAMENTO d ON d.id_departamento = c.id_departamento
                WHERE c.estado = 'VIGENTE'
                  AND (c.fecha_culminacion IS NULL OR c.fecha_culminacion >= CAST(GETDATE() AS DATE))
                  AND (@busqueda IS NULL
                       OR CAST(e.id_empleado AS VARCHAR(20)) LIKE '%' + @busqueda + '%'
                       OR e.dui LIKE '%' + @busqueda + '%'
                       OR e.nombre LIKE '%' + @busqueda + '%'
                       OR e.apellido LIKE '%' + @busqueda + '%'
                       OR CONCAT(e.nombre, ' ', e.apellido) LIKE '%' + @busqueda + '%')
                  AND (@id_empleado IS NULL OR c.id_empleado = @id_empleado)
                  AND (@tipo_contrato IS NULL OR tc.nombre_tipo_contrato = @tipo_contrato)
                  AND (@situacion IS NULL
                       OR (@situacion = 'POR VENCER'
                           AND c.fecha_culminacion IS NOT NULL
                           AND DATEDIFF(DAY, CAST(GETDATE() AS DATE), c.fecha_culminacion) BETWEEN 0 AND 30)
                       OR (@situacion = 'VIGENTE'
                           AND (c.fecha_culminacion IS NULL
                                OR DATEDIFF(DAY, CAST(GETDATE() AS DATE), c.fecha_culminacion) > 30))
                  )
            )
            SELECT
                id_contrato,
                id_empleado,
                id_cargo,
                id_departamento,
                id_tipo_contrato,
                nombre_empleado,
                nombre_cargo,
                nombre_tipo_contrato,
                nombre_departamento,
                salario_bruto,
                fecha_contratacion,
                fecha_culminacion,
                estado,
                dias_restantes,
                COUNT(*) OVER() AS total_registros
            FROM ContratosVigentes
            ORDER BY
                CASE WHEN fecha_culminacion IS NULL THEN 1 ELSE 0 END,
                fecha_culminacion,
                nombre_empleado,
                id_contrato
            OFFSET @desplazamiento ROWS
            FETCH NEXT @tamanoPagina ROWS ONLY
            OPTION (RECOMPILE);";

        var contratos = new List<ContratoListado>();
        var total = 0;
        using var conexionSql = new SqlConnection(_conexion.Cadena);
        using var cmd = new SqlCommand(consulta, conexionSql);
        cmd.Parameters.Add("@desplazamiento", SqlDbType.Int).Value = desplazamiento;
        cmd.Parameters.Add("@tamanoPagina", SqlDbType.Int).Value = tamanoPagina;
        cmd.Parameters.Add("@busqueda", SqlDbType.VarChar, 100).Value =
            (object?)busqueda ?? DBNull.Value;
        cmd.Parameters.Add("@id_empleado", SqlDbType.Int).Value =
            (object?)idEmpleado ?? DBNull.Value;
        cmd.Parameters.Add("@tipo_contrato", SqlDbType.VarChar, 50).Value =
            (object?)tipoContrato ?? DBNull.Value;
        cmd.Parameters.Add("@situacion", SqlDbType.VarChar, 20).Value =
            (object?)situacion ?? DBNull.Value;

        conexionSql.Open();
        using var dr = cmd.ExecuteReader();
        while (dr.Read())
        {
            if (total == 0)
            {
                total = Convert.ToInt32(dr["total_registros"]);
            }

            contratos.Add(MapearContrato(dr));
        }

        return (contratos, total);
    }

    public int Registrar(
        int idEmpleado,
        int idCargo,
        int idDepartamento,
        int idTipoContrato,
        decimal salarioBruto,
        DateOnly fechaContratacion,
        DateOnly? fechaCulminacion)
    {
        const string consulta = @"
            SET NOCOUNT ON;
            SET XACT_ABORT ON;

            BEGIN TRY
                DECLARE @msg NVARCHAR(200);
                DECLARE @rango_min DECIMAL(10,2), @rango_max DECIMAL(10,2);
                DECLARE @nombre_tipo VARCHAR(50);
                DECLARE @ultima_fecha_culminacion DATE;

                IF NOT EXISTS
                (
                    SELECT 1
                    FROM dbo.EMPLEADO e
                    WHERE e.id_empleado = @id_empleado
                      AND (
                          e.estado = 'ACTIVO'
                          OR EXISTS
                          (
                              SELECT 1
                              FROM dbo.CONTRATO c
                              WHERE c.id_empleado = e.id_empleado
                                AND c.estado = 'FINALIZADO'
                                AND c.fecha_culminacion IS NOT NULL
                          )
                      )
                )
                    THROW 51020, N'El empleado no existe o no está activo', 1;

                IF EXISTS
                (
                    SELECT 1 FROM dbo.CONTRATO
                    WHERE id_empleado = @id_empleado AND estado = 'VIGENTE'
                )
                    THROW 51028, N'El empleado ya tiene un contrato vigente', 1;

                SELECT @ultima_fecha_culminacion = MAX(c.fecha_culminacion)
                FROM dbo.CONTRATO c
                WHERE c.id_empleado = @id_empleado
                  AND c.estado = 'FINALIZADO'
                  AND c.fecha_culminacion IS NOT NULL;

                IF @ultima_fecha_culminacion IS NOT NULL
                   AND @fecha_contratacion <> DATEADD(DAY, 1, @ultima_fecha_culminacion)
                    THROW 51034, N'El nuevo contrato debe iniciar un dia despues de la fecha de culminacion del contrato anterior', 1;

                IF NOT EXISTS (SELECT 1 FROM dbo.DEPARTAMENTO WHERE id_departamento = @id_departamento)
                    THROW 51006, N'El departamento indicado no existe', 1;

                IF NOT EXISTS (SELECT 1 FROM dbo.TIPO_CONTRATO WHERE id_tipo_contrato = @id_tipo_contrato)
                    THROW 51007, N'El tipo de contrato indicado no existe', 1;

                SELECT @nombre_tipo = nombre_tipo_contrato
                FROM dbo.TIPO_CONTRATO
                WHERE id_tipo_contrato = @id_tipo_contrato;

                SELECT @rango_min = rango_minimo_cargo, @rango_max = rango_maximo_cargo
                FROM dbo.CARGO
                WHERE id_cargo = @id_cargo;

                IF @rango_min IS NULL
                    THROW 51008, N'El cargo indicado no existe', 1;

                IF @salario_bruto NOT BETWEEN @rango_min AND @rango_max
                BEGIN
                    SET @msg = CONCAT(N'El salario debe estar entre ', @rango_min, N' y ', @rango_max, N' (rango del cargo)');
                    THROW 51009, @msg, 1;
                END;

                IF @nombre_tipo = 'PERMANENTE' AND @fecha_culminacion IS NOT NULL
                    THROW 51032, N'Un contrato permanente no lleva fecha de culminación', 1;

                IF @nombre_tipo = 'TEMPORAL' AND @fecha_culminacion IS NULL
                    THROW 51033, N'Un contrato temporal requiere fecha de culminación', 1;

                IF @fecha_culminacion IS NOT NULL AND @fecha_culminacion <= CAST(GETDATE() AS DATE)
                    THROW 51011, N'La fecha de culminación debe ser posterior a la fecha actual', 1;

                IF @fecha_culminacion IS NOT NULL AND @fecha_culminacion < @fecha_contratacion
                    THROW 51010, N'La fecha de culminación no puede ser anterior a la contratación', 1;

                BEGIN TRANSACTION;

                DECLARE @id_contrato INT = NEXT VALUE FOR dbo.Seq_Contrato;

                INSERT INTO dbo.CONTRATO
                (
                    id_contrato,
                    id_empleado,
                    id_cargo,
                    id_departamento,
                    id_tipo_contrato,
                    salario_bruto,
                    fecha_contratacion,
                    fecha_culminacion,
                    estado
                )
                VALUES
                (
                    @id_contrato,
                    @id_empleado,
                    @id_cargo,
                    @id_departamento,
                    @id_tipo_contrato,
                    @salario_bruto,
                    @fecha_contratacion,
                    @fecha_culminacion,
                    'VIGENTE'
                );

                UPDATE dbo.EMPLEADO
                SET estado = 'ACTIVO'
                WHERE id_empleado = @id_empleado
                  AND estado = 'INACTIVO';

                COMMIT TRANSACTION;
                SELECT @id_contrato;
            END TRY
            BEGIN CATCH
                IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
                THROW;
            END CATCH;";

        using var conexionSql = new SqlConnection(_conexion.Cadena);
        using var cmd = new SqlCommand(consulta, conexionSql);
        cmd.Parameters.Add("@id_empleado", SqlDbType.Int).Value = idEmpleado;
        cmd.Parameters.Add("@id_cargo", SqlDbType.Int).Value = idCargo;
        cmd.Parameters.Add("@id_departamento", SqlDbType.Int).Value = idDepartamento;
        cmd.Parameters.Add("@id_tipo_contrato", SqlDbType.Int).Value = idTipoContrato;
        var salario = cmd.Parameters.Add("@salario_bruto", SqlDbType.Decimal);
        salario.Precision = 10;
        salario.Scale = 2;
        salario.Value = salarioBruto;
        cmd.Parameters.Add("@fecha_contratacion", SqlDbType.Date).Value = fechaContratacion.ToDateTime(TimeOnly.MinValue);
        cmd.Parameters.Add("@fecha_culminacion", SqlDbType.Date).Value =
            (object?)fechaCulminacion?.ToDateTime(TimeOnly.MinValue) ?? DBNull.Value;

        conexionSql.Open();
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public void Actualizar(
        int idContrato,
        int idCargo,
        int idDepartamento,
        int idTipoContrato,
        decimal salarioBruto,
        DateOnly fechaContratacion,
        DateOnly? fechaCulminacion)
    {
        const string consulta = @"
            UPDATE dbo.CONTRATO
            SET id_cargo = @id_cargo,
                id_departamento = @id_departamento,
                id_tipo_contrato = @id_tipo_contrato,
                salario_bruto = @salario_bruto,
                fecha_contratacion = @fecha_contratacion,
                fecha_culminacion = @fecha_culminacion
            WHERE id_contrato = @id_contrato
              AND estado = 'VIGENTE';";

        using var conexionSql = new SqlConnection(_conexion.Cadena);
        using var cmd = new SqlCommand(consulta, conexionSql);
        cmd.Parameters.Add("@id_contrato", SqlDbType.Int).Value = idContrato;
        cmd.Parameters.Add("@id_cargo", SqlDbType.Int).Value = idCargo;
        cmd.Parameters.Add("@id_departamento", SqlDbType.Int).Value = idDepartamento;
        cmd.Parameters.Add("@id_tipo_contrato", SqlDbType.Int).Value = idTipoContrato;
        var salario = cmd.Parameters.Add("@salario_bruto", SqlDbType.Decimal);
        salario.Precision = 10;
        salario.Scale = 2;
        salario.Value = salarioBruto;
        cmd.Parameters.Add("@fecha_contratacion", SqlDbType.Date).Value = fechaContratacion.ToDateTime(TimeOnly.MinValue);
        cmd.Parameters.Add("@fecha_culminacion", SqlDbType.Date).Value =
            (object?)fechaCulminacion?.ToDateTime(TimeOnly.MinValue) ?? DBNull.Value;

        conexionSql.Open();
        if (cmd.ExecuteNonQuery() != 1)
        {
            throw new InvalidOperationException("El contrato no existe o ya no está vigente.");
        }
    }

    public void Finalizar(int idContrato)
    {
        const string consulta = @"
            SET XACT_ABORT ON;
            BEGIN TRY
                BEGIN TRANSACTION;

                DECLARE @id_empleado INT;

                SELECT @id_empleado = id_empleado
                FROM dbo.CONTRATO WITH (UPDLOCK, HOLDLOCK)
                WHERE id_contrato = @id_contrato
                  AND estado = 'VIGENTE';

                IF @id_empleado IS NULL
                    THROW 51030, N'El contrato no existe o ya fue finalizado', 1;

                UPDATE dbo.CONTRATO
                SET estado = 'FINALIZADO',
                    fecha_culminacion = CASE
                        WHEN fecha_contratacion > CAST(GETDATE() AS DATE) THEN fecha_contratacion
                        ELSE CAST(GETDATE() AS DATE)
                    END
                WHERE id_empleado = @id_empleado
                  AND estado = 'VIGENTE';

                UPDATE dbo.EMPLEADO
                SET estado = 'INACTIVO'
                WHERE id_empleado = @id_empleado;

                IF @@ROWCOUNT <> 1
                    THROW 51031, N'No se pudo inactivar al empleado asociado', 1;

                COMMIT TRANSACTION;
            END TRY
            BEGIN CATCH
                IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
                THROW;
            END CATCH;";

        using var conexionSql = new SqlConnection(_conexion.Cadena);
        using var cmd = new SqlCommand(consulta, conexionSql);
        cmd.Parameters.Add("@id_contrato", SqlDbType.Int).Value = idContrato;

        conexionSql.Open();
        cmd.ExecuteNonQuery();
    }

    private static ContratoListado MapearContrato(SqlDataReader dr)
    {
        var diasRestantes = dr["dias_restantes"] == DBNull.Value
            ? (int?)null
            : Convert.ToInt32(dr["dias_restantes"]);

        return new ContratoListado
        {
            IdContrato = Convert.ToInt32(dr["id_contrato"]),
            IdEmpleado = Convert.ToInt32(dr["id_empleado"]),
            IdCargo = Convert.ToInt32(dr["id_cargo"]),
            IdDepartamento = Convert.ToInt32(dr["id_departamento"]),
            IdTipoContrato = Convert.ToInt32(dr["id_tipo_contrato"]),
            NombreEmpleado = Convert.ToString(dr["nombre_empleado"]) ?? string.Empty,
            NombreCargo = Convert.ToString(dr["nombre_cargo"]) ?? string.Empty,
            NombreTipoContrato = Convert.ToString(dr["nombre_tipo_contrato"]) ?? string.Empty,
            NombreDepartamento = Convert.ToString(dr["nombre_departamento"]) ?? string.Empty,
            SalarioBruto = Convert.ToDecimal(dr["salario_bruto"]),
            FechaContratacion = DateOnly.FromDateTime(Convert.ToDateTime(dr["fecha_contratacion"])),
            FechaCulminacion = dr["fecha_culminacion"] == DBNull.Value
                ? null
                : DateOnly.FromDateTime(Convert.ToDateTime(dr["fecha_culminacion"])),
            Estado = Convert.ToString(dr["estado"]) ?? string.Empty,
            DiasRestantes = diasRestantes,
            Situacion = diasRestantes is >= 0 and <= 30 ? "POR VENCER" : "VIGENTE"
        };
    }

    private static string? NormalizarFiltro(string? valor)
    {
        return string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
    }

}
