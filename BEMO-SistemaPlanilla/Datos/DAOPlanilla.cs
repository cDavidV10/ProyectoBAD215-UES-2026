using BEMO.Entidades;
using Microsoft.Data.SqlClient;
using System.Data;

namespace BEMO.Datos;

public class DAOPlanilla
{
    private readonly Conexion _conexion;

    public DAOPlanilla(Conexion conexion)
    {
        _conexion = conexion;
    }

    public (List<PlanillaListado> Planillas, int Total) ListarPaginado(
        int pagina,
        int tamanoPagina,
        string? estado)
    {
        pagina = Math.Max(1, pagina);
        tamanoPagina = Math.Max(1, tamanoPagina);
        estado = string.IsNullOrWhiteSpace(estado) ? null : estado.Trim().ToUpperInvariant();

        const string consulta = @"
            SELECT
                id_planilla,
                numero_planilla,
                periodo_inicio,
                periodo_fin,
                fecha_pago,
                fecha_generacion,
                total_neto,
                estado,
                cantidad_empleados,
                ISNULL(total_bruto, 0) AS total_bruto,
                ISNULL(total_deducciones, 0) AS total_deducciones,
                ISNULL(total_liquido, 0) AS total_liquido,
                COUNT(*) OVER() AS total_registros
            FROM dbo.VW_Planillas_Generadas
            WHERE (@estado IS NULL OR estado = @estado)
            ORDER BY periodo_inicio DESC, id_planilla DESC
            OFFSET @desplazamiento ROWS
            FETCH NEXT @tamanoPagina ROWS ONLY;";

        var planillas = new List<PlanillaListado>();
        var total = 0;
        using var conexionSql = new SqlConnection(_conexion.Cadena);
        using var cmd = new SqlCommand(consulta, conexionSql);
        cmd.Parameters.Add("@estado", SqlDbType.VarChar, 20).Value =
            (object?)estado ?? DBNull.Value;
        cmd.Parameters.Add("@desplazamiento", SqlDbType.Int).Value =
            (pagina - 1) * tamanoPagina;
        cmd.Parameters.Add("@tamanoPagina", SqlDbType.Int).Value = tamanoPagina;

        conexionSql.Open();
        using var dr = cmd.ExecuteReader();
        while (dr.Read())
        {
            if (total == 0)
            {
                total = Convert.ToInt32(dr["total_registros"]);
            }

            planillas.Add(MapearPlanilla(dr));
        }

        return (planillas, total);
    }

    public PlanillaListado? Obtener(int idPlanilla)
    {
        const string consulta = @"
            SELECT TOP 1
                id_planilla,
                numero_planilla,
                periodo_inicio,
                periodo_fin,
                fecha_pago,
                fecha_generacion,
                total_neto,
                estado,
                cantidad_empleados,
                ISNULL(total_bruto, 0) AS total_bruto,
                ISNULL(total_deducciones, 0) AS total_deducciones,
                ISNULL(total_liquido, 0) AS total_liquido
            FROM dbo.VW_Planillas_Generadas
            WHERE id_planilla = @id_planilla;";

        using var conexionSql = new SqlConnection(_conexion.Cadena);
        using var cmd = new SqlCommand(consulta, conexionSql);
        cmd.Parameters.Add("@id_planilla", SqlDbType.Int).Value = idPlanilla;

        conexionSql.Open();
        using var dr = cmd.ExecuteReader();
        return dr.Read() ? MapearPlanilla(dr) : null;
    }

    public List<RegistroAsistenciaPlanilla> ListarAsistencia(int idPlanilla)
    {
        return ListarAsistenciaPaginado(idPlanilla, 1, int.MaxValue).Asistencias;
    }

