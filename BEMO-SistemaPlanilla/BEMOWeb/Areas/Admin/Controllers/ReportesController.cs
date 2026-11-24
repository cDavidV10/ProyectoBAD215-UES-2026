using BEMO.Negocio;
using BEMOWeb.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BEMOWeb.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "ADMINISTRADOR")]
public class ReportesController : ReportesControllerBase
{
    protected override string ViewPrefix => "~/Areas/Admin/Views/Reportes";

    public ReportesController(
        CN_ReporteCostos reportes,
        global::Negocio.CN_Departamento departamentos)
        : base(reportes, departamentos)
    {
    }
}
