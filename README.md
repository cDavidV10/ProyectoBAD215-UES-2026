# BEMO - Sistema de Planilla

Sistema web para administrar empleados, contratos, planillas, asistencia, capacitaciones, préstamos, parámetros legales, constancias salariales, reportes, usuarios y auditoría.

El proyecto fue desarrollado para la asignatura Base de Datos II y utiliza una arquitectura por capas con ASP.NET Core MVC y SQL Server.

## Funcionalidades principales

- Autenticación mediante usuarios del sistema y autorización por roles.
- Administración de empleados, contratos, cargos, departamentos y tipos de contrato.
- Creación y procesamiento de planillas quincenales.
- Registro de asistencia con autocompletado de días trabajados y no trabajados hasta completar 15 días.
- Cálculo, cierre y reapertura controlada de planillas.
- Gestión de capacitaciones, participantes y resultados.
- Administración de préstamos y anticipos.
- Gestión de parámetros legales, rentas y descuentos.
- Emisión de constancias salariales y reportes de costos.
- Administración de usuarios y cambio de contraseñas por parte del administrador.
- Registro y consulta de auditoría.

### Regla actual de las planillas

Los empleados elegibles se cargan al crear la planilla. Si posteriormente se registra un contrato nuevo, este no modifica automáticamente una planilla que ya existe en estado `BORRADOR`. El botón para actualizar empleados fue retirado del sistema.

## Tecnologías utilizadas

- .NET 10.
- ASP.NET Core MVC.
- C# con nullable reference types e implicit usings habilitados.
- SQL Server.
- Entity Framework Core para el contexto de datos.
- `Microsoft.Data.SqlClient` para consultas y procedimientos almacenados.
- Tailwind CSS 4.
- Node.js y npm para la compilación de estilos.
- Tipografía Inter mediante `@fontsource-variable/inter`.

## Arquitectura

```text
BEMOWeb       Presentación, vistas Razor, autenticación y controladores
    ↓
Negocio       Reglas de negocio y servicios CN_*
    ↓
Datos         Acceso a datos, conexión SQL Server y BemoContext
    ↓
SQL Server    Tablas, vistas, funciones y procedimientos almacenados

Entidades     Modelos compartidos entre las capas
```

### Estructura del proyecto

```text
.
├── BEMO-SistemaPlanilla
│   ├── BEMOWeb        Aplicación web ASP.NET Core MVC
│   ├── Datos          Acceso a datos y conexión a SQL Server
│   ├── Negocio        Servicios y reglas de negocio
│   └── Entidades      Entidades y modelos compartidos
├── input.css          Entrada principal de Tailwind CSS
├── package.json       Dependencias y scripts de frontend
├── package-lock.json  Versiones bloqueadas de npm
└── scripts             Scripts auxiliares del frontend
```

## Módulos del sistema

La aplicación tiene dos áreas principales:

- `Admin`: acceso a todas las funciones administrativas, incluyendo parámetros legales, usuarios, roles y auditoría.
- `RRHH`: acceso a las operaciones de recursos humanos.

Los módulos disponibles son:

1. Inicio.
2. Empleados.
3. Contratos.
4. Planilla.
5. Parámetros legales.
6. Préstamos y anticipos.
7. Capacitaciones.
8. Constancias salariales.
9. Reportes.
10. Usuarios y roles.
11. Auditoría.

## Requisitos

Instale los siguientes componentes antes de ejecutar el proyecto:

- .NET SDK 10.
- Node.js y npm.
- SQL Server accesible desde el equipo donde se ejecuta la aplicación.
- Una base de datos llamada `Sistema_PlanillaCTS` con el esquema, datos iniciales y procedimientos requeridos por el sistema.

Compruebe las instalaciones con:

```powershell
dotnet --version
node --version
npm --version
```

## Configuración de la conexión

La aplicación carga la cadena de conexión desde la variable `ConnectionStrings__Bemo` en un archivo `.env` ubicado en la raíz del proyecto.

Cree o edite `.env` con una cadena similar a una de las siguientes:

```dotenv
# Autenticación de SQL Server
ConnectionStrings__Bemo="Server=localhost,1433;Database=Sistema_PlanillaCTS;User Id=usuario_sql;Password=clave_segura;TrustServerCertificate=True;Connect Timeout=15"
```

Para una instancia local con autenticación de Windows:

