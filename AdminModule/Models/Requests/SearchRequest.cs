namespace AdminModule.Models.Requests
{
    public class SearchRequest
    {
        public string? Name { get; set; }
        public int Page { get; set; } = 0;
        public int BatchSize { get; set; } = 10;
        public string? OrderBy { get; set; }
    }
}
