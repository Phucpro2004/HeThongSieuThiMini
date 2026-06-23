using System.ComponentModel.DataAnnotations;

namespace MiniShop.Models
{
    public class GoodsReceiptDetail
    {
        [Key]
        public int Id { get; set; }

        public int GoodsReceiptId { get; set; }
        public GoodsReceipt? GoodsReceipt { get; set; }

        public int ProductId { get; set; }
        public Product? Product { get; set; }

        public int Quantity { get; set; }
        
        public decimal UnitPrice { get; set; }
        
        public decimal SubTotal { get; set; }
    }
}
