using BEMO.Entidades;
using Microsoft.Data.SqlClient;

namespace BEMO.Datos;

public class DAOCargo
{
    private readonly Conexion _conexion;

    public DAOCargo(Conexion conexion)
    {
        _conexion = conexion;
    }

    public List<Cargo> Listar()
    {
        var cargos = new List<Cargo>();
        const string consulta = @"
            SELECT id_cargo, nombre_cargo, rango_minimo_cargo, rango_maximo_cargo
            FROM dbo.CARGO
            ORDER BY nombre_cargo;";

        using var conexionSql = new SqlConnection(_conexion.Cadena);
        using var cmd = new SqlCommand(consulta, conexionSql);

        conexionSql.Open();
        using var dr = cmd.ExecuteReader();
        while (dr.Read())
        {
            cargos.Add(new Cargo
            {
                IdCargo = dr.GetInt32(0),
                NombreCargo = dr.GetString(1),
                RangoMinimoCargo = dr.GetDecimal(2),
                RangoMaximoCargo = dr.GetDecimal(3)
            });
        }

        return cargos;
    }
}
