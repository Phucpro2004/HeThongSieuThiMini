namespace MiniShop.DTOs
{
    public class ProductResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Barcode { get; set; }
        public decimal Price { get; set; }
        public decimal OriginalPrice { get; set; }
        public int StockQuantity { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public string? Unit { get; set; }
        public bool IsActive { get; set; }
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }
    }
}
