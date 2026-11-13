namespace BEMO.Entidades;

public class RegistroAsistenciaPlanilla
{
    public int IdDetallePlanilla { get; set; }
    public int IdPlanilla { get; set; }
    public int NumeroPlanilla { get; set; }
    public DateOnly PeriodoInicio { get; set; }
    public DateOnly PeriodoFin { get; set; }
    public int IdEmpleado { get; set; }
    public string NombreEmpleado { get; set; } = string.Empty;
    public string NombreDepartamento { get; set; } = string.Empty;
    public int DiasTrabajados { get; set; }
    public int DiasNoTrabajados { get; set; }
    public int HorasExtras { get; set; }
    public decimal Bonificaciones { get; set; }
}