```dotenv
ConnectionStrings__Bemo="Server=.;Database=Sistema_PlanillaCTS;Trusted_Connection=True;TrustServerCertificate=True"
```

No incluya credenciales reales en el repositorio. El archivo `.env` está excluido por `.gitignore`.

### Conexión remota

Si SQL Server está en otro equipo, verifique lo siguiente:

1. TCP/IP habilitado en SQL Server.
2. Puerto configurado, normalmente `1433`.
3. Firewall permitiendo la conexión.
4. Usuario SQL con acceso a `Sistema_PlanillaCTS`.
5. `Server` configurado con la IP o nombre correcto.

El usuario de SQL Server utilizado para conectarse a la base de datos no es el mismo usuario que inicia sesión en la interfaz web. La interfaz utiliza registros de la tabla de usuarios del sistema y roles como `ADMINISTRADOR` o `RECURSOS HUMANOS`.

## Instalación

Desde la raíz del proyecto:

```powershell
npm install
dotnet restore .\BEMO-SistemaPlanilla\BEMOWeb\BEMOWeb.csproj
```

Antes del primer uso, asegúrese de que la base de datos tenga el esquema y los procedimientos almacenados correspondientes a esta versión de la aplicación.

## Ejecución

Para iniciar la aplicación con HTTP:

```powershell
dotnet run --project .\BEMO-SistemaPlanilla\BEMOWeb\BEMOWeb.csproj --launch-profile http
```

Para iniciar con HTTPS:

```powershell
dotnet run --project .\BEMO-SistemaPlanilla\BEMOWeb\BEMOWeb.csproj --launch-profile https
```

Direcciones configuradas en desarrollo:

- HTTP: `http://localhost:5002`
- HTTPS: `https://localhost:7112`

El proyecto compila automáticamente los estilos de Tailwind antes de construir la aplicación web.

## Compilación manual de estilos

Si solo necesita regenerar el CSS:

```powershell
npm run build:web
```

Este comando copia las fuentes Inter y genera `BEMO-SistemaPlanilla/BEMOWeb/wwwroot/css/site.css`.

## Verificación del proyecto

Para compilar toda la aplicación:

```powershell
dotnet build .\BEMO-SistemaPlanilla\BEMOWeb\BEMOWeb.csproj
```

Una compilación correcta debe terminar sin errores. La aplicación no contiene actualmente un proyecto de pruebas automatizadas; para la validación funcional se recomienda ejecutar los flujos principales de cada módulo:

- Inicio de sesión con cada rol.
- Creación, captura de asistencia, cálculo y cierre de una planilla.
- Validación de los 15 días de asistencia.
- Registro y finalización de contratos.
- Creación de capacitaciones, asignación de participantes y resultados.
- Cambio de contraseña desde administración.
- Generación de constancias y reportes.
- Consulta de auditoría.

## Seguridad y buenas prácticas

- No subir `.env`, contraseñas ni cadenas de conexión al repositorio.
- Usar contraseñas seguras para usuarios web y usuarios de SQL Server.
- En producción, configurar HTTPS con un certificado válido.
- Limitar los permisos del usuario de SQL Server según las necesidades de la instalación.
- Mantener actualizados el esquema SQL y la aplicación para que los nombres y parámetros de los procedimientos coincidan.
- Respaldar la base de datos antes de aplicar cambios al esquema.

## Solución de problemas

### Falta `ConnectionStrings__Bemo`

Verifique que exista `.env` en la raíz y que contenga exactamente la variable `ConnectionStrings__Bemo`.

### Error 18456

SQL Server rechazó las credenciales. Revise `User Id`, `Password`, autenticación mixta y el estado del login.

### Error 4060

La base de datos no existe, el nombre es incorrecto o el usuario no tiene permisos sobre `Sistema_PlanillaCTS`.

### No se puede ejecutar un procedimiento almacenado

Confirme que el procedimiento exista en la base de datos y que el usuario SQL tenga permiso `EXECUTE`. También revise que la versión del procedimiento coincida con la versión del código.

### Tailwind o npm no se encuentran

Ejecute `npm install` desde la raíz del proyecto y vuelva a ejecutar `npm run build:web`.

## Equipo de desarrollo

- Br. Genesis Nicoll Cañas Escobar - CE24001
- Br. Tito Betuel Portillo Acosta - PA240015
- Br. Carlos David Vasquez Rivas - VR24001

## Licencia

Proyecto académico desarrollado para Base de Datos II, ciclo 2026.
