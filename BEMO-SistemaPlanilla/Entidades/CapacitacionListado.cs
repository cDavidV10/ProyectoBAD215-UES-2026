namespace BEMO.Entidades;

public class CapacitacionListado
{
    public int IdCapacitacion { get; set; }
    public int IdTipoCapacitacion { get; set; }
    public string NombreCapacitacion { get; set; } = string.Empty;
    public string NombreTipoCapacitacion { get; set; } = string.Empty;
    public DateOnly? FechaInicio { get; set; }
    public DateOnly? FechaFin { get; set; }
    public int EmpleadosParticipantes { get; set; }
    public int Participaciones { get; set; }
    public decimal? PromedioResultados { get; set; }
    public decimal? MejorResultado { get; set; }
    public decimal? MenorResultado { get; set; }
    public string Estado { get; set; } = string.Empty;
}
