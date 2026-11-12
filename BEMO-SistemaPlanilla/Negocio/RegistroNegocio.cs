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
        servicios.AddScoped<CN_DeptoUbicacion>();
        servicios.AddScoped<CN_MunicipioUbicacion>();
        servicios.AddScoped<CN_Afp>();
        servicios.AddScoped<CN_Cargo>();
        servicios.AddScoped<CN_TipoContrato>();
        servicios.AddScoped<CN_Contrato>();
        servicios.AddScoped<global::Negocio.CN_Departamento>();
        servicios.AddScoped<CN_Empleado>();
        servicios.AddScoped<CN_Login>();

        return servicios;
    }
}