    public (List<RegistroAsistenciaPlanilla> Asistencias, int Total) ListarAsistenciaPaginado(
        int idPlanilla,
        int pagina,
        int tamanoPagina)
    {
        pagina = Math.Max(1, pagina);
        tamanoPagina = Math.Max(1, tamanoPagina);

        const string consulta = @"
            SELECT
                id_detalle_planilla,
                id_planilla,
                numero_planilla,
                periodo_inicio,
                periodo_fin,
                id_empleado,
                CONCAT(nombre, ' ', apellido) AS nombre_empleado,
                nombre_departamento,
                dias_trabajados,
                dias_no_trabajados,
                horas_extras,
                bonificaciones,
                COUNT(*) OVER() AS total_registros
            FROM dbo.VW_Registro_Asistencia_Empleados
            WHERE id_planilla = @id_planilla
            ORDER BY nombre, apellido, id_empleado
            OFFSET @desplazamiento ROWS
            FETCH NEXT @tamanoPagina ROWS ONLY;";

        var registros = new List<RegistroAsistenciaPlanilla>();
        var total = 0;
        using var conexionSql = new SqlConnection(_conexion.Cadena);
        using var cmd = new SqlCommand(consulta, conexionSql);
        cmd.Parameters.Add("@id_planilla", SqlDbType.Int).Value = idPlanilla;
        cmd.Parameters.Add("@desplazamiento", SqlDbType.Int).Value =
            (pagina - 1) * tamanoPagina;
        cmd.Parameters.Add("@tamanoPagina", SqlDbType.Int).Value = tamanoPagina;

        conexionSql.Open();
        using var dr = cmd.ExecuteReader();
        while (dr.Read())
        {
            if (total == 0)
            {
                total = Convert.ToInt32(dr["total_registros"]);
            }

            registros.Add(new RegistroAsistenciaPlanilla
            {
                IdDetallePlanilla = Convert.ToInt32(dr["id_detalle_planilla"]),
                IdPlanilla = Convert.ToInt32(dr["id_planilla"]),
                NumeroPlanilla = Convert.ToInt32(dr["numero_planilla"]),
                PeriodoInicio = Fecha(dr["periodo_inicio"]),
                PeriodoFin = Fecha(dr["periodo_fin"]),
                IdEmpleado = Convert.ToInt32(dr["id_empleado"]),
                NombreEmpleado = Texto(dr["nombre_empleado"]),
                NombreDepartamento = Texto(dr["nombre_departamento"]),
                DiasTrabajados = Convert.ToInt32(dr["dias_trabajados"]),
                DiasNoTrabajados = Convert.ToInt32(dr["dias_no_trabajados"]),
                HorasExtras = Convert.ToInt32(dr["horas_extras"]),
                Bonificaciones = Convert.ToDecimal(dr["bonificaciones"])
            });
        }

        return (registros, total);
    }

