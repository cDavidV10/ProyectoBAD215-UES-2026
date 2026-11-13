using BEMO.Negocio;
using BEMOWeb.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BEMOWeb.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "ADMINISTRADOR")]
public class PlanillaController : PlanillaControllerBase
{
    protected override string ViewPrefix => "~/Areas/Admin/Views/Planilla";

    public PlanillaController(CN_Planilla planillas) : base(planillas)
    {
    }
}
