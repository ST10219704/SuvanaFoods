using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace SuvanaFoods.Models;

public partial class SuvanaFoodsDbContext : DbContext
{
    public SuvanaFoodsDbContext()
    {
    }

    public SuvanaFoodsDbContext(DbContextOptions<SuvanaFoodsDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Admin> Admins { get; set; }

    public virtual DbSet<Cart> Carts { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Contact> Contacts { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<FoodItem> FoodItems { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=DESKTOP-JP19TVO; Initial Catalog=SuvanaFoodsDB; Encrypt=False; Integrated Security=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Admin>(entity =>
        {
            entity.HasKey(e => e.AdminId).HasName("PK__Admin__719FE4E8441D08B0");

            entity.ToTable("Admin");

            entity.HasIndex(e => e.Username, "UQ__Admin__536C85E4F77DD382").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__Admin__A9D105345D0B774B").IsUnique();

            entity.Property(e => e.AdminId).HasColumnName("AdminID");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Password)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Role)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Username)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Cart>(entity =>
        {
            entity.HasKey(e => e.CartId).HasName("PK__Cart__51BCD7B72C8600EA");

            entity.ToTable("Cart");

            entity.HasOne(d => d.Customer).WithMany(p => p.Carts)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Cart__CustomerId__5CD6CB2B");

            entity.HasOne(d => d.FoodItem).WithMany(p => p.Carts)
                .HasForeignKey(d => d.FoodItemId)
                .HasConstraintName("FK__Cart__FoodItemId__5DCAEF64");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__Category__19093A0B7ED0AA78");

            entity.ToTable("Category");

            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Contact>(entity =>
        {
            entity.HasKey(e => e.ContactId).HasName("PK__Contact__5C66259B63AFBE73");

            entity.ToTable("Contact");

            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Message).IsUnicode(false);
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Subject)
                .HasMaxLength(200)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.CustomerId).HasName("PK__Customer__A4AE64B81B40F029");

            entity.ToTable("Customer");

            entity.HasIndex(e => e.Username, "UQ__Customer__536C85E4F12A4466").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__Customer__A9D105348BF19A2B").IsUnique();

            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
            entity.Property(e => e.Address).IsUnicode(false);
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ImageUrl).IsUnicode(false);
            entity.Property(e => e.Mobile)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Password)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Username)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<FoodItem>(entity =>
        {
            entity.HasKey(e => e.FoodItemId).HasName("PK__FoodItem__464DC8128A226572");

            entity.ToTable("FoodItem");

            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.Property(e => e.Description)
                .IsUnicode(false);

            entity.Property(e => e.Category)
                .IsUnicode(false);

            entity.Property(e => e.Price)
                .HasColumnType("decimal(18, 2)");

            entity.Property(e => e.Quantity) // Add Quantity configuration
                .IsRequired(); // Or specify .HasDefaultValue(0) if applicable

            entity.Property(e => e.ImageUrl)
                .IsUnicode(false);

            entity.Property(e => e.IsActive) // Add IsActive configuration
                .IsRequired(); // Or specify .HasDefaultValue(true) if applicable
        });


        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PK__Order__C3905BCF322957C7");

            entity.ToTable("Order");

            entity.Property(e => e.OrderDate).HasColumnType("datetime");
            entity.Property(e => e.OrderNo).IsUnicode(false);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.Customer).WithMany(p => p.Orders)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("FK__Order__CustomerI__619B8048");

            entity.HasOne(d => d.FoodItem).WithMany(p => p.Orders)
                .HasForeignKey(d => d.FoodItemId)
                .HasConstraintName("FK__Order__FoodItemI__60A75C0F");

            entity.HasOne(d => d.Payment).WithMany(p => p.Orders)
                .HasForeignKey(d => d.PaymentId)
                .HasConstraintName("FK__Order__PaymentId__628FA481");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("PK__Payment__9B556A38E08C3581");

            entity.ToTable("Payment");

            entity.Property(e => e.Address).IsUnicode(false);
            entity.Property(e => e.CardNo)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ExpiryDate)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.PaymentMode)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
