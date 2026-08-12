using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using UserManagementApi.Data;
using UserManagementApi.DTOs;

namespace UserManagementApi.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IMemoryCache _cache;

    public OrdersController(
        ApplicationDbContext context,
        IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }


    [HttpGet]
    public async Task<IActionResult> GetOrders(
        [FromQuery] OrderQueryRequest request,
        CancellationToken cancellationToken)
    {
        if (request.From.HasValue &&
            request.To.HasValue &&
            request.From > request.To)
        {
            return BadRequest(new
            {
                message =
                    "The From date cannot be later than the To date."
            });
        }

        string status =
            request.Status?.Trim() ?? string.Empty;

        string category =
            request.ProductCategory?.Trim()
            ?? string.Empty;

        string cacheKey =
            $"orders:" +
            $"{status}:" +
            $"{category}:" +
            $"{request.From:O}:" +
            $"{request.To:O}:" +
            $"{request.Page}:" +
            $"{request.PageSize}";


        if (_cache.TryGetValue(
                cacheKey,
                out PagedResult<OrderListItemDto>? cachedResult))
        {
            return Ok(cachedResult);
        }


        var query =
            _context.Orders
                .AsNoTracking()
                .AsQueryable();


        if (!string.IsNullOrWhiteSpace(status))
        {
            query =
                query.Where(
                    order =>
                        order.Status == status);
        }


        if (!string.IsNullOrWhiteSpace(category))
        {
            query =
                query.Where(
                    order =>
                        order.ProductCategory ==
                        category);
        }


        if (request.From.HasValue)
        {
            query =
                query.Where(
                    order =>
                        order.OrderDate >=
                        request.From.Value);
        }


        if (request.To.HasValue)
        {
            query =
                query.Where(
                    order =>
                        order.OrderDate <
                        request.To.Value);
        }


        int totalCount =
            await query.CountAsync(
                cancellationToken);


        var items =
            await query
                .OrderByDescending(
                    order => order.OrderDate)

                .Skip(
                    (request.Page - 1) *
                    request.PageSize)

                .Take(request.PageSize)

                .Select(order =>
                    new OrderListItemDto
                    {
                        Id = order.Id,

                        CustomerId =
                            order.CustomerId,

                        ProductCategory =
                            order.ProductCategory,

                        Status =
                            order.Status,

                        OrderDate =
                            order.OrderDate,

                        TotalAmount =
                            order.TotalAmount
                    })

                .ToListAsync(
                    cancellationToken);


        var result =
            new PagedResult<OrderListItemDto>
            {
                Items = items,

                Page =
                    request.Page,

                PageSize =
                    request.PageSize,

                TotalCount =
                    totalCount
            };


        _cache.Set(
            cacheKey,
            result,
            TimeSpan.FromMinutes(2));


        return Ok(result);
    }


    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetOrderById(
        int id,
        CancellationToken cancellationToken)
    {
        var order =
            await _context.Orders
                .AsNoTracking()

                .Where(
                    order =>
                        order.Id == id)

                .Select(order =>
                    new OrderListItemDto
                    {
                        Id = order.Id,

                        CustomerId =
                            order.CustomerId,

                        ProductCategory =
                            order.ProductCategory,

                        Status =
                            order.Status,

                        OrderDate =
                            order.OrderDate,

                        TotalAmount =
                            order.TotalAmount
                    })

                .FirstOrDefaultAsync(
                    cancellationToken);


        if (order == null)
        {
            return NotFound(new
            {
                message =
                    $"Order {id} was not found."
            });
        }


        return Ok(order);
    }
}
