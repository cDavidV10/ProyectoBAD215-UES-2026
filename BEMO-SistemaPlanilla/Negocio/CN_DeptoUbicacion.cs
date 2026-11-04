using BEMO.Datos;
using BEMO.Entidades;

namespace BEMO.Negocio
{
    public class CN_DeptoUbicacion
    {
        private readonly DAODeptoUbicacion _metodos;

        public CN_DeptoUbicacion(DAODeptoUbicacion metodos)
        {
            _metodos = metodos;
        }

        public List<DepartamentoUbicacion> listar()
        {
            return _metodos.listar();
        }
    }
}
