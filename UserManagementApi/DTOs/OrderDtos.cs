using System.ComponentModel.DataAnnotations;

namespace UserManagementApi.DTOs;

public class OrderQueryRequest
{
    public string? Status { get; set; }

    public string? ProductCategory { get; set; }

    public DateTime? From { get; set; }

    public DateTime? To { get; set; }

    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    [Range(1, 100)]
    public int PageSize { get; set; } = 25;
}


public class OrderListItemDto
{
    public int Id { get; set; }

    public string CustomerId { get; set; } = string.Empty;

    public string ProductCategory { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateTime OrderDate { get; set; }

    public decimal TotalAmount { get; set; }
}


public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; set; } =
        Array.Empty<T>();

    public int Page { get; set; }

    public int PageSize { get; set; }

    public int TotalCount { get; set; }
}
