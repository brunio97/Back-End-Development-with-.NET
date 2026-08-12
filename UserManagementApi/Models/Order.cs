using System.ComponentModel.DataAnnotations;

namespace UserManagementApi.Models;

public class Order
{
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string CustomerId { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string ProductCategory { get; set; } = string.Empty;

    [Required]
    [StringLength(30)]
    public string Status { get; set; } = string.Empty;

    public DateTime OrderDate { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal TotalAmount { get; set; }
}
