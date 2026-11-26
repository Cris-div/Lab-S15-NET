using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Veterinaria.Entidades.Models;

namespace Veterinaria.Datos.Models;

public partial class VeterinariaContext : DbContext
{
    public VeterinariaContext(DbContextOptions<VeterinariaContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Mascotas> Mascotas { get; set; }

    public virtual DbSet<Propietarios> Propietarios { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Mascotas>(entity =>
        {
            entity.HasIndex(e => e.PropietarioId, "IX_Mascotas_PropietarioId");

            entity.Property(e => e.Especie).HasMaxLength(40);
            entity.Property(e => e.Nombre).HasMaxLength(80);
            entity.Property(e => e.Peso).HasColumnType("decimal(7, 2)");

            entity.HasOne(d => d.Propietario).WithMany(p => p.Mascotas)
                .HasForeignKey(d => d.PropietarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Mascotas_Propietarios");
        });

        modelBuilder.Entity<Propietarios>(entity =>
        {
            entity.HasIndex(e => e.Documento, "UQ_Propietarios_Documento").IsUnique();

            entity.Property(e => e.Documento)
                .HasMaxLength(8)
                .IsUnicode(false);
            entity.Property(e => e.Nombre).HasMaxLength(100);
            entity.Property(e => e.Telefono)
                .HasMaxLength(15)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("Usuarios");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Correo).IsUnique();
            entity.Property(e => e.Correo).HasMaxLength(254).IsUnicode(false).IsRequired();
            entity.Property(e => e.ClaveHash).HasMaxLength(100).IsUnicode(false).IsRequired();
            entity.Property(e => e.Rol).HasMaxLength(30).IsUnicode(false).IsRequired();
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
