using System.Collections.Generic;

namespace Common.Models;

public class PaginationInfo<T>
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int ItemsCount { get; set; }
    public int PageCount { get; set; }
    public List<T> Items { get; init; }
}
