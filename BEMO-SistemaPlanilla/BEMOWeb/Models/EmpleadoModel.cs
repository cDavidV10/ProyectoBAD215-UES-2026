using BEMO.Entidades;

namespace BEMOWeb.Models;

public class EmpleadoModel
{
    public int EmpleadosActivos { get; init; }

    public int EmpleadosInactivos { get; init; }

    public int EmpleadosTotales { get; init;}

    public List<Departamento> Departamentos { get; init; } = new();

    public List<Empleado> ListaEmpleados { get; init; } = new();

    public int PaginaActual { get; init; }

    public int TotalPaginas { get; init; }

    public int TotalRegistros { get; init; }

    public int TamanoPagina { get; init; } = 5;
}
