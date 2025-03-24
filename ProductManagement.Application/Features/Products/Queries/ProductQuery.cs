namespace ProductManagement.Application.Features.Products.Queries
{
    public class ProductQuery
    {
        public required string Name { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}