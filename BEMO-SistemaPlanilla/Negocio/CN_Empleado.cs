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

        public List<Empleado> ListarActivosSinContrato()
        {
            return _metodos.ListarActivosSinContrato();
        }

        public (List<Empleado> Empleados, int Total) ListarPaginado(
            int pagina,
            int tamanoPagina,
            string? busqueda,
            string? departamento,
            string? estado,
            int? totalSinFiltros = null)
        {
            return _metodos.ListarPaginado(pagina, tamanoPagina, busqueda, departamento, estado, totalSinFiltros);
        }

        public Empleado? ObtenerPorId(int idEmpleado)
        {
            return _metodos.ObtenerPorId(idEmpleado);
        }

        public void ActualizarDatosPersonales(
            int idEmpleado,
            string nombre,
            string apellido,
            string? nit,
            string numeroIsss,
            string telefono,
            string? correo,
            int idAfp,
            int idMunicipio,
            string nombreCalle,
            string numeroCasa,
            string estado)
        {
            _metodos.ActualizarDatosPersonales(
                idEmpleado,
                nombre,
                apellido,
                nit,
                numeroIsss,
                telefono,
                correo,
                idAfp,
                idMunicipio,
                nombreCalle,
                numeroCasa,
                estado);
        }

        public int Registrar(
            Empleado empleado,
            int idMunicipio,
            string nombreCalle,
            string numeroCasa,
            int idCargo,
            int idDepartamento,
            int idTipoContrato,
            decimal salarioBruto,
            DateOnly fechaContratacion,
            DateOnly? fechaCulminacion)
        {
            return _metodos.Registrar(
                empleado,
                idMunicipio,
                nombreCalle,
                numeroCasa,
                idCargo,
                idDepartamento,
                idTipoContrato,
                salarioBruto,
                fechaContratacion,
                fechaCulminacion);
        }

        public (int Activos, int Inactivos, int Total) ObtenerResumen()
        {
            return _metodos.ObtenerResumen();
        }
    }
}
