using BEMO.Datos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BEMO.Negocio;

public static class Registro
{
    public static IServiceCollection AgregarNegocio(this IServiceCollection servicios, string cadenaConexion)
    {
        servicios.AddDbContext<BemoContext>(opciones => opciones.UseSqlServer(cadenaConexion));
        servicios.AddScoped<ConexionNegocio>();
        return servicios;
    }
}