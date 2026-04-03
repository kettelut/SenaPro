using Microsoft.EntityFrameworkCore;
using SenaPro.Domain.Entities;

namespace SenaPro.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Sorteio> Sorteios { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Sorteio>(entity =>
        {
            entity.ToTable("sorteios");

            entity.HasKey(e => e.Id);

            entity.HasIndex(e => e.Concurso)
                .IsUnique();

            entity.Property(e => e.Concurso)
                .IsRequired();

            entity.Property(e => e.Data)
                .IsRequired();

            entity.Property(e => e.Dezena1)
                .IsRequired();

            entity.Property(e => e.Dezena2)
                .IsRequired();

            entity.Property(e => e.Dezena3)
                .IsRequired();

            entity.Property(e => e.Dezena4)
                .IsRequired();

            entity.Property(e => e.Dezena5)
                .IsRequired();

            entity.Property(e => e.Dezena6)
                .IsRequired();

            entity.Property(e => e.PremioSena)
                .HasPrecision(18, 2);

            entity.Property(e => e.GanhadoresSena)
                .HasDefaultValue(0);

            entity.Property(e => e.PremioQuina)
                .HasPrecision(18, 2);

            entity.Property(e => e.GanhadoresQuina)
                .HasDefaultValue(0);

            entity.Property(e => e.PremioQuadra)
                .HasPrecision(18, 2);

            entity.Property(e => e.GanhadoresQuadra)
                .HasDefaultValue(0);

            entity.Property(e => e.Conferido)
                .HasDefaultValue(false);
        });
    }
}