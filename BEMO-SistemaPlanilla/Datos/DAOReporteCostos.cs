using BEMO.Entidades;
using Microsoft.Data.SqlClient;
using System.Data;

namespace BEMO.Datos;

public class DAOReporteCostos
{
    private readonly Conexion _conexion;

    public DAOReporteCostos(Conexion conexion)
    {
        _conexion = conexion;
    }

    public List<CostoDepartamentoComparativo> Obtener(
        DateOnly? fechaDesde,
        DateOnly? fechaHasta)
    {
        var resultados = new List<CostoDepartamentoComparativo>();
        using var conexionSql = new SqlConnection(_conexion.Cadena);
        using var cmd = new SqlCommand("dbo.usp_Reporte_Costo_Departamento", conexionSql)
        {
            CommandType = CommandType.StoredProcedure
        };

        cmd.Parameters.Add("@fecha_desde", SqlDbType.Date).Value =
            (object?)fechaDesde?.ToDateTime(TimeOnly.MinValue) ?? DBNull.Value;
        cmd.Parameters.Add("@fecha_hasta", SqlDbType.Date).Value =
            (object?)fechaHasta?.ToDateTime(TimeOnly.MinValue) ?? DBNull.Value;

        conexionSql.Open();
        using var dr = cmd.ExecuteReader();
        while (dr.Read())
        {
            resultados.Add(new CostoDepartamentoComparativo
            {
                NumeroPlanilla = Convert.ToInt32(dr["numero_planilla"]),
                PeriodoInicio = Fecha(dr["periodo_inicio"]),
                PeriodoFin = Fecha(dr["periodo_fin"]),
                NombreDepartamento = Convert.ToString(dr["nombre_departamento"]) ?? string.Empty,
                Empleados = Convert.ToInt32(dr["empleados"]),
                CostoTotal = Convert.ToDecimal(dr["costo_total"]),
                CostoPeriodoAnterior = DecimalNullable(dr["costo_periodo_anterior"]),
                Diferencia = DecimalNullable(dr["diferencia"]),
                VariacionPorcentual = DecimalNullable(dr["variacion_porcentual"])
            });
        }

        return resultados;
    }

    private static DateOnly Fecha(object valor) =>
        DateOnly.FromDateTime(Convert.ToDateTime(valor));

    private static decimal? DecimalNullable(object valor) =>
        valor == DBNull.Value ? null : Convert.ToDecimal(valor);
}
