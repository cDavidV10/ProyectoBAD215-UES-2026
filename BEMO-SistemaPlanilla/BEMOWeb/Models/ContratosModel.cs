using BEMO.Entidades;

namespace BEMOWeb.Models;

public class ContratosModel
{
    public int ContratosVigentes { get; init; }

    public int ContratosPorVencer { get; init; }

    public List<ContratoListado> Contratos { get; init; } = new();

    public List<ContratoEmpleadoOption> Empleados { get; init; } = new();

    public List<string> TiposContrato { get; init; } = new();

    public int? EmpleadoSeleccionado { get; init; }

    public string? TipoSeleccionado { get; init; }

    public string? EstadoSeleccionado { get; init; }

    public string? Busqueda { get; init; }

    public int PaginaActual { get; init; }

    public int TotalPaginas { get; init; }

    public int TotalRegistros { get; init; }

    public int TamanoPagina { get; init; }
}

public class ContratoEmpleadoOption
{
    public int IdEmpleado { get; init; }

    public string Nombre { get; init; } = string.Empty;
}
