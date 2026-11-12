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

        public (List<Empleado> Empleados, int Total) ListarPaginado(int pagina, int tamanoPagina, string? busqueda, string? departamento, string? estado)
        {
            return _metodos.ListarPaginado(pagina, tamanoPagina, busqueda, departamento, estado);
        }

        public (int Activos, int Inactivos, int Total) ObtenerResumen()
        {
            return _metodos.ObtenerResumen();
        }
    }
}
