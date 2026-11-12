using BEMO.Entidades;
using Microsoft.Data.SqlClient;

namespace BEMO.Datos;

public class DAOAfp
{
    private readonly Conexion _conexion;

    public DAOAfp(Conexion conexion)
    {
        _conexion = conexion;
    }

    public List<Afp> Listar()
    {
        var afps = new List<Afp>();
        const string consulta = @"
            SELECT id_afp, nombre_afp
            FROM dbo.AFP
            ORDER BY nombre_afp;";

        using var conexionSql = new SqlConnection(_conexion.Cadena);
        using var cmd = new SqlCommand(consulta, conexionSql);

        conexionSql.Open();
        using var dr = cmd.ExecuteReader();
        while (dr.Read())
        {
            afps.Add(new Afp
            {
                IdAfp = dr.GetInt32(0),
                NombreAfp = dr.GetString(1)
            });
        }

        return afps;
    }
}
