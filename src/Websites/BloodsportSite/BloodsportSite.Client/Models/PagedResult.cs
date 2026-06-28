namespace BloodsportSite.Client.Models;

/// <summary>A single page of results plus the totals needed to drive a pager.</summary>
public record PagedResult<T>(
    List<T> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);
