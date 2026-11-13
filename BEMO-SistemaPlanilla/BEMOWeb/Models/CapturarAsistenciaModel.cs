namespace BEMOWeb.Models;

public class CapturarAsistenciaModel
{
    public int IdPlanilla { get; set; }
    public int Pagina { get; set; } = 1;
    public List<AsistenciaInputModel> Asistencias { get; set; } = new();
}

public class AsistenciaInputModel
{
    public int IdDetallePlanilla { get; set; }
    public int DiasTrabajados { get; set; }
    public int DiasNoTrabajados { get; set; }
    public int HorasExtras { get; set; }
    public decimal Bonificaciones { get; set; }
}
