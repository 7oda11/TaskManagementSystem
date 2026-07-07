namespace TaskManagementSystem.Services.DTOs
{
    public class PagedResultDto<T>
    {
        public IEnumerable<T> Data { get; set; } = new List<T>();
        public int TotalRecords { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalRecords / (double)PageSize);
    }
}
