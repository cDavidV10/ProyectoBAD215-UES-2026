namespace BEMO.Entidades;

public class PrestamosAnticiposResumen
{
    public int PrestamosActivos { get; set; }
    public int PrestamosPagados { get; set; }
    public decimal SaldoPrestamos { get; set; }
    public int AnticiposPendientes { get; set; }
    public int AnticiposDescontados { get; set; }
    public decimal MontoAnticiposPendientes { get; set; }
}
