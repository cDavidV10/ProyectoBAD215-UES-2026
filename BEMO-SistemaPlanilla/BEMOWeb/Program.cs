using BEMO.Negocio;

DotNetEnv.Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

// Configuración para compilar HTML en tiempo de ejecución de forma automática
builder.Services.AddControllersWithViews()
    .AddRazorRuntimeCompilation();

var cadena = builder.Configuration.GetConnectionString("Bemo")
    ?? throw new InvalidOperationException("Falta ConnectionStrings__Bemo en el archivo .env");

builder.Services.AgregarNegocio(cadena);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

// 1. Ruta para que el sistema encuentre las Áreas (Admin y RRHH)
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// 2. Ruta predeterminada modificada para arrancar en el Login
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
