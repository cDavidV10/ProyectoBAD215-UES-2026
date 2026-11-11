using BEMO.Entidades;

namespace BEMOWeb.Models;

public class EmpleadoModel
{
    public int EmpleadosActivos { get; init; }

    public int EmpleadosInactivos { get; init; }

    public int EmpleadosTotales { get; init;}

    public List<Departamento> Departamentos { get; init; } = new();
}
