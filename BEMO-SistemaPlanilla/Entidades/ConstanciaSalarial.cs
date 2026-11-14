namespace BEMO.Entidades;

public class ConstanciaSalarial
{
    public ConstanciaSalarialEncabezado Encabezado { get; init; } = new();
    public List<ConstanciaSalarialPeriodo> Periodos { get; init; } = new();
    public ConstanciaSalarialResumen Resumen { get; init; } = new();
}

public class ConstanciaSalarialEncabezado
{
    public int IdEmpleado { get; init; }
    public string NombreCompleto { get; init; } = string.Empty;
    public string Dui { get; init; } = string.Empty;
    public string NumeroIsss { get; init; } = string.Empty;
    public string NombreTipoContrato { get; init; } = string.Empty;
    public string NombreCargo { get; init; } = string.Empty;
    public string NombreDepartamento { get; init; } = string.Empty;
    public decimal SalarioBaseActual { get; init; }
    public DateOnly FechaIngreso { get; init; }
    public DateOnly PeriodoDesde { get; init; }
    public DateOnly PeriodoHasta { get; init; }
}

public class ConstanciaSalarialPeriodo
{
    public int NumeroPlanilla { get; init; }
    public DateOnly PeriodoInicio { get; init; }
    public DateOnly PeriodoFin { get; init; }
    public DateOnly FechaPago { get; init; }
    public decimal TotalBruto { get; init; }
    public decimal TotalDeducciones { get; init; }
    public decimal SalarioLiquido { get; init; }
}

public class ConstanciaSalarialResumen
{
    public int PlanillasIncluidas { get; init; }
    public decimal TotalBruto { get; init; }
    public decimal TotalDeducciones { get; init; }
    public decimal TotalLiquido { get; init; }
    public decimal PromedioMensualBruto { get; init; }
    public decimal PromedioMensualLiquido { get; init; }
}
