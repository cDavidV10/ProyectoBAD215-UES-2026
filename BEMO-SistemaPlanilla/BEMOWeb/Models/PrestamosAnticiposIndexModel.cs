using BEMO.Entidades;

namespace BEMOWeb.Models;

public class PrestamosAnticiposIndexModel
{
    public string TipoSeleccionado { get; set; } = "prestamos";
    public string? Busqueda { get; set; }
    public string? EstadoSeleccionado { get; set; }
    public List<PrestamoListado> Prestamos { get; set; } = new();
    public List<AnticipoSalarioListado> Anticipos { get; set; } = new();
    public PrestamosAnticiposResumen Resumen { get; set; } = new();
    public int PaginaActual { get; set; } = 1;
    public int TotalPaginas { get; set; } = 1;
    public int TotalRegistros { get; set; }
    public int TamanoPagina { get; set; } = 5;
}
