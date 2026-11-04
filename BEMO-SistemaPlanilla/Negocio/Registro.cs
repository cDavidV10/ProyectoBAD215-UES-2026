using Microsoft.Extensions.DependencyInjection;

namespace BEMO.Datos;

public static class RegistroDatos
{
    public static IServiceCollection AgregarDatos(this IServiceCollection servicios, string cadenaConexion)
    {
        servicios.AddSingleton(new Conexion(cadenaConexion));
        servicios.AddScoped<DAODeptoUbicacion>();
        return servicios;
    }
}