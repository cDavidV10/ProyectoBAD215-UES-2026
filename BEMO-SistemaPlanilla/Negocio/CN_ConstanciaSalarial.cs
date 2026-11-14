using BEMO.Datos;
using BEMO.Entidades;

namespace BEMO.Negocio;

public class CN_ConstanciaSalarial
{
    private readonly DAOConstanciaSalarial _metodos;

    public CN_ConstanciaSalarial(DAOConstanciaSalarial metodos)
    {
        _metodos = metodos;
    }

    public (List<ConstanciaEmpleadoOpcion> Empleados, int Total) ListarEmpleadosDisponibles(
        int pagina,
        int tamanoPagina,
        string? busqueda = null) =>
        _metodos.ListarEmpleadosDisponibles(pagina, tamanoPagina, busqueda);

    public ConstanciaSalarial? Generar(int idEmpleado, int meses, DateOnly fechaEmision) =>
        _metodos.Generar(idEmpleado, meses, fechaEmision);
}
