using BEMO.Entidades;
using Microsoft.Data.SqlClient;
using System.Data;

namespace BEMO.Datos;

public class DAOParametrosLegales
{
    private readonly Conexion _conexion;

    public DAOParametrosLegales(Conexion conexion)
    {
        _conexion = conexion;
    }

    public List<TipoDescuento> ListarTiposDescuento()
    {
        const string consulta = @"
            SELECT id_tipo_descuento, nombre_tipo_descuento
            FROM dbo.TIPO_DESCUENTO
            ORDER BY nombre_tipo_descuento;";

        var tipos = new List<TipoDescuento>();
        using var conexionSql = new SqlConnection(_conexion.Cadena);
        using var cmd = new SqlCommand(consulta, conexionSql);
        conexionSql.Open();
        using var dr = cmd.ExecuteReader();

        while (dr.Read())
        {
            tipos.Add(new TipoDescuento
            {
                IdTipoDescuento = Convert.ToInt32(dr["id_tipo_descuento"]),
                NombreTipoDescuento = Convert.ToString(dr["nombre_tipo_descuento"])?.Trim() ?? string.Empty
            });
        }

        return tipos;
    }

    public List<TipoRenta> ListarTiposRenta()
    {
        const string consulta = @"
            SELECT id_tipo_renta, nombre_tramo
            FROM dbo.TIPO_RENTA
            ORDER BY id_tipo_renta;";

        var tipos = new List<TipoRenta>();
        using var conexionSql = new SqlConnection(_conexion.Cadena);
        using var cmd = new SqlCommand(consulta, conexionSql);
        conexionSql.Open();
        using var dr = cmd.ExecuteReader();

        while (dr.Read())
        {
            tipos.Add(new TipoRenta
            {
                IdTipoRenta = Convert.ToInt32(dr["id_tipo_renta"]),
                NombreTramo = Convert.ToString(dr["nombre_tramo"])?.Trim() ?? string.Empty
            });
        }

        return tipos;
    }

    public List<DescuentoLey> ListarDescuentosLey()
    {
        const string consulta = @"
            WITH DescuentosVigentes AS (
                SELECT d.id_descuento, d.id_tipo_descuento, t.nombre_tipo_descuento,
                       d.porcentaje_descuento, d.tope_mensual, d.fecha_vigencia,
                       ROW_NUMBER() OVER (
                           PARTITION BY d.id_tipo_descuento
                           ORDER BY d.fecha_vigencia DESC, d.id_descuento DESC
                       ) AS fila
                FROM dbo.DESCUENTO_LEY d
                INNER JOIN dbo.TIPO_DESCUENTO t
                    ON t.id_tipo_descuento = d.id_tipo_descuento
                WHERE d.fecha_vigencia <= CAST(GETDATE() AS DATE)
            )
            SELECT id_descuento, id_tipo_descuento, nombre_tipo_descuento,
                   porcentaje_descuento, tope_mensual, fecha_vigencia
            FROM DescuentosVigentes
            WHERE fila = 1
            ORDER BY fecha_vigencia DESC, nombre_tipo_descuento;";

        var descuentos = new List<DescuentoLey>();
        using var conexionSql = new SqlConnection(_conexion.Cadena);
        using var cmd = new SqlCommand(consulta, conexionSql);
        conexionSql.Open();
        using var dr = cmd.ExecuteReader();

        while (dr.Read())
        {
            var idTipo = Convert.ToInt32(dr["id_tipo_descuento"]);
            descuentos.Add(new DescuentoLey
            {
                IdDescuento = Convert.ToInt32(dr["id_descuento"]),
                IdTipoDescuento = idTipo,
                PorcentajeDescuento = Convert.ToDecimal(dr["porcentaje_descuento"]),
                TopeMensual = dr["tope_mensual"] == DBNull.Value ? null : Convert.ToDecimal(dr["tope_mensual"]),
                FechaVigencia = Fecha(dr["fecha_vigencia"]),
                TipoDescuento = new TipoDescuento
                {
                    IdTipoDescuento = idTipo,
                    NombreTipoDescuento = Convert.ToString(dr["nombre_tipo_descuento"])?.Trim() ?? string.Empty
                }
            });
        }

        return descuentos;
    }

