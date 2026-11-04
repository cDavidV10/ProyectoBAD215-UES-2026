using BEMO.Entidades;
using Microsoft.Data.SqlClient;

namespace BEMO.Datos
{
    public class DAODeptoUbicacion
    {
        private readonly Conexion _conexion;

        public DAODeptoUbicacion(Conexion conexion)
        {
            _conexion = conexion;
        }

        public List<DepartamentoUbicacion> listar()
        {
            List<DepartamentoUbicacion> lista = new List<DepartamentoUbicacion>();

            try {
                using(SqlConnection conexionSql = new SqlConnection(_conexion.Cadena))
                {
                    string consulta = "SELECT id_departamento_ubicacion, nombre_departamento_ubicacion FROM DEPARTAMENTO_UBICACION";

                    SqlCommand cmd = new SqlCommand(consulta, conexionSql);

                    conexionSql.Open();
                    SqlDataReader dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        DepartamentoUbicacion depto = new DepartamentoUbicacion();
                        depto.IdDepartamentoUbicacion = dr.GetInt32(0);
                        depto.NombreDepartamentoUbicacion = dr.GetString(1);
                        lista.Add(depto);
                    }
                }
            }
            catch { }

            return lista;
        } 
    }
}
