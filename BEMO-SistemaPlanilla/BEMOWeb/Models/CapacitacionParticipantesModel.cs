using BEMO.Entidades;
using System.ComponentModel.DataAnnotations;

namespace BEMOWeb.Models;

public class CapacitacionParticipantesModel
{
    public int IdCapacitacion { get; set; }
    public string NombreCapacitacion { get; set; } = string.Empty;
    public List<EmpleadoCapacitacionListado> Empleados { get; set; } = new();
    public List<Departamento> Departamentos { get; set; } = new();
    public string? Busqueda { get; set; }
    public string? DepartamentoSeleccionado { get; set; }
    public int PaginaActual { get; set; } = 1;
    public int TotalPaginas { get; set; } = 1;
    public int TotalRegistros { get; set; }
    public int TamanoPagina { get; set; } = 8;

    [Required(ErrorMessage = "Indique la fecha de inicio.")]
    public DateOnly? FechaInicio { get; set; }

    [Required(ErrorMessage = "Indique la fecha de finalización.")]
    public DateOnly? FechaFin { get; set; }

    public List<int> IdEmpleadosSeleccionados { get; set; } = new();

    public bool EstaSeleccionado(int idEmpleado)
    {
        return IdEmpleadosSeleccionados.Contains(idEmpleado);
    }
}
