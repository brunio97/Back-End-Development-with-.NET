-- WarehouseX Optimized Order Query
--
-- Improvements:
-- 1. Selects only required columns instead of SELECT *
-- 2. Filters data before returning results
-- 3. Uses parameterized values
-- 4. Uses pagination
-- 5. Works with the composite index on
--    Status, ProductCategory, and OrderDate

SELECT
    Id,
    CustomerId,
    ProductCategory,
    Status,
    OrderDate,
    TotalAmount
FROM Orders
WHERE Status = @status
  AND ProductCategory = @category
  AND OrderDate >= @fromDate
  AND OrderDate < @toDate
ORDER BY OrderDate DESC
LIMIT @pageSize
OFFSET @offset;
