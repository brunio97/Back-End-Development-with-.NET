using Microsoft.EntityFrameworkCore;
using UserManagementApi.Models;

namespace UserManagementApi.Data;

public static class OrderSeedData
{
    public static async Task InitializeAsync(
        ApplicationDbContext context)
    {
        if (await context.Orders.AnyAsync())
        {
            return;
        }

        string[] statuses =
        {
            "Pending",
            "Processing",
            "Shipped",
            "Delivered"
        };

        string[] categories =
        {
            "Electronics",
            "Clothing",
            "Home",
            "Sports",
            "Books"
        };

        var random = new Random(42);

        var orders = new List<Order>();

        for (int i = 1; i <= 2000; i++)
        {
            orders.Add(
                new Order
                {
                    CustomerId =
                        $"CUST-{random.Next(1, 500):D4}",

                    ProductCategory =
                        categories[
                            random.Next(
                                categories.Length)],

                    Status =
                        statuses[
                            random.Next(
                                statuses.Length)],

                    OrderDate =
                        DateTime.UtcNow
                            .AddDays(
                                -random.Next(
                                    0,
                                    365)),

                    TotalAmount =
                        random.Next(
                            20,
                            5000)
                });
        }

        await context.Orders
            .AddRangeAsync(orders);

        await context.SaveChangesAsync();
    }
}
