using FluentValidation;
using MiniShop.DTOs;

namespace MiniShop.Validators
{
    public class ProductCreateRequestValidator : AbstractValidator<ProductCreateRequest>
    {
        public ProductCreateRequestValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Tên sản phẩm không được để trống.");
            RuleFor(x => x.Price).GreaterThanOrEqualTo(0).WithMessage("Giá bán không hợp lệ.");
            RuleFor(x => x.OriginalPrice).GreaterThanOrEqualTo(0).WithMessage("Giá gốc không hợp lệ.");
            RuleFor(x => x.StockQuantity).GreaterThanOrEqualTo(0).WithMessage("Số lượng tồn kho không được âm.");
            RuleFor(x => x.CategoryId).GreaterThan(0).WithMessage("Danh mục không hợp lệ.");
        }
    }
}
