using System;
using BEMO.Datos;
using BEMO.Entidades;

namespace BEMO.Negocio
{
    public class CN_Empleado
    {
        private readonly DAOEmpleado _metodos;

        public CN_Empleado(DAOEmpleado metodos)
        {
            _metodos = metodos;
        }

        public int UsuariosActivos()
        {
            return _metodos.UsuariosActivos();
        }

        public int EmpleadosInactivos()
        {
            return _metodos.EmpleadosInactivos();
        }

        public int EmpladosTotales()
        {
            return _metodos.EmpleadosTotales();
        }
    }
}
