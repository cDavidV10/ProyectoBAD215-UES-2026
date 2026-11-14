using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BEMO.Negocio;
using BEMOWeb.Controllers;
using BEMOWeb.Models;

namespace BEMOWeb.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "ADMINISTRADOR")]
public class CapacitacionesController : CapacitacionesControllerBase
{
    public CapacitacionesController(CN_Capacitacion capacitaciones, global::Negocio.CN_Departamento departamentos)
        : base(capacitaciones, departamentos)
    {
    }

    [HttpGet]
    public IActionResult Index(int pagina = 1, string? busqueda = null, int? tipo = null, string? estado = null) =>
        IndexCore(pagina, busqueda, tipo, estado);

    [HttpGet]
    public IActionResult Nueva() => NuevaGetCore();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Nueva(NuevaCapacitacionModel modelo) => NuevaPostCore(modelo);

    [HttpGet]
    public IActionResult Participantes(
        int id,
        string? busqueda = null,
        string? departamento = null,
        int pagina = 1,
        string? seleccionados = null) =>
        ParticipantesGetCore(id, busqueda, departamento, pagina, seleccionados);

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Participantes(CapacitacionParticipantesModel modelo) => ParticipantesPostCore(modelo);

    [HttpGet]
    public IActionResult Detalle(int id, int pagina = 1) => DetalleCore(id, pagina);

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ActualizarResultados(CapacitacionResultadosModel modelo) =>
        ActualizarResultadosCore(modelo);
}
