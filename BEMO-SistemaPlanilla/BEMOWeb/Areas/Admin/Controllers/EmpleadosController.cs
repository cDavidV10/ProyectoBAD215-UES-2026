using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BEMO.Entidades;
using BEMO.Negocio;
using Negocio;
using Microsoft.Data.SqlClient;

namespace BEMOWeb.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "ADMINISTRADOR")]
public class EmpleadosController : Controller
{
     private readonly CN_Empleado _empleados;
     private readonly CN_Departamento _departamento;
     private readonly CN_DeptoUbicacion _departamentosUbicacion;
     private readonly CN_MunicipioUbicacion _municipiosUbicacion;
     private readonly CN_Afp _afps;
     private readonly CN_Cargo _cargos;
     private readonly CN_TipoContrato _tiposContrato;

        public EmpleadosController(
            CN_Empleado empleados,
            CN_Departamento departamento,
            CN_DeptoUbicacion departamentosUbicacion,
            CN_MunicipioUbicacion municipiosUbicacion,
            CN_Afp afps,
            CN_Cargo cargos,
            CN_TipoContrato tiposContrato)
        {
            _empleados = empleados;
            _departamento = departamento;
            _departamentosUbicacion = departamentosUbicacion;
            _municipiosUbicacion = municipiosUbicacion;
            _afps = afps;
            _cargos = cargos;
            _tiposContrato = tiposContrato;
        }
    public IActionResult Index(int pagina = 1, string? busqueda = null, string? departamento = null, string? estado = null)
        {
            const int tamanoPagina = 5;
            pagina = Math.Max(1, pagina);
            var resumen = _empleados.ObtenerResumen();
            var resultado = _empleados.ListarPaginado(pagina, tamanoPagina, busqueda, departamento, estado, resumen.Total);
            var totalRegistros = resultado.Total;
            var totalPaginas = Math.Max(1, (int)Math.Ceiling(totalRegistros / (double)tamanoPagina));
            if (pagina > totalPaginas)
            {
                pagina = totalPaginas;
                resultado = _empleados.ListarPaginado(pagina, tamanoPagina, busqueda, departamento, estado, resumen.Total);
            }

            var empleadoModel = new Models.EmpleadoModel
            {
                EmpleadosActivos = resumen.Activos,
                EmpleadosInactivos = resumen.Inactivos,
                EmpleadosTotales = resumen.Total,
                Departamentos = _departamento.Listar(),
                ListaEmpleados = resultado.Empleados,
                PaginaActual = pagina,
                TotalPaginas = totalPaginas,
                TotalRegistros = totalRegistros,
                TamanoPagina = tamanoPagina,
                DepartamentoSeleccionado = departamento,
                EstadoSeleccionado = estado,
                Busqueda = busqueda
            };

            return View(empleadoModel);
        }

