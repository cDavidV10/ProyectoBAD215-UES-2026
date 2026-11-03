using BEMO.Datos;

namespace BEMO.Negocio;

public class ConexionNegocio
{
    private readonly BemoContext _contexto;

    public ConexionNegocio(BemoContext contexto)
    {
        _contexto = contexto;
    }

    public async Task<bool> Probar()
    {
        return await _contexto.Database.CanConnectAsync();
    }
}