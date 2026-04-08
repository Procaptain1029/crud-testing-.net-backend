using Microsoft.EntityFrameworkCore;
using PuestosApi.Models;

namespace PuestosApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Puesto> Puestos => Set<Puesto>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Puesto>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Area).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Nombre).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Nivel).IsRequired().HasConversion<string>().HasMaxLength(50);
            entity.Property(e => e.Modalidad).IsRequired().HasConversion<string>().HasMaxLength(50);
            entity.Property(e => e.Jornada).IsRequired().HasConversion<string>().HasMaxLength(50);
            entity.Property(e => e.SalarioReferencia).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("datetime('now')");

            // Unique index: no two active puestos with same Nombre+Area (case-insensitive handled at app level for SQLite)
            entity.HasIndex(e => new { e.Nombre, e.Area, e.Activo })
                  .HasFilter("[Activo] = 1")
                  .IsUnique()
                  .HasDatabaseName("IX_Puesto_Nombre_Area_Activo");
        });
    }
}
