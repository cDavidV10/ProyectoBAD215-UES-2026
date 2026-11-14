namespace BEMOWeb.Models;

public class CapacitacionResultadosModel
{
    public int IdCapacitacion { get; set; }

    public List<CapacitacionResultadoModel> Resultados { get; set; } = new();
}

public class CapacitacionResultadoModel
{
    public int IdDetalleCapacitacion { get; set; }

    public decimal Resultado { get; set; }
}