    public DescuentoLey? ObtenerDescuentoLey(int id) =>
        ListarDescuentosLey().FirstOrDefault(item => item.IdDescuento == id);

    public List<Renta> ListarRentas()
    {
        const string consulta = @"
            SELECT r.id_renta, r.id_tipo_renta, tr.nombre_tramo, r.codigo_renta,
                   r.rango_minimo_renta, r.rango_maximo_renta, r.porcentaje_renta,
                   r.valor_fijo, r.fecha_vigencia_renta
            FROM dbo.RENTA r
            INNER JOIN dbo.TIPO_RENTA tr
                ON tr.id_tipo_renta = r.id_tipo_renta
            WHERE r.fecha_vigencia_renta = (
                SELECT MAX(fecha_vigencia_renta)
                FROM dbo.RENTA
                WHERE fecha_vigencia_renta <= CAST(GETDATE() AS DATE)
            )
            ORDER BY r.rango_minimo_renta, r.codigo_renta;";

        var rentas = new List<Renta>();
        using var conexionSql = new SqlConnection(_conexion.Cadena);
        using var cmd = new SqlCommand(consulta, conexionSql);
        conexionSql.Open();
        using var dr = cmd.ExecuteReader();

        while (dr.Read())
        {
            var idTipo = Convert.ToInt32(dr["id_tipo_renta"]);
            rentas.Add(new Renta
            {
                IdRenta = Convert.ToInt32(dr["id_renta"]),
                IdTipoRenta = idTipo,
                CodigoRenta = Convert.ToString(dr["codigo_renta"])?.Trim() ?? string.Empty,
                RangoMinimoRenta = Convert.ToDecimal(dr["rango_minimo_renta"]),
                RangoMaximoRenta = Convert.ToDecimal(dr["rango_maximo_renta"]),
                PorcentajeRenta = Convert.ToDecimal(dr["porcentaje_renta"]),
                ValorFijo = Convert.ToDecimal(dr["valor_fijo"]),
                FechaVigenciaRenta = Fecha(dr["fecha_vigencia_renta"]),
                TipoRenta = new TipoRenta
                {
                    IdTipoRenta = idTipo,
                    NombreTramo = Convert.ToString(dr["nombre_tramo"])?.Trim() ?? string.Empty
                }
            });
        }

        return rentas;
    }

    public Renta? ObtenerRenta(int id) =>
        ListarRentas().FirstOrDefault(item => item.IdRenta == id);

    public bool TienePlanillaCerradaDescuento(int id)
    {
        const string consulta = @"
            SELECT CASE WHEN EXISTS (
                SELECT 1
                FROM dbo.DETALLE_DESCUENTO_PLANILLA dd
                INNER JOIN dbo.DETALLE_PLANILLA dp ON dp.id_detalle_planilla = dd.id_detalle_planilla
                INNER JOIN dbo.PLANILLA p ON p.id_planilla = dp.id_planilla
                WHERE dd.id_descuento = @id_descuento
                  AND p.estado = 'CERRADA'
            ) THEN 1 ELSE 0 END;";

        return ExisteReferenciaCerrada(consulta, "@id_descuento", id);
    }

    public bool TienePlanillaCerradaRenta(int id)
    {
        const string consulta = @"
            SELECT CASE WHEN EXISTS (
                SELECT 1
                FROM dbo.DETALLE_PLANILLA dp
                INNER JOIN dbo.PLANILLA p ON p.id_planilla = dp.id_planilla
                WHERE dp.id_renta = @id_renta
                  AND p.estado = 'CERRADA'
            ) THEN 1 ELSE 0 END;";

        return ExisteReferenciaCerrada(consulta, "@id_renta", id);
    }

