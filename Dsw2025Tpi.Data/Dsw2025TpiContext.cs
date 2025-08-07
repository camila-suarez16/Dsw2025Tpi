using Microsoft.EntityFrameworkCore;
using Dsw2025Tpi.Domain.Entities;

namespace Dsw2025Tpi.Data;

public class Dsw2025TpiContext: DbContext
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Customer> Customers => Set<Customer>();

    public Dsw2025TpiContext(DbContextOptions<Dsw2025TpiContext> options) : base(options)
    {
    }

   
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Product>(eb =>
        {
            eb.ToTable("Products");
            eb.Property(p => p.Sku)
            .HasMaxLength(20)
            .IsRequired();
            eb.HasIndex(p => p.Sku) //sku unico
            .IsUnique();
            eb.Property(p => p.Name)
            .HasMaxLength(60)
            .IsRequired();
            eb.Property(p => p.Description)
            .HasMaxLength(100);
            eb.Property(p => p.CurrentUnitPrice)
            .IsRequired()
            .HasPrecision(15, 2);
            eb.Property(p => p.InternalCode)
            .HasMaxLength(20);
            eb.Property(p => p.StockQuantity)
           .IsRequired();
        });

        modelBuilder.Entity<Order>(eb =>
        {
            eb.ToTable("Orders");
            eb.Property(p => p.BillingAddress)
            .HasMaxLength(60)
            .IsRequired();
            eb.Property(p => p.ShippingAddress)
            .HasMaxLength(60)
            .IsRequired();
            eb.Property(p => p.Notes)
            .HasMaxLength(50);
            eb.Property(p => p.TotalAmount)
            .HasPrecision(15, 2);
            eb.Property(p => p.Status)
            .HasConversion<string>() //enum como string
            .IsRequired(); 
       });
        
        modelBuilder.Entity<Customer>(eb =>
        {
            eb.ToTable("Customers");
            eb.Property(p => p.Name)
            .HasMaxLength(50)
            .IsRequired();
            eb.Property(p => p.PhoneNumber)
            .HasMaxLength(20)
            .IsRequired();
        });

        modelBuilder.Entity<OrderItem>(eb =>
        {
            eb.ToTable("OrderItems");
            eb.Property(p => p.Quantity)
            .IsRequired();
            eb.Property(p => p.UnitPrice)
            .HasPrecision(15, 2);
            eb.Property(p => p.SubTotal)
            .HasPrecision(15, 2);
            //RELACIONES
            eb.HasOne(p => p.Product)
              .WithMany(p => p.OrderItems)
              .HasForeignKey(p => p.ProductId);
            eb.HasOne(p => p.Order)
              .WithMany(p => p.Items)
              .HasForeignKey(p => p.OrderId);
        });

    }
}
