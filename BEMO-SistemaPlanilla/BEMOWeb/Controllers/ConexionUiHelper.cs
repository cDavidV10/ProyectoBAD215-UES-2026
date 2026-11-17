using BEMO.Datos;
using Microsoft.AspNetCore.Mvc;

namespace BEMOWeb.Controllers;

/// <summary>
/// Centraliza la verificación de conexión a la base de datos para la pantalla de inicio de sesión.
/// </summary>
internal static class ConexionUiHelper
{
    public static bool CargarEstadoConexion(
        Controller controller,
        Conexion conexion,
        IWebHostEnvironment entorno)
    {
        var conectado = conexion.ProbarConexion(out var errorConexion, out var advertenciaPermisos);
        controller.ViewBag.Conectado = conectado;

        if (entorno.IsDevelopment())
        {
            controller.ViewBag.ErrorConexion = errorConexion;
            controller.ViewBag.AdvertenciaPermisos = advertenciaPermisos;
        }

        return conectado;
    }
}
