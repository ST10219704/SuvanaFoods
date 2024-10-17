using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace AgriEnergyConnect.Models;

// DbContext for the database 
// This was adapted from C# Corner
// Link: https://www.c-sharpcorner.com/blogs/asp-net-mvc-5-using-dbcontext-connect-database
// Author: Azdev Xxi 
public partial class AgriEnergyConnectDbContext : DbContext
{
    public AgriEnergyConnectDbContext()
    {
    }

    public AgriEnergyConnectDbContext(DbContextOptions<AgriEnergyConnectDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<Farmer> Farmers { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=DESKTOP-JP19TVO; Initial Catalog=AgriEnergyConnectDB; Encrypt=False; Integrated Security=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.EmpId).HasName("PK__Employee__2623598BE67E49A6");

            entity.ToTable("Employee");

            entity.Property(e => e.EmpId).HasColumnName("Emp_ID");
            entity.Property(e => e.EmpPassword)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("Emp_Password");
            entity.Property(e => e.EmpUsername)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("Emp_Username");
        });

        modelBuilder.Entity<Farmer>(entity =>
        {
            entity.HasKey(e => e.FarId).HasName("PK__Farmer__11E66CCF6B64DCA3");

            entity.ToTable("Farmer");

            entity.Property(e => e.FarId).HasColumnName("Far_ID");
            entity.Property(e => e.FarEmail)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("Far_Email");
            entity.Property(e => e.FarLocation)
                .HasMaxLength(250)
                .IsUnicode(false)
                .HasColumnName("Far_Location");
            entity.Property(e => e.FarName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("Far_Name");
            entity.Property(e => e.FarPassword)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("Far_Password");
            entity.Property(e => e.FarPhone)
                .HasMaxLength(12)
                .IsUnicode(false)
                .HasColumnName("Far_Phone");
            entity.Property(e => e.FarUsername)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("Far_Username");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.ProId).HasName("PK__Product__04BA0DF5036F96E0");

            entity.ToTable("Product");

            entity.Property(e => e.ProId).HasColumnName("Pro_ID");
            entity.Property(e => e.FarId).HasColumnName("Far_ID");
            entity.Property(e => e.ProCategory)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("Pro_Category");
            entity.Property(e => e.ProName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("Pro_Name");
            entity.Property(e => e.ProProductionDate).HasColumnName("Pro_ProductionDate");

            entity.HasOne(d => d.Far).WithMany(p => p.Products)
                .HasForeignKey(d => d.FarId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Product__Far_ID__3B75D760");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
