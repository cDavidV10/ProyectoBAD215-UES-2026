using BEMO.Entidades;
using Microsoft.Data.SqlClient;
using System.Data;

namespace BEMO.Datos
{
    public class DAOLogin
    {
        private readonly Conexion _conexion;

        public DAOLogin(Conexion conexion)
        {
            _conexion = conexion;
        }

        public Usuario? Autenticar(string user, string clave)
        {
            using var conexionSql = new SqlConnection(_conexion.Cadena);
            using var cmd = new SqlCommand("dbo.sp_Autenticar", conexionSql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add("@nombre_usuario", SqlDbType.VarChar, 50).Value = user.Trim();
            cmd.Parameters.Add("@password_plana", SqlDbType.VarChar, 255).Value = clave;

            conexionSql.Open();

            using var dr = cmd.ExecuteReader();
            if (!dr.Read())
            {
                return null;
            }

            var idUsuario = dr.GetInt32(dr.GetOrdinal("id_usuario"));
            var idEmpleado = dr.GetInt32(dr.GetOrdinal("id_empleado"));
            var idRol = dr.GetInt32(dr.GetOrdinal("id_rol"));
            var nombreUsuario = dr.GetString(dr.GetOrdinal("nombre_usuario"));
            var nombreRol = dr.GetString(dr.GetOrdinal("nombre_rol"));

            return new Usuario
            {
                IdUsuario = idUsuario,
                IdEmpleado = idEmpleado,
                IdRol = idRol,
                NombreUsuario = nombreUsuario,
                Rol = new Rol
                {
                    IdRol = idRol,
                    NombreRol = nombreRol
                }
            };
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
