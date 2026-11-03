using BEMO.Negocio;

DotNetEnv.Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

var cadena = builder.Configuration.GetConnectionString("Bemo")
    ?? throw new InvalidOperationException("Falta ConnectionStrings__Bemo en el archivo .env");

builder.Services.AddControllersWithViews();
builder.Services.AgregarNegocio(cadena);

var app = builder.Build();

using (var alcance = app.Services.CreateScope())
{
    var conexion = alcance.ServiceProvider.GetRequiredService<ConexionNegocio>();
    if (!await conexion.Probar())
    {
        throw new InvalidOperationException("No se pudo conectar a la base de datos BEMO.");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
