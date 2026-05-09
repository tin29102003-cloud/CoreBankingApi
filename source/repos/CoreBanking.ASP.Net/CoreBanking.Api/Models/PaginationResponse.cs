namespace CoreBanking.Api.Models
{
    public class PaginationResponse<TEntity>(int pageindex, int pageSize, long count, IEnumerable<TEntity> items)
    {
        public int PageIndex => pageindex;
        public int PageSize => pageSize;
        public long TotalCount => count;
        public IEnumerable<TEntity> Items => items;
    }
}
