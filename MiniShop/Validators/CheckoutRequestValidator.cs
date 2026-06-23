using FluentValidation;
using MiniShop.DTOs;

namespace MiniShop.Validators
{
    public class CheckoutRequestValidator : AbstractValidator<CheckoutRequest>
    {
        public CheckoutRequestValidator()
        {
            RuleFor(x => x.AmountReceived).GreaterThanOrEqualTo(0).WithMessage("Số tiền khách đưa không được âm.");
            RuleFor(x => x.CartItems).NotEmpty().WithMessage("Giỏ hàng không được để trống.");
            
            RuleForEach(x => x.CartItems).ChildRules(items =>
            {
                items.RuleFor(i => i.ProductId).GreaterThan(0).WithMessage("Mã sản phẩm không hợp lệ.");
                items.RuleFor(i => i.Quantity).GreaterThan(0).WithMessage("Số lượng phải lớn hơn 0.");
            });
        }
    }
}
