using System;
using BEMO.Datos;
using BEMO.Entidades;
using Microsoft.AspNetCore.Identity;

namespace BEMO.Negocio
{
    public class CN_Login
    {
        private readonly DAOLogin _metodos;
        private readonly PasswordHasher<string> _passwordHasher = new();

        public CN_Login(DAOLogin metodos)
        {
            _metodos = metodos;
        }

        public Usuario? Buscar(string user)
        {
            return _metodos.Buscar(user);
        }

        public bool ValidarCredenciales(string user, string clave, out Usuario? usuario)
        {
            usuario = null;

            if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(clave))
            {
                return false;
            }

            var encontrado = _metodos.Buscar(user);
            if (encontrado == null || string.IsNullOrWhiteSpace(encontrado.PasswordHash))
            {
                return false;
            }

            var resultado = _passwordHasher.VerifyHashedPassword(
                encontrado.NombreUsuario, encontrado.PasswordHash, clave);

            if (resultado == PasswordVerificationResult.Failed)
            {
                return false;
            }

            usuario = encontrado;
            return true;
        }
    }
}