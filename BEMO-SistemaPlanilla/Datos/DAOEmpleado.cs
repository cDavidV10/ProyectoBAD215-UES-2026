using BEMO.Entidades;
using Microsoft.Data.SqlClient;
using System.Data;

namespace BEMO.Datos;

public class DAOEmpleado
{
    private readonly Conexion _conexion;

    public DAOEmpleado(Conexion conexion)
    {
        _conexion = conexion;
    }

    public int UsuariosActivos()
    {
        return ContarEmpleadosPorEstado("ACTIVO");
    }

    public int EmpleadosInactivos()
    {
        return ContarEmpleadosPorEstado("INACTIVO");
    }

    public int EmpleadosTotales()
    {
        return ContarEmpleadosPorEstado("ACTIVO") + ContarEmpleadosPorEstado("INACTIVO");
    }

    public (List<Empleado> Empleados, int Total) ListarPaginado(int pagina, int tamanoPagina, string? busqueda, string? departamento, string? estado)
    {
        pagina = Math.Max(1, pagina);
        tamanoPagina = Math.Max(1, tamanoPagina);
        busqueda = NormalizarFiltro(busqueda);
        departamento = NormalizarFiltro(departamento);
        estado = NormalizarFiltro(estado);
        var desplazamiento = (pagina - 1) * tamanoPagina;
        var empleados = new List<Empleado>();
        var total = 0;

        const string consulta = @"
            WITH EmpleadosUnicos AS
            (
                SELECT *,
                       ROW_NUMBER() OVER
                       (
                           PARTITION BY id_empleado
                           ORDER BY id_contrato DESC
                       ) AS fila
                FROM dbo.VW_Empleados_General
                WHERE (@busqueda IS NULL
                       OR dui LIKE '%' + @busqueda + '%'
                       OR nombre LIKE '%' + @busqueda + '%'
                       OR apellido LIKE '%' + @busqueda + '%'
                       OR CONCAT(nombre, ' ', apellido) LIKE '%' + @busqueda + '%')
                  AND (@departamento IS NULL OR nombre_departamento = @departamento)
                  AND (@estado IS NULL OR estado = @estado)
            )
            SELECT *, COUNT(*) OVER() AS total_registros
            FROM EmpleadosUnicos
            WHERE fila = 1
            ORDER BY id_empleado
            OFFSET @desplazamiento ROWS
            FETCH NEXT @tamanoPagina ROWS ONLY;";

        using var conexionSql = new SqlConnection(_conexion.Cadena);
        using var cmd = new SqlCommand(consulta, conexionSql);
        cmd.Parameters.Add("@desplazamiento", SqlDbType.Int).Value = desplazamiento;
        cmd.Parameters.Add("@tamanoPagina", SqlDbType.Int).Value = tamanoPagina;
        AgregarFiltros(cmd, busqueda, departamento, estado);

        conexionSql.Open();
        using var dr = cmd.ExecuteReader();

        while (dr.Read())
        {
            if (total == 0)
            {
                total = Convert.ToInt32(dr["total_registros"]);
            }

            var empleado = new Empleado
            {
                IdEmpleado = Convert.ToInt32(dr["id_empleado"]),
                Dui = Texto(dr, "dui")?.Trim() ?? string.Empty,
                Nit = Texto(dr, "nit"),
                NumeroIsss = Texto(dr, "numero_isss") ?? string.Empty,
                Nombre = Texto(dr, "nombre") ?? string.Empty,
                Apellido = Texto(dr, "apellido") ?? string.Empty,
                Telefono = Texto(dr, "telefono") ?? string.Empty,
                Correo = Texto(dr, "correo"),
                FechaNac = DateOnly.FromDateTime(Convert.ToDateTime(dr["fecha_nac"])),
                Estado = Texto(dr, "estado") ?? string.Empty,
                Afp = new Afp
                {
                    NombreAfp = Texto(dr, "nombre_afp") ?? string.Empty
                },
                Direccion = new Direccion
                {
                    NombreCalle = Texto(dr, "nombre_calle") ?? string.Empty,
                    NumeroCasa = Texto(dr, "numero_casa") ?? string.Empty
                }
            };

            var contrato = CrearContrato(dr, empleado.IdEmpleado);
            if (contrato is not null)
            {
                empleado.Contratos.Add(contrato);
            }

            empleados.Add(empleado);
        }

        return (empleados, total);
    }