    public void CrearDescuentoLey(int idTipoDescuento, decimal porcentaje, decimal? topeMensual, DateOnly fechaVigencia)
    {
        ValidarDescuento(porcentaje, topeMensual);
        const string consulta = @"
            INSERT INTO dbo.DESCUENTO_LEY
                (id_tipo_descuento, porcentaje_descuento, tope_mensual, fecha_vigencia)
            VALUES
                (@id_tipo_descuento, @porcentaje, @tope_mensual, @fecha_vigencia);";

        using var conexionSql = new SqlConnection(_conexion.Cadena);
        using var cmd = new SqlCommand(consulta, conexionSql);
        cmd.Parameters.Add("@id_tipo_descuento", SqlDbType.Int).Value = idTipoDescuento;
        AgregarDecimal(cmd, "@porcentaje", porcentaje, 5, 2);
        AgregarDecimalNullable(cmd, "@tope_mensual", topeMensual, 10, 2);
        cmd.Parameters.Add("@fecha_vigencia", SqlDbType.Date).Value = fechaVigencia.ToDateTime(TimeOnly.MinValue);
        conexionSql.Open();
        cmd.ExecuteNonQuery();
    }

    public void ActualizarDescuentoLey(int id, int idTipoDescuento, decimal porcentaje, decimal? topeMensual, DateOnly fechaVigencia)
    {
        ValidarDescuento(porcentaje, topeMensual);
        const string consulta = @"
            UPDATE dbo.DESCUENTO_LEY
            SET id_tipo_descuento = @id_tipo_descuento,
                porcentaje_descuento = @porcentaje,
                tope_mensual = @tope_mensual,
                fecha_vigencia = @fecha_vigencia
            WHERE id_descuento = @id_descuento;";

        using var conexionSql = new SqlConnection(_conexion.Cadena);
        using var cmd = new SqlCommand(consulta, conexionSql);
        cmd.Parameters.Add("@id_descuento", SqlDbType.Int).Value = id;
        cmd.Parameters.Add("@id_tipo_descuento", SqlDbType.Int).Value = idTipoDescuento;
        AgregarDecimal(cmd, "@porcentaje", porcentaje, 5, 2);
        AgregarDecimalNullable(cmd, "@tope_mensual", topeMensual, 10, 2);
        cmd.Parameters.Add("@fecha_vigencia", SqlDbType.Date).Value = fechaVigencia.ToDateTime(TimeOnly.MinValue);
        conexionSql.Open();
        if (cmd.ExecuteNonQuery() == 0)
        {
            throw new InvalidOperationException("El descuento de ley indicado no existe.");
        }
    }

    public void CrearRenta(int idTipoRenta, string codigoRenta, decimal rangoMinimo, decimal rangoMaximo, decimal porcentaje, decimal valorFijo, DateOnly fechaVigencia)
    {
        ValidarRenta(codigoRenta, rangoMinimo, rangoMaximo, porcentaje, valorFijo);
        const string consulta = @"
            INSERT INTO dbo.RENTA
                (id_tipo_renta, codigo_renta, rango_minimo_renta, rango_maximo_renta,
                 porcentaje_renta, valor_fijo, fecha_vigencia_renta)
            VALUES
                (@id_tipo_renta, @codigo_renta, @rango_minimo, @rango_maximo,
                 @porcentaje, @valor_fijo, @fecha_vigencia);";

        using var conexionSql = new SqlConnection(_conexion.Cadena);
        using var cmd = new SqlCommand(consulta, conexionSql);
        AgregarParametrosRenta(cmd, idTipoRenta, codigoRenta, rangoMinimo, rangoMaximo, porcentaje, valorFijo, fechaVigencia);
        conexionSql.Open();
        cmd.ExecuteNonQuery();
    }