    public (List<DetallePlanillaListado> Detalles, int Total) ListarDetalle(
        int idPlanilla,
        int pagina,
        int tamanoPagina)
    {
        pagina = Math.Max(1, pagina);
        tamanoPagina = Math.Max(1, tamanoPagina);

        const string consulta = @"
            SELECT
                id_planilla,
                numero_planilla,
                periodo_inicio,
                periodo_fin,
                fecha_pago,
                estado_planilla,
                id_detalle_planilla,
                id_empleado,
                CONCAT(nombre, ' ', apellido) AS nombre_empleado,
                dui,
                nombre_departamento,
                nombre_cargo,
                dias_trabajados,
                dias_no_trabajados,
                horas_extras,
                salario_mensual,
                salario_quincenal,
                bonificaciones,
                aguinaldo,
                total_bruto,
                monto_isss,
                monto_afp,
                monto_renta,
                descuentos_prestamos,
                descuentos_anticipos,
                total_deducciones,
                salario_liquido,
                estado_detalle,
                COUNT(*) OVER() AS total_registros
            FROM dbo.VW_Detalle_Planilla
            WHERE id_planilla = @id_planilla
            ORDER BY nombre, apellido, id_empleado
            OFFSET @desplazamiento ROWS
            FETCH NEXT @tamanoPagina ROWS ONLY;";

        var detalles = new List<DetallePlanillaListado>();
        var total = 0;
        using var conexionSql = new SqlConnection(_conexion.Cadena);
        using var cmd = new SqlCommand(consulta, conexionSql);
        cmd.Parameters.Add("@id_planilla", SqlDbType.Int).Value = idPlanilla;
        cmd.Parameters.Add("@desplazamiento", SqlDbType.Int).Value =
            (pagina - 1) * tamanoPagina;
        cmd.Parameters.Add("@tamanoPagina", SqlDbType.Int).Value = tamanoPagina;

        conexionSql.Open();
        using var dr = cmd.ExecuteReader();
        while (dr.Read())
        {
            if (total == 0)
            {
                total = Convert.ToInt32(dr["total_registros"]);
            }

            detalles.Add(new DetallePlanillaListado
            {
                IdPlanilla = Convert.ToInt32(dr["id_planilla"]),
                NumeroPlanilla = Convert.ToInt32(dr["numero_planilla"]),
                PeriodoInicio = Fecha(dr["periodo_inicio"]),
                PeriodoFin = Fecha(dr["periodo_fin"]),
                FechaPago = Fecha(dr["fecha_pago"]),
                EstadoPlanilla = Texto(dr["estado_planilla"]),
                IdDetallePlanilla = Convert.ToInt32(dr["id_detalle_planilla"]),
                IdEmpleado = Convert.ToInt32(dr["id_empleado"]),
                NombreEmpleado = Texto(dr["nombre_empleado"]),
                Dui = Texto(dr["dui"]),
                NombreDepartamento = Texto(dr["nombre_departamento"]),
                NombreCargo = Texto(dr["nombre_cargo"]),
                DiasTrabajados = Convert.ToInt32(dr["dias_trabajados"]),
                DiasNoTrabajados = Convert.ToInt32(dr["dias_no_trabajados"]),
                HorasExtras = Convert.ToInt32(dr["horas_extras"]),
                SalarioMensual = Convert.ToDecimal(dr["salario_mensual"]),
                SalarioQuincenal = Convert.ToDecimal(dr["salario_quincenal"]),
                Bonificaciones = Convert.ToDecimal(dr["bonificaciones"]),
                Aguinaldo = Convert.ToDecimal(dr["aguinaldo"]),
                TotalBruto = Convert.ToDecimal(dr["total_bruto"]),
                MontoIsss = Convert.ToDecimal(dr["monto_isss"]),
                MontoAfp = Convert.ToDecimal(dr["monto_afp"]),
                MontoRenta = Convert.ToDecimal(dr["monto_renta"]),
                DescuentosPrestamos = Convert.ToDecimal(dr["descuentos_prestamos"]),
                DescuentosAnticipos = Convert.ToDecimal(dr["descuentos_anticipos"]),
                TotalDeducciones = Convert.ToDecimal(dr["total_deducciones"]),
                SalarioLiquido = Convert.ToDecimal(dr["salario_liquido"]),
                EstadoDetalle = Texto(dr["estado_detalle"])
            });
        }

        return (detalles, total);
    }

    public int Crear(DateOnly periodoInicio, DateOnly periodoFin, DateOnly fechaPago, int? idUsuario = null)
    {
        using var conexionSql = new SqlConnection(_conexion.Cadena);
        using var cmd = new SqlCommand("dbo.usp_Crear_Planilla", conexionSql)
        {
            CommandType = CommandType.StoredProcedure
        };

        cmd.Parameters.Add("@periodo_inicio", SqlDbType.Date).Value = periodoInicio.ToDateTime(TimeOnly.MinValue);
        cmd.Parameters.Add("@periodo_fin", SqlDbType.Date).Value = periodoFin.ToDateTime(TimeOnly.MinValue);
        cmd.Parameters.Add("@fecha_pago", SqlDbType.Date).Value = fechaPago.ToDateTime(TimeOnly.MinValue);
        var idPlanilla = cmd.Parameters.Add("@id_planilla", SqlDbType.Int);
        idPlanilla.Direction = ParameterDirection.Output;

        conexionSql.Open();
        EstablecerUsuario(conexionSql, idUsuario);
        cmd.ExecuteNonQuery();
        return Convert.ToInt32(idPlanilla.Value);
    }

