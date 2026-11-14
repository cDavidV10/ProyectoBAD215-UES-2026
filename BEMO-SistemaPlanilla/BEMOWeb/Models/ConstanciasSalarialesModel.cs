using BEMO.Entidades;

namespace BEMOWeb.Models;

public class ConstanciasSalarialesIndexModel
{
    public List<ConstanciaEmpleadoOpcion> Empleados { get; init; } = new();
    public string? Busqueda { get; init; }
    public DateOnly FechaEmision { get; init; }
    public int PaginaActual { get; init; }
    public int TotalPaginas { get; init; }
    public int TotalRegistros { get; init; }
    public int TamanoPagina { get; init; }
}

public class ConstanciaSalarialViewModel
{
    public ConstanciaSalarial Constancia { get; init; } = new();
    public int Meses { get; init; }
}