    [HttpGet]
    public IActionResult Editar(int id)
    {
        var empleado = _empleados.ObtenerPorId(id);
        if (empleado is null)
        {
            return NotFound();
        }

        return View("~/Areas/Admin/Views/Empleados/Editar.cshtml", CrearModeloEdicion(empleado));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Editar(Models.EditarEmpleadoModel modelo)
    {
        CargarCatalogosEdicion(modelo);
        ValidarEdicion(modelo);
        RestaurarDatosProtegidos(modelo);

        if (!ModelState.IsValid)
        {
            return View("~/Areas/Admin/Views/Empleados/Editar.cshtml", modelo);
        }

        try
        {
            _empleados.ActualizarDatosPersonales(
                modelo.IdEmpleado,
                modelo.Nombre,
                modelo.Apellido,
                modelo.Nit,
                modelo.NumeroIsss,
                modelo.Telefono,
                modelo.Correo,
                modelo.IdAfp,
                modelo.IdMunicipio,
                modelo.NombreCalle,
                modelo.NumeroCasa,
                modelo.Estado);

            TempData["Mensaje"] = "La información del empleado fue actualizada correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (SqlException ex)
        {
            AgregarMensajeDeError(ex);
            return View("~/Areas/Admin/Views/Empleados/Editar.cshtml", modelo);
        }
    }

    [HttpGet]
    public IActionResult Nuevo()
    {
        var modelo = new Models.NuevoEmpleadoModel
        {
            DepartamentosUbicacion = _departamentosUbicacion.listar(),
            Municipios = _municipiosUbicacion.Listar(),
            Afps = _afps.Listar(),
            Departamentos = _departamento.Listar(),
            Cargos = _cargos.Listar(),
            TiposContrato = _tiposContrato.Listar()
        };

        return View("~/Areas/Admin/Views/Empleados/Nuevo.cshtml", modelo);
    }

    private Models.EditarEmpleadoModel CrearModeloEdicion(Empleado empleado)
    {
        var departamentosUbicacion = _departamentosUbicacion.listar();
        var municipios = _municipiosUbicacion.Listar();
        var afps = _afps.Listar();
        var nombreDepartamentoUbicacion = empleado.Direccion?.Distrito?.DepartamentoUbicacion?.NombreDepartamentoUbicacion;
        var departamentoUbicacion = departamentosUbicacion.FirstOrDefault(departamento =>
            string.Equals(departamento.NombreDepartamentoUbicacion, nombreDepartamentoUbicacion, StringComparison.OrdinalIgnoreCase));
        var municipio = municipios.FirstOrDefault(item =>
            item.IdDepartamentoUbicacion == departamentoUbicacion?.IdDepartamentoUbicacion
            && string.Equals(item.NombreMunicipio, empleado.Direccion?.Distrito?.NombreDistrito, StringComparison.OrdinalIgnoreCase));
        var afp = afps.FirstOrDefault(item =>
            string.Equals(item.NombreAfp, empleado.Afp?.NombreAfp, StringComparison.OrdinalIgnoreCase));

        return new Models.EditarEmpleadoModel
        {
            IdEmpleado = empleado.IdEmpleado,
            Dui = empleado.Dui,
            FechaNac = empleado.FechaNac,
            Nombre = empleado.Nombre,
            Apellido = empleado.Apellido,
            Nit = empleado.Nit,
            NumeroIsss = empleado.NumeroIsss,
            Telefono = empleado.Telefono,
            Correo = empleado.Correo,
            IdAfp = afp?.IdAfp ?? 0,
            IdDepartamentoUbicacion = departamentoUbicacion?.IdDepartamentoUbicacion ?? 0,
            IdMunicipio = municipio?.IdMunicipio ?? 0,
            NombreCalle = empleado.Direccion?.NombreCalle ?? string.Empty,
            NumeroCasa = empleado.Direccion?.NumeroCasa ?? string.Empty,
            Estado = empleado.Estado,
            DepartamentosUbicacion = departamentosUbicacion,
            Municipios = municipios,
            Afps = afps
        };
    }

    private void CargarCatalogosEdicion(Models.EditarEmpleadoModel modelo)
    {
        modelo.DepartamentosUbicacion = _departamentosUbicacion.listar();
        modelo.Municipios = _municipiosUbicacion.Listar();
        modelo.Afps = _afps.Listar();
    }

    private void RestaurarDatosProtegidos(Models.EditarEmpleadoModel modelo)
    {
        if (modelo.IdEmpleado <= 0)
        {
            return;
        }

        var empleado = _empleados.ObtenerPorId(modelo.IdEmpleado);
        if (empleado is not null)
        {
            modelo.Dui = empleado.Dui;
            modelo.FechaNac = empleado.FechaNac;
            modelo.NumeroIsss = empleado.NumeroIsss;
        }
    }

    private void ValidarEdicion(Models.EditarEmpleadoModel modelo)
    {
        if (modelo.IdAfp > 0 && !modelo.Afps.Any(afp => afp.IdAfp == modelo.IdAfp))
        {
            ModelState.AddModelError(nameof(modelo.IdAfp), "La AFP seleccionada no es válida.");
        }

        if (modelo.IdDepartamentoUbicacion > 0
            && !modelo.DepartamentosUbicacion.Any(departamento => departamento.IdDepartamentoUbicacion == modelo.IdDepartamentoUbicacion))
        {
            ModelState.AddModelError(nameof(modelo.IdDepartamentoUbicacion), "El departamento de ubicación no es válido.");
        }

        if (modelo.IdMunicipio > 0
            && !modelo.Municipios.Any(municipio =>
                municipio.IdMunicipio == modelo.IdMunicipio
                && municipio.IdDepartamentoUbicacion == modelo.IdDepartamentoUbicacion))
        {
            ModelState.AddModelError(nameof(modelo.IdMunicipio), "El municipio no corresponde al departamento seleccionado.");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Nuevo(Models.NuevoEmpleadoModel modelo)
    {
        CargarCatalogos(modelo);
        LimpiarErroresDeNavegacion();
        ValidarRegistro(modelo);
        TraducirErroresDeModelo();
        if (!ModelState.IsValid)
        {
            return View("~/Areas/Admin/Views/Empleados/Nuevo.cshtml", modelo);
        }

        try
        {
            var idEmpleado = _empleados.Registrar(
                modelo.Empleado,
                modelo.IdMunicipio!.Value,
                modelo.NombreCalle,
                modelo.NumeroCasa,
                modelo.IdCargo!.Value,
                modelo.IdDepartamento!.Value,
                modelo.IdTipoContrato!.Value,
                modelo.SalarioBruto!.Value,
                modelo.FechaContratacion!.Value,
                modelo.FechaCulminacion);

            TempData["Mensaje"] = $"El empleado fue registrado correctamente con ID {idEmpleado}.";
            return RedirectToAction(nameof(Index));
        }
        catch (SqlException ex)
        {
            AgregarMensajeDeError(ex);
            return View("~/Areas/Admin/Views/Empleados/Nuevo.cshtml", CargarCatalogos(modelo));
        }
    }

    private void AgregarMensajeDeError(SqlException ex)
    {
        var mensaje = ex.Number switch
        {
            51001 => "Ya existe un empleado registrado con ese DUI.",
            51002 => "Ya existe un empleado registrado con ese número de ISSS.",
            51003 => "Ya existe un empleado registrado con ese NIT.",
            51004 => "La AFP seleccionada no está disponible.",
            51005 => "El municipio seleccionado no está disponible.",
            51006 => "El departamento laboral seleccionado no está disponible.",
            51007 => "El tipo de contrato seleccionado no está disponible.",
            51008 => "El cargo seleccionado no está disponible.",
            51009 => "El salario ingresado no está dentro del rango permitido para el cargo.",
            51010 => "La fecha de culminación no puede ser anterior a la fecha de contratación.",
            51011 => "La fecha de culminación debe ser posterior a la fecha actual.",
            _ when ex.Message.Contains("PK_Empleado", StringComparison.OrdinalIgnoreCase)
                => "No se pudo generar el identificador del empleado porque ya existe. Es necesario revisar la secuencia de empleados en la base de datos.",
            _ when ex.Message.Contains("PK_Direccion", StringComparison.OrdinalIgnoreCase)
                => "No se pudo generar el identificador de la dirección porque ya existe. Es necesario revisar la secuencia de direcciones en la base de datos.",
            _ when ex.Message.Contains("UQ_Empleado_Nit", StringComparison.OrdinalIgnoreCase)
                => "Ya existe un empleado registrado con ese NIT.",
            _ when ex.Message.Contains("CK_Empleado_Nit", StringComparison.OrdinalIgnoreCase)
                => "El NIT ingresado no es compatible con la configuración actual de la base de datos. Verifica el formato o actualiza la restricción del NIT.",
            _ when ex.Message.Contains("UQ_Empleado_Dui", StringComparison.OrdinalIgnoreCase)
                => "Ya existe un empleado registrado con ese DUI.",
            _ when ex.Message.Contains("UQ_Empleado_Isss", StringComparison.OrdinalIgnoreCase)
                => "Ya existe un empleado registrado con ese número de ISSS.",
            _ when ex.Number is 2627 or 2601
                => "No se pudo guardar el empleado porque uno de los datos ya está registrado.",
            _ when ex.Number is 8152 or 2628
                => "Uno de los datos ingresados supera la longitud permitida.",
            _ when ex.Number == 515
                => "Falta un dato obligatorio para completar el registro.",
            _ when ex.Number == 547
                => "Uno de los datos seleccionados ya no está disponible en la base de datos.",
            _ when ex.Number == 2812
                => "El procedimiento de registro no está disponible en la base de datos.",
            _ when ex.Number is 201 or 8145
                => "El procedimiento de registro de la base de datos no coincide con la versión de la aplicación. Debe actualizarse el procedimiento usp_Registrar_Empleado.",
            _ => "No se pudo guardar el empleado. Verifica la información ingresada e inténtalo nuevamente."
        };

        ModelState.AddModelError(string.Empty, mensaje);
    }

    private void TraducirErroresDeModelo()
    {
        foreach (var item in ModelState)
        {
            var mensajes = item.Value.Errors
                .Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage)
                    ? MensajeParaCampo(item.Key)
                    : error.ErrorMessage)
                .ToList();

            if (mensajes.Any(string.IsNullOrWhiteSpace))
            {
                item.Value.Errors.Clear();
                foreach (var mensaje in mensajes)
                {
                    item.Value.Errors.Add(mensaje);
                }
            }
        }
    }

    private void LimpiarErroresDeNavegacion()
    {
        var claves = ModelState.Keys
            .Where(clave => clave.Equals("Empleado.Afp", StringComparison.OrdinalIgnoreCase)
                || clave.StartsWith("Empleado.Afp.", StringComparison.OrdinalIgnoreCase)
                || clave.Equals("Empleado.Direccion", StringComparison.OrdinalIgnoreCase)
                || clave.StartsWith("Empleado.Direccion.", StringComparison.OrdinalIgnoreCase)
                || clave.Equals("Empleado.Usuario", StringComparison.OrdinalIgnoreCase)
                || clave.StartsWith("Empleado.Usuario.", StringComparison.OrdinalIgnoreCase)
                || clave.StartsWith("Empleado.Contratos", StringComparison.OrdinalIgnoreCase)
                || clave.StartsWith("Empleado.Prestamos", StringComparison.OrdinalIgnoreCase)
                || clave.StartsWith("Empleado.AnticiposSalario", StringComparison.OrdinalIgnoreCase)
                || clave.StartsWith("Empleado.DetallesCapacitacion", StringComparison.OrdinalIgnoreCase))
            .ToList();

        foreach (var clave in claves)
        {
            ModelState.Remove(clave);
        }
    }

    private static string MensajeParaCampo(string campo)
    {
        return campo switch
        {
            "Empleado.FechaNac" => "Ingrese una fecha de nacimiento válida.",
            "FechaContratacion" => "Ingrese una fecha de contratación válida.",
            "FechaCulminacion" => "Ingrese una fecha de culminación válida.",
            "SalarioBruto" => "Ingrese un salario válido usando solo números.",
            "IdDepartamentoUbicacion" => "Seleccione un departamento de ubicación.",
            "IdMunicipio" => "Seleccione un municipio.",
            "IdAfp" or "Empleado.IdAfp" => "Seleccione una AFP.",
            "IdCargo" => "Seleccione un cargo.",
            "IdDepartamento" => "Seleccione un departamento laboral.",
            "IdTipoContrato" => "Seleccione un tipo de contrato.",
            _ => "El valor ingresado no es válido."
        };
    }

    private Models.NuevoEmpleadoModel CargarCatalogos(Models.NuevoEmpleadoModel modelo)
    {
        modelo.DepartamentosUbicacion = _departamentosUbicacion.listar();
        modelo.Municipios = _municipiosUbicacion.Listar();
        modelo.Afps = _afps.Listar();
        modelo.Departamentos = _departamento.Listar();
        modelo.Cargos = _cargos.Listar();
        modelo.TiposContrato = _tiposContrato.Listar();
        return modelo;
    }

    private void ValidarRegistro(Models.NuevoEmpleadoModel modelo)
    {
        var fechaHoy = DateOnly.FromDateTime(DateTime.Today);
        var fechaMinimaNacimiento = DateOnly.FromDateTime(DateTime.Today).AddYears(-18);

        if (modelo.Empleado.FechaNac == DateOnly.MinValue)
        {
            ModelState.AddModelError("Empleado.FechaNac", "La fecha de nacimiento es obligatoria.");
        }
        else if (modelo.Empleado.FechaNac > DateOnly.FromDateTime(DateTime.Today))
        {
            ModelState.AddModelError("Empleado.FechaNac", "La fecha de nacimiento no puede estar en el futuro.");
        }
        else if (modelo.Empleado.FechaNac > fechaMinimaNacimiento)
        {
            ModelState.AddModelError("Empleado.FechaNac", "El empleado debe tener al menos 18 años.");
        }

        if (modelo.Empleado.IdAfp <= 0)
        {
            ModelState.AddModelError("Empleado.IdAfp", "Seleccione una AFP.");
        }
        else if (!modelo.Afps.Any(a => a.IdAfp == modelo.Empleado.IdAfp))
        {
            ModelState.AddModelError("Empleado.IdAfp", "La AFP seleccionada no es válida.");
        }

        if (!modelo.IdDepartamentoUbicacion.HasValue)
        {
            ModelState.AddModelError(nameof(modelo.IdDepartamentoUbicacion), "Seleccione el departamento de ubicación.");
        }
        else if (!modelo.DepartamentosUbicacion.Any(d => d.IdDepartamentoUbicacion == modelo.IdDepartamentoUbicacion.Value))
        {
            ModelState.AddModelError(nameof(modelo.IdDepartamentoUbicacion), "El departamento de ubicación no es válido.");
        }

        if (!modelo.IdMunicipio.HasValue)
        {
            ModelState.AddModelError(nameof(modelo.IdMunicipio), "Seleccione el municipio.");
        }
        else if (!modelo.Municipios.Any(m => m.IdMunicipio == modelo.IdMunicipio.Value && m.IdDepartamentoUbicacion == modelo.IdDepartamentoUbicacion))
        {
            ModelState.AddModelError(nameof(modelo.IdMunicipio), "El municipio no corresponde al departamento seleccionado.");
        }

        if (!modelo.IdCargo.HasValue)
        {
            ModelState.AddModelError(nameof(modelo.IdCargo), "Seleccione el cargo.");
        }
        else if (!modelo.Cargos.Any(c => c.IdCargo == modelo.IdCargo.Value))
        {
            ModelState.AddModelError(nameof(modelo.IdCargo), "El cargo no es válido.");
        }

        if (!modelo.IdDepartamento.HasValue)
        {
            ModelState.AddModelError(nameof(modelo.IdDepartamento), "Seleccione el departamento laboral.");
        }
        else if (!modelo.Departamentos.Any(d => d.IdDepartamento == modelo.IdDepartamento.Value))
        {
            ModelState.AddModelError(nameof(modelo.IdDepartamento), "El departamento laboral no es válido.");
        }

        if (!modelo.IdTipoContrato.HasValue)
        {
            ModelState.AddModelError(nameof(modelo.IdTipoContrato), "Seleccione el tipo de contrato.");
        }
        else if (!modelo.TiposContrato.Any(t => t.IdTipoContrato == modelo.IdTipoContrato.Value))
        {
            ModelState.AddModelError(nameof(modelo.IdTipoContrato), "El tipo de contrato no es válido.");
        }

        if (!modelo.SalarioBruto.HasValue || modelo.SalarioBruto.Value <= 0)
        {
            ModelState.AddModelError(nameof(modelo.SalarioBruto), "Ingrese un salario bruto válido.");
        }

        if (!modelo.FechaContratacion.HasValue)
        {
            ModelState.AddModelError(nameof(modelo.FechaContratacion), "La fecha de contratación es obligatoria.");
        }

        if (modelo.FechaCulminacion.HasValue && modelo.FechaCulminacion.Value <= fechaHoy)
        {
            ModelState.AddModelError(nameof(modelo.FechaCulminacion), "La fecha de culminación debe ser posterior a la fecha actual.");
        }
        else if (modelo.FechaCulminacion.HasValue && modelo.FechaContratacion.HasValue && modelo.FechaCulminacion < modelo.FechaContratacion)
        {
            ModelState.AddModelError(nameof(modelo.FechaCulminacion), "La fecha de culminación no puede ser anterior a la contratación.");
        }
    }
}
