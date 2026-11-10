using BEMO.Entidades;
using Microsoft.Data.SqlClient;

namespace BEMO.Datos
{
    public class DAOLogin
    {
        private readonly Conexion _conexion;

        public DAOLogin(Conexion conexion)
        {
            _conexion = conexion;
        }

        public Usuario? Buscar(string user)
        {
            string consulta = @"
                    select
                        u.id_usuario,
                        u.id_empleado,
                        r.id_rol,
                        u.nombre_usuario,
                        u.password_hash,
                        r.nombre_rol as NombreRol
                    from USUARIO u
                    inner join ROL r on r.id_rol = u.id_rol
                    where u.nombre_usuario = @Usuario";

            try
            {
                using (SqlConnection conexionSql = new SqlConnection(_conexion.Cadena))
                {
                    using SqlCommand cmd = new SqlCommand(consulta, conexionSql);
                    cmd.Parameters.AddWithValue("@Usuario", user.Trim());
                    conexionSql.Open();

                    using SqlDataReader dr = cmd.ExecuteReader();
                    if (!dr.Read())
                    {
                        return null;
                    }

                    var rol = new Rol
                    {
                        IdRol = dr.GetInt32(2),
                        NombreRol = dr.GetString(5)
                    };

                    return new Usuario
                    {
                        IdUsuario = dr.GetInt32(0),
                        IdEmpleado = dr.GetInt32(1),
                        NombreUsuario = dr.GetString(3),
                        PasswordHash = dr.IsDBNull(4) ? string.Empty : dr.GetString(4),
                        Rol = rol
                    };
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
