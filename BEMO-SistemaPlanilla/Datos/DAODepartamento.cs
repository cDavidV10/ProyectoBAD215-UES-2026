using BEMO.Entidades;
using Microsoft.Data.SqlClient;
using System.Data;


namespace BEMO.Datos
{
    public class DAODepartamento
    {
        private readonly Conexion _conexion;

        public DAODepartamento(Conexion conexion)
        {
            _conexion = conexion;
        }

        public List<Departamento> Listar()
        {
            List<Departamento> datos = new List<Departamento>();
            string consulta = "select * from DEPARTAMENTO order by id_departamento";

            try
            {
                using var conexionSql = new SqlConnection(_conexion.Cadena);
                using var cmd = new SqlCommand(consulta, conexionSql);

                conexionSql.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                    {
                        datos.Add(
                            new Departamento
                            {
                                IdDepartamento = dr.GetInt32(0),
                                NombreDepartamento = dr.GetString(1)
                            }
                        );
                    }
            }
            catch (Exception)
            {
                
                throw;
            }
        


            return datos;
        }
    }
}
