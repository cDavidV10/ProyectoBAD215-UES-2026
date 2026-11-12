using BEMO.Entidades;
using Microsoft.Data.SqlClient;

namespace BEMO.Datos;

public class DAOTipoContrato
{
    private readonly Conexion _conexion;

    public DAOTipoContrato(Conexion conexion)
    {
        _conexion = conexion;
    }

    public List<TipoContrato> Listar()
    {
        var tipos = new List<TipoContrato>();
        const string consulta = @"
            SELECT id_tipo_contrato, nombre_tipo_contrato
            FROM dbo.TIPO_CONTRATO
            ORDER BY nombre_tipo_contrato;";

        using var conexionSql = new SqlConnection(_conexion.Cadena);
        using var cmd = new SqlCommand(consulta, conexionSql);

        conexionSql.Open();
        using var dr = cmd.ExecuteReader();
        while (dr.Read())
        {
            tipos.Add(new TipoContrato
            {
                IdTipoContrato = dr.GetInt32(0),
                NombreTipoContrato = dr.GetString(1)
            });
        }

        return tipos;
    }
}
