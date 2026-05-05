namespace CoreBanking.Api.Models
{
    public class PaginationRequest(int pagesize = 10, int pageIndex = 0)
    {
        public int PageSize { get; set; } = pagesize;
        public int PageIndex { get; set; } = pageIndex;
    }
}
