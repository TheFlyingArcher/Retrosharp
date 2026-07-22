namespace Retrosharp.UI.Api.Models
{
    /// <summary>
    /// Response envelope for paginated list/search endpoints. See spec/api.md, "Pagination".
    /// </summary>
    public class PagedResult<T>
    {
        public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();

        public int TotalCount { get; set; }

        public int Limit { get; set; }

        public int Offset { get; set; }
    }
}