    public void CapturarAsistencia(
        int idDetallePlanilla,
        int diasTrabajados,
        int diasNoTrabajados,
        int horasExtras,
        decimal bonificaciones,
        int? idUsuario = null)
    {
        using var conexionSql = new SqlConnection(_conexion.Cadena);
        using var cmd = new SqlCommand("dbo.usp_Capturar_Asistencia", conexionSql)
        {
            CommandType = CommandType.StoredProcedure
        };

        cmd.Parameters.Add("@id_detalle_planilla", SqlDbType.Int).Value = idDetallePlanilla;
        cmd.Parameters.Add("@dias_trabajados", SqlDbType.Int).Value = diasTrabajados;
        cmd.Parameters.Add("@dias_no_trabajados", SqlDbType.Int).Value = diasNoTrabajados;
        cmd.Parameters.Add("@horas_extras", SqlDbType.Int).Value = horasExtras;
        cmd.Parameters.Add("@bonificaciones", SqlDbType.Decimal).Value = bonificaciones;

        conexionSql.Open();
        EstablecerUsuario(conexionSql, idUsuario);
        cmd.ExecuteNonQuery();
    }

    public void Calcular(int idPlanilla, bool incluirAguinaldo, int? idUsuario = null)
    {
        Ejecutar("dbo.usp_Calcular_Planilla", cmd =>
        {
            cmd.Parameters.Add("@id_planilla", SqlDbType.Int).Value = idPlanilla;
            cmd.Parameters.Add("@incluir_aguinaldo", SqlDbType.Bit).Value = incluirAguinaldo;
        }, idUsuario);
    }

    public void Cerrar(int idPlanilla, int? idUsuario = null)
    {
        Ejecutar("dbo.usp_Cerrar_Planilla", cmd =>
        {
            cmd.Parameters.Add("@id_planilla", SqlDbType.Int).Value = idPlanilla;
        }, idUsuario);
    }

    public void Reabrir(int idPlanilla, string motivo, int? idUsuario = null)
    {
        Ejecutar("dbo.usp_Reabrir_Planilla", cmd =>
        {
            cmd.Parameters.Add("@id_planilla", SqlDbType.Int).Value = idPlanilla;
            cmd.Parameters.Add("@motivo", SqlDbType.NVarChar, 100).Value = motivo.Trim();
        }, idUsuario);
    }

    private void Ejecutar(string procedimiento, Action<SqlCommand> agregarParametros, int? idUsuario)
    {
        using var conexionSql = new SqlConnection(_conexion.Cadena);
        using var cmd = new SqlCommand(procedimiento, conexionSql)
        {
            CommandType = CommandType.StoredProcedure
        };
        agregarParametros(cmd);
        conexionSql.Open();
        EstablecerUsuario(conexionSql, idUsuario);
        cmd.ExecuteNonQuery();
    }

    private static void EstablecerUsuario(SqlConnection conexionSql, int? idUsuario)
    {
        if (!idUsuario.HasValue)
        {
            return;
        }

        using var cmd = new SqlCommand(
            "EXEC sys.sp_set_session_context @key = N'id_usuario', @value = @id_usuario;",
            conexionSql);
        cmd.Parameters.Add("@id_usuario", SqlDbType.Int).Value = idUsuario.Value;
        cmd.ExecuteNonQuery();
    }

    private static PlanillaListado MapearPlanilla(SqlDataReader dr)
    {
        return new PlanillaListado
        {
            IdPlanilla = Convert.ToInt32(dr["id_planilla"]),
            NumeroPlanilla = Convert.ToInt32(dr["numero_planilla"]),
            PeriodoInicio = Fecha(dr["periodo_inicio"]),
            PeriodoFin = Fecha(dr["periodo_fin"]),
            FechaPago = Fecha(dr["fecha_pago"]),
            FechaGeneracion = Fecha(dr["fecha_generacion"]),
            TotalNeto = Convert.ToDecimal(dr["total_neto"]),
            Estado = Texto(dr["estado"]),
            CantidadEmpleados = Convert.ToInt32(dr["cantidad_empleados"]),
            TotalBruto = Convert.ToDecimal(dr["total_bruto"]),
            TotalDeducciones = Convert.ToDecimal(dr["total_deducciones"]),
            TotalLiquido = Convert.ToDecimal(dr["total_liquido"])
        };
    }

    private static DateOnly Fecha(object valor)
    {
        return DateOnly.FromDateTime(Convert.ToDateTime(valor));
    }

    private static string Texto(object valor)
    {
        return valor == DBNull.Value ? string.Empty : Convert.ToString(valor) ?? string.Empty;
    }
}