    public (int Activos, int Inactivos, int Total) ObtenerResumen()
    {
        const string consulta = @"
            SELECT
                COUNT(*) AS total,
                SUM(CASE WHEN estado = 'ACTIVO' THEN 1 ELSE 0 END) AS activos,
                SUM(CASE WHEN estado = 'INACTIVO' THEN 1 ELSE 0 END) AS inactivos
            FROM EMPLEADO;";

        using var conexionSql = new SqlConnection(_conexion.Cadena);
        using var cmd = new SqlCommand(consulta, conexionSql);

        conexionSql.Open();
        using var dr = cmd.ExecuteReader();
        if (!dr.Read())
        {
            return (0, 0, 0);
        }

        return (
            Convert.ToInt32(dr["activos"]),
            Convert.ToInt32(dr["inactivos"]),
            Convert.ToInt32(dr["total"]));
    }

    private static void AgregarFiltros(SqlCommand cmd, string? busqueda, string? departamento, string? estado)
    {
        cmd.Parameters.Add("@busqueda", SqlDbType.VarChar, 100).Value =
            (object?)NormalizarFiltro(busqueda) ?? DBNull.Value;
        cmd.Parameters.Add("@departamento", SqlDbType.VarChar, 50).Value =
            (object?)NormalizarFiltro(departamento) ?? DBNull.Value;
        cmd.Parameters.Add("@estado", SqlDbType.VarChar, 20).Value =
            (object?)NormalizarFiltro(estado) ?? DBNull.Value;
    }

    private static string? NormalizarFiltro(string? filtro)
    {
        return string.IsNullOrWhiteSpace(filtro) ? null : filtro.Trim();
    }

    private static Contrato? CrearContrato(SqlDataReader dr, int idEmpleado)
    {
        var idContrato = EnteroNullable(dr, "id_contrato");
        if (idContrato is null)
        {
            return null;
        }

        return new Contrato
        {
            IdContrato = idContrato.Value,
            IdEmpleado = idEmpleado,
            SalarioBruto = DecimalNullable(dr, "salario_bruto") ?? 0,
            FechaContratacion = DateOnly.FromDateTime(Convert.ToDateTime(dr["fecha_contratacion"])),
            FechaCulminacion = FechaNullable(dr, "fecha_culminacion"),
            Estado = Texto(dr, "estado_contrato") ?? string.Empty,
            Cargo = new Cargo
            {
                NombreCargo = Texto(dr, "nombre_cargo") ?? string.Empty
            },
            Departamento = new Departamento
            {
                NombreDepartamento = Texto(dr, "nombre_departamento") ?? string.Empty
            },
            TipoContrato = new TipoContrato
            {
                NombreTipoContrato = Texto(dr, "nombre_tipo_contrato") ?? string.Empty
            }
        };
    }

    private static string? Texto(SqlDataReader dr, string columna)
    {
        return dr[columna] == DBNull.Value ? null : Convert.ToString(dr[columna]);
    }

    private static int? EnteroNullable(SqlDataReader dr, string columna)
    {
        return dr[columna] == DBNull.Value ? null : Convert.ToInt32(dr[columna]);
    }

    private static decimal? DecimalNullable(SqlDataReader dr, string columna)
    {
        return dr[columna] == DBNull.Value ? null : Convert.ToDecimal(dr[columna]);
    }

    private static DateOnly? FechaNullable(SqlDataReader dr, string columna)
    {
        return dr[columna] == DBNull.Value
            ? null
            : DateOnly.FromDateTime(Convert.ToDateTime(dr[columna]));
    }

    private int ContarEmpleadosPorEstado(string estado)
    {
        const string consulta = @"
            SELECT COUNT(id_empleado)
            FROM EMPLEADO
            WHERE estado = @estado;";

        using var conexionSql = new SqlConnection(_conexion.Cadena);
        using var cmd = new SqlCommand(consulta, conexionSql);
        cmd.Parameters.Add("@estado", SqlDbType.VarChar, 20).Value = estado;

        conexionSql.Open();
        return Convert.ToInt32(cmd.ExecuteScalar());
    }
}
