using BEMO.Datos;
using BEMO.Entidades;

namespace BEMO.Negocio;

public class CN_PrestamosAnticipos
{
    private readonly DAOPrestamosAnticipos _metodos;

    public CN_PrestamosAnticipos(DAOPrestamosAnticipos metodos)
    {
        _metodos = metodos;
    }

    public List<Empleado> ListarEmpleadosActivos() => _metodos.ListarEmpleadosActivos();

    public List<PrestamoListado> ListarPrestamos(string? busqueda = null, string? estado = null) =>
        _metodos.ListarPrestamos(busqueda, estado);

    public PrestamoListado? ObtenerPrestamo(int idPrestamo) => _metodos.ObtenerPrestamo(idPrestamo);

    public List<AnticipoSalarioListado> ListarAnticipos(string? busqueda = null, string? estado = null) =>
        _metodos.ListarAnticipos(busqueda, estado);

    public AnticipoSalarioListado? ObtenerAnticipo(int idAnticipo) => _metodos.ObtenerAnticipo(idAnticipo);

    public PrestamosAnticiposResumen ObtenerResumen() => _metodos.ObtenerResumen();

    public int? ObtenerPlazoPrestamo(decimal monto) => _metodos.ObtenerPlazoPrestamo(monto);

    public decimal? ObtenerMaximoAnticipo(int idEmpleado, DateOnly fecha) =>
        _metodos.ObtenerMaximoAnticipo(idEmpleado, fecha);

    public int RegistrarPrestamo(int idEmpleado, decimal monto, DateOnly fechaOtorgamiento) =>
        _metodos.RegistrarPrestamo(idEmpleado, monto, fechaOtorgamiento);

    public int RegistrarAnticipo(int idEmpleado, decimal monto, DateOnly fechaAnticipo) =>
        _metodos.RegistrarAnticipo(idEmpleado, monto, fechaAnticipo);
}
