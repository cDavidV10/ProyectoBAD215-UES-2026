using BEMO.Entidades;
using Microsoft.Data.SqlClient;
using System.Data;

namespace BEMO.Datos;

public class DAOConstanciaSalarial
{
    private readonly Conexion _conexion;

    public DAOConstanciaSalarial(Conexion conexion)
    {
        _conexion = conexion;
    }

    public (List<ConstanciaEmpleadoOpcion> Empleados, int Total) ListarEmpleadosDisponibles(
        int pagina,
        int tamanoPagina,
        string? busqueda = null)
    {
        pagina = Math.Max(1, pagina);
        tamanoPagina = Math.Max(1, tamanoPagina);

        const string consulta = @"
            WITH ContratosVigentes AS
            (
                SELECT
                    c.id_empleado,
                    c.salario_bruto,
                    c.fecha_contratacion,
                    c.id_cargo,
                    c.id_departamento,
                    ROW_NUMBER() OVER
                    (
                        PARTITION BY c.id_empleado
                        ORDER BY c.fecha_contratacion DESC, c.id_contrato DESC
                    ) AS fila
                FROM dbo.CONTRATO c
                WHERE c.estado = 'VIGENTE'
            )
            SELECT
                e.id_empleado,
                CONCAT(e.nombre, ' ', e.apellido) AS nombre_completo,
                e.dui,
                cargo.nombre_cargo,
                departamento.nombre_departamento,
                cv.salario_bruto,
                cv.fecha_contratacion,
                COUNT(*) OVER() AS total_registros
            FROM dbo.EMPLEADO e
            INNER JOIN ContratosVigentes cv
                ON cv.id_empleado = e.id_empleado
               AND cv.fila = 1
            INNER JOIN dbo.CARGO cargo
                ON cargo.id_cargo = cv.id_cargo
            INNER JOIN dbo.DEPARTAMENTO departamento
                ON departamento.id_departamento = cv.id_departamento
            WHERE e.estado = 'ACTIVO'
              AND (@busqueda IS NULL
                   OR e.dui LIKE '%' + @busqueda + '%'
                   OR e.nombre LIKE '%' + @busqueda + '%'
                   OR e.apellido LIKE '%' + @busqueda + '%'
                   OR CONCAT(e.nombre, ' ', e.apellido) LIKE '%' + @busqueda + '%')
            ORDER BY e.nombre, e.apellido, e.id_empleado;";

        var empleados = new List<ConstanciaEmpleadoOpcion>();
        using var conexionSql = new SqlConnection(_conexion.Cadena);
        using var cmd = new SqlCommand(consulta, conexionSql);
        cmd.Parameters.Add("@busqueda", SqlDbType.VarChar, 100).Value =
            string.IsNullOrWhiteSpace(busqueda) ? DBNull.Value : busqueda.Trim();
        cmd.Parameters.Add("@desplazamiento", SqlDbType.Int).Value =
            (pagina - 1) * tamanoPagina;
        cmd.Parameters.Add("@tamanoPagina", SqlDbType.Int).Value = tamanoPagina;

        var consultaPaginada = consulta.Replace(
            "ORDER BY e.nombre, e.apellido, e.id_empleado;",
            "ORDER BY e.nombre, e.apellido, e.id_empleado\n            OFFSET @desplazamiento ROWS\n            FETCH NEXT @tamanoPagina ROWS ONLY;");
        cmd.CommandText = consultaPaginada;

        conexionSql.Open();
        using var dr = cmd.ExecuteReader();
        var total = 0;
        while (dr.Read())
        {
            if (total == 0)
            {
                total = Convert.ToInt32(dr["total_registros"]);
            }

            empleados.Add(new ConstanciaEmpleadoOpcion
            {
                IdEmpleado = Convert.ToInt32(dr["id_empleado"]),
                NombreCompleto = Texto(dr["nombre_completo"]),
                Dui = Texto(dr["dui"]).Trim(),
                NombreCargo = Texto(dr["nombre_cargo"]),
                NombreDepartamento = Texto(dr["nombre_departamento"]),
                SalarioBruto = Convert.ToDecimal(dr["salario_bruto"]),
                FechaContratacion = Fecha(dr["fecha_contratacion"])
            });
        }

        return (empleados, total);
    }

