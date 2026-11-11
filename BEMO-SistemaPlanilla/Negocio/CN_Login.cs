using System;
using BEMO.Datos;
using BEMO.Entidades;

namespace BEMO.Negocio
{
    public class CN_Login
    {
        private readonly DAOLogin _metodos;

        public CN_Login(DAOLogin metodos)
        {
            _metodos = metodos;
        }

        public Usuario? Buscar(string user)
        {
            return _metodos.Buscar(user);
        }

        public bool ValidarCredenciales(
            string user,
            string clave,
            out Usuario? usuario)
        {
            usuario = null;

            if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(clave))
            {
                return false;
            }

            usuario = _metodos.Autenticar(user, clave);
            return usuario is not null;
        }
    }
}
