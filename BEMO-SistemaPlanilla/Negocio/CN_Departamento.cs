using System;
using System.Collections.Generic;
using System.Text;
using BEMO.Datos;
using BEMO.Entidades;

namespace Negocio
{
    public class CN_Departamento
    {
        private readonly DAODepartamento _metodos;

        public CN_Departamento(DAODepartamento metodos)
        {
            _metodos = metodos;
        }

        public List<Departamento> Listar()
        {
            return _metodos.Listar();
        }
    }
}
