using BEMO.Entidades;
using Microsoft.Data.SqlClient;
using System.Data;


namespace BEMO.Datos
{
    
    public class DAOEmpleado
    {
        private readonly Conexion _conexion;

        public DAOEmpleado(Conexion conexion)
        {
            _conexion = conexion;
        }

        public int UsuariosActivos()
        {
            return ContarEmpleadosPorEstado("ACTIVO");
        }

        public int EmpleadosInactivos()
        {
            return ContarEmpleadosPorEstado("INACTIVO");
        }

        public int EmpleadosTotales()
        {
            return ContarEmpleadosPorEstado("ACTIVO") + ContarEmpleadosPorEstado("INACTIVO");
        }

        private int ContarEmpleadosPorEstado(string estado)
        {
            const string consulta = @"
                SELECT COUNT(id_empleado)
                FROM EMPLEADO
                WHERE estado = @estado;";

            using var conexionSql = new SqlConnection(_conexion.Cadena);
            using var cmd = new SqlCommand(consulta, conexionSql);
            cmd.Parameters.Add("@estado", SqlDbType.VarChar, 20).Value = estado;

            conexionSql.Open();
            return Convert.ToInt32(cmd.ExecuteScalar());
        }
    }
}
