using BEMO.Entidades;
using Microsoft.EntityFrameworkCore;

namespace BEMO.Datos;

public class BemoContext : DbContext
{
    public BemoContext(DbContextOptions<BemoContext> opciones) : base(opciones)
    {
    }

    public DbSet<TipoContrato> TiposContrato { get; set; }
    public DbSet<TipoCapacitacion> TiposCapacitacion { get; set; }
    public DbSet<TipoDescuento> TiposDescuento { get; set; }
    public DbSet<TipoRenta> TiposRenta { get; set; }
    public DbSet<Departamento> Departamentos { get; set; }
    public DbSet<DepartamentoUbicacion> DepartamentosUbicacion { get; set; }
    public DbSet<Cargo> Cargos { get; set; }
    public DbSet<MunicipioUbicacion> MunicipiosUbicacion { get; set; }
    public DbSet<Empleado> Empleados { get; set; }
    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Contrato> Contratos { get; set; }
    public DbSet<AnticipoSalario> AnticiposSalario { get; set; }
    public DbSet<Planilla> Planillas { get; set; }
    public DbSet<Renta> Rentas { get; set; }
    public DbSet<DetallePlanilla> DetallesPlanilla { get; set; }
    public DbSet<Prestamo> Prestamos { get; set; }
    public DbSet<DescuentoLey> DescuentosLey { get; set; }
    public DbSet<Capacitacion> Capacitaciones { get; set; }
    public DbSet<DetalleCapacitacion> DetallesCapacitacion { get; set; }
    public DbSet<Auditoria> Auditorias { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cargo>()
            .HasOne(c => c.CargoJefe)
            .WithMany(c => c.Subordinados)
            .HasForeignKey(c => c.IdCargoJefe)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
