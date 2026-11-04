using BEMO.Datos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BEMO.Negocio;

public static class RegistroNegocio
{
    public static IServiceCollection AgregarNegocio(
        this IServiceCollection servicios,
        string cadenaConexion)
    {
        servicios.AddDbContext<BemoContext>(opciones =>
            opciones.UseSqlServer(cadenaConexion));

        servicios.AgregarDatos(cadenaConexion);
        servicios.AddScoped<ConexionNegocio>();
        servicios.AddScoped<CN_DeptoUbicacion>();

        return servicios;
    }
}
