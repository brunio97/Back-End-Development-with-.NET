using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UserManagementApi.Models;

namespace UserManagementApi.Data;

public class ApplicationDbContext
    : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(
        ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Order>(entity =>
        {
            entity.HasIndex(order => new
            {
                order.Status,
                order.ProductCategory,
                order.OrderDate
            })
            .HasDatabaseName(
                "IX_Orders_Status_Category_OrderDate");

            entity.HasIndex(
                order => order.CustomerId)
                .HasDatabaseName(
                    "IX_Orders_CustomerId");
        });
    }
}
