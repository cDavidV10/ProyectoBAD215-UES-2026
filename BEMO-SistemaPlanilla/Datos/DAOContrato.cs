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
              AND c.fecha_contratacion <= CAST(GETDATE() AS DATE)
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
                  AND c.fecha_contratacion <= CAST(GETDATE() AS DATE)
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

    private static ContratoListado MapearContrato(SqlDataReader dr)
    {
        var diasRestantes = dr["dias_restantes"] == DBNull.Value
            ? (int?)null
            : Convert.ToInt32(dr["dias_restantes"]);

        return new ContratoListado
        {
            IdContrato = Convert.ToInt32(dr["id_contrato"]),
            IdEmpleado = Convert.ToInt32(dr["id_empleado"]),
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
