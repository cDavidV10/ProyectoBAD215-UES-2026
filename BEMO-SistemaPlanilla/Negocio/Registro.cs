using Microsoft.Extensions.DependencyInjection;

namespace BEMO.Datos;

public static class RegistroDatos
{
    public static IServiceCollection AgregarDatos(this IServiceCollection servicios, string cadenaConexion)
    {
        servicios.AddSingleton(new Conexion(cadenaConexion));
        servicios.AddScoped<DAODeptoUbicacion>();
        servicios.AddScoped<DAOMunicipioUbicacion>();
        servicios.AddScoped<DAOAfp>();
        servicios.AddScoped<DAOCargo>();
        servicios.AddScoped<DAOTipoContrato>();
        servicios.AddScoped<DAOContrato>();
        servicios.AddScoped<DAODepartamento>();
        servicios.AddScoped<DAOEmpleado>();
        servicios.AddScoped<DAOPlanilla>();
        servicios.AddScoped<DAOPrestamosAnticipos>();
        servicios.AddScoped<DAOUsuario>();
        servicios.AddScoped<DAOLogin>();
        return servicios;
    }
}