    public void ActualizarRenta(int id, int idTipoRenta, string codigoRenta, decimal rangoMinimo, decimal rangoMaximo, decimal porcentaje, decimal valorFijo, DateOnly fechaVigencia)
    {
        ValidarRenta(codigoRenta, rangoMinimo, rangoMaximo, porcentaje, valorFijo);
        const string consulta = @"
            UPDATE dbo.RENTA
            SET id_tipo_renta = @id_tipo_renta,
                codigo_renta = @codigo_renta,
                rango_minimo_renta = @rango_minimo,
                rango_maximo_renta = @rango_maximo,
                porcentaje_renta = @porcentaje,
                valor_fijo = @valor_fijo,
                fecha_vigencia_renta = @fecha_vigencia
            WHERE id_renta = @id_renta;";

        using var conexionSql = new SqlConnection(_conexion.Cadena);
        using var cmd = new SqlCommand(consulta, conexionSql);
        cmd.Parameters.Add("@id_renta", SqlDbType.Int).Value = id;
        AgregarParametrosRenta(cmd, idTipoRenta, codigoRenta, rangoMinimo, rangoMaximo, porcentaje, valorFijo, fechaVigencia);
        conexionSql.Open();
        if (cmd.ExecuteNonQuery() == 0)
        {
            throw new InvalidOperationException("El tramo de renta indicado no existe.");
        }
    }

    private bool ExisteReferenciaCerrada(string consulta, string nombreParametro, int id)
    {
        using var conexionSql = new SqlConnection(_conexion.Cadena);
        using var cmd = new SqlCommand(consulta, conexionSql);
        cmd.Parameters.Add(nombreParametro, SqlDbType.Int).Value = id;
        conexionSql.Open();
        return Convert.ToInt32(cmd.ExecuteScalar()) == 1;
    }

    private static void AgregarParametrosRenta(SqlCommand cmd, int idTipoRenta, string codigoRenta, decimal rangoMinimo, decimal rangoMaximo, decimal porcentaje, decimal valorFijo, DateOnly fechaVigencia)
    {
        cmd.Parameters.Add("@id_tipo_renta", SqlDbType.Int).Value = idTipoRenta;
        cmd.Parameters.Add("@codigo_renta", SqlDbType.VarChar, 20).Value = codigoRenta.Trim();
        AgregarDecimal(cmd, "@rango_minimo", rangoMinimo, 10, 2);
        AgregarDecimal(cmd, "@rango_maximo", rangoMaximo, 10, 2);
        AgregarDecimal(cmd, "@porcentaje", porcentaje, 10, 2);
        AgregarDecimal(cmd, "@valor_fijo", valorFijo, 10, 2);
        cmd.Parameters.Add("@fecha_vigencia", SqlDbType.Date).Value = fechaVigencia.ToDateTime(TimeOnly.MinValue);
    }

    private static void ValidarDescuento(decimal porcentaje, decimal? topeMensual)
    {
        if (porcentaje <= 0 || porcentaje > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(porcentaje), "El porcentaje debe estar entre 0 y 100.");
        }

        if (topeMensual.HasValue && topeMensual.Value <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(topeMensual), "El tope mensual debe ser mayor que cero.");
        }
    }

    private static void ValidarRenta(string codigoRenta, decimal rangoMinimo, decimal rangoMaximo, decimal porcentaje, decimal valorFijo)
    {
        if (string.IsNullOrWhiteSpace(codigoRenta))
        {
            throw new ArgumentException("El código del tramo es obligatorio.", nameof(codigoRenta));
        }

        if (rangoMinimo < 0 || rangoMaximo <= rangoMinimo)
        {
            throw new ArgumentOutOfRangeException(nameof(rangoMaximo), "El rango máximo debe ser mayor que el rango mínimo.");
        }

        if (porcentaje is < 0 or > 100 || valorFijo < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(porcentaje), "El porcentaje o valor fijo no es válido.");
        }
    }

    private static void AgregarDecimal(SqlCommand cmd, string nombre, decimal valor, byte precision, byte scale)
    {
        var parametro = cmd.Parameters.Add(nombre, SqlDbType.Decimal);
        parametro.Precision = precision;
        parametro.Scale = scale;
        parametro.Value = valor;
    }

    private static void AgregarDecimalNullable(SqlCommand cmd, string nombre, decimal? valor, byte precision, byte scale)
    {
        var parametro = cmd.Parameters.Add(nombre, SqlDbType.Decimal);
        parametro.Precision = precision;
        parametro.Scale = scale;
        parametro.Value = (object?)valor ?? DBNull.Value;
    }

    private static DateOnly Fecha(object valor) => DateOnly.FromDateTime(Convert.ToDateTime(valor));
}