    public ConstanciaSalarial? Generar(int idEmpleado, int meses, DateOnly fechaEmision)
    {
        using var conexionSql = new SqlConnection(_conexion.Cadena);
        using var cmd = new SqlCommand("dbo.usp_Constancia_Salario", conexionSql)
        {
            CommandType = CommandType.StoredProcedure
        };
        cmd.Parameters.Add("@id_empleado", SqlDbType.Int).Value = idEmpleado;
        cmd.Parameters.Add("@meses", SqlDbType.Int).Value = meses;
        cmd.Parameters.Add("@fecha_emision", SqlDbType.Date).Value =
            fechaEmision.ToDateTime(TimeOnly.MinValue);

        conexionSql.Open();
        using var dr = cmd.ExecuteReader();
        if (!dr.Read())
        {
            return null;
        }

        var encabezado = new ConstanciaSalarialEncabezado
        {
            IdEmpleado = Convert.ToInt32(dr["id_empleado"]),
            NombreCompleto = $"{Texto(dr["nombre"])} {Texto(dr["apellido"])}".Trim(),
            Dui = Texto(dr["dui"]).Trim(),
            NumeroIsss = Texto(dr["numero_isss"]).Trim(),
            NombreTipoContrato = Texto(dr["nombre_tipo_contrato"]),
            NombreCargo = Texto(dr["nombre_cargo"]),
            NombreDepartamento = Texto(dr["nombre_departamento"]),
            SalarioBaseActual = Convert.ToDecimal(dr["salario_base_actual"]),
            FechaIngreso = Fecha(dr["fecha_ingreso"]),
            PeriodoDesde = Fecha(dr["periodo_desde"]),
            PeriodoHasta = Fecha(dr["periodo_hasta"])
        };

        var periodos = new List<ConstanciaSalarialPeriodo>();
        if (dr.NextResult())
        {
            while (dr.Read())
            {
                periodos.Add(new ConstanciaSalarialPeriodo
                {
                    NumeroPlanilla = Convert.ToInt32(dr["numero_planilla"]),
                    PeriodoInicio = Fecha(dr["periodo_inicio"]),
                    PeriodoFin = Fecha(dr["periodo_fin"]),
                    FechaPago = Fecha(dr["fecha_pago"]),
                    TotalBruto = Convert.ToDecimal(dr["total_bruto"]),
                    TotalDeducciones = Convert.ToDecimal(dr["total_deducciones"]),
                    SalarioLiquido = Convert.ToDecimal(dr["salario_liquido"])
                });
            }
        }

        var resumen = new ConstanciaSalarialResumen();
        if (dr.NextResult() && dr.Read())
        {
            resumen = new ConstanciaSalarialResumen
            {
                PlanillasIncluidas = Convert.ToInt32(dr["planillas_incluidas"]),
                TotalBruto = Convert.ToDecimal(dr["total_bruto"]),
                TotalDeducciones = Convert.ToDecimal(dr["total_deducciones"]),
                TotalLiquido = Convert.ToDecimal(dr["total_liquido"]),
                PromedioMensualBruto = Convert.ToDecimal(dr["promedio_mensual_bruto"]),
                PromedioMensualLiquido = Convert.ToDecimal(dr["promedio_mensual_liquido"])
            };
        }

        return new ConstanciaSalarial
        {
            Encabezado = encabezado,
            Periodos = periodos,
            Resumen = resumen
        };
    }

    private static string Texto(object valor) =>
        valor == DBNull.Value ? string.Empty : Convert.ToString(valor) ?? string.Empty;

    private static DateOnly Fecha(object valor) =>
        DateOnly.FromDateTime(Convert.ToDateTime(valor));
}
