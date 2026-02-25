namespace S2O1.Business.DTOs.Common
{
    public class PagedResultDto<T>
    {
        public System.Collections.Generic.IEnumerable<T> Items { get; set; } = new System.Collections.Generic.List<T>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)System.Math.Ceiling((double)TotalCount / PageSize);
    }
}
