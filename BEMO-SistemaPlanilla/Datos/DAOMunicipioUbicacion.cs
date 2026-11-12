using BEMO.Entidades;
using Microsoft.Data.SqlClient;

namespace BEMO.Datos;

public class DAOMunicipioUbicacion
{
    private readonly Conexion _conexion;

    public DAOMunicipioUbicacion(Conexion conexion)
    {
        _conexion = conexion;
    }

    public List<MunicipioUbicacion> Listar()
    {
        var municipios = new List<MunicipioUbicacion>();
        const string consulta = @"
            SELECT id_municipio, id_departamento_ubicacion, nombre_municipio
            FROM dbo.MUNICIPIO_UBICACION
            ORDER BY id_departamento_ubicacion, nombre_municipio;";

        using var conexionSql = new SqlConnection(_conexion.Cadena);
        using var cmd = new SqlCommand(consulta, conexionSql);

        conexionSql.Open();
        using var dr = cmd.ExecuteReader();
        while (dr.Read())
        {
            municipios.Add(new MunicipioUbicacion
            {
                IdMunicipio = dr.GetInt32(0),
                IdDepartamentoUbicacion = dr.GetInt32(1),
                NombreMunicipio = dr.GetString(2)
            });
        }

        return municipios;
    }
}
