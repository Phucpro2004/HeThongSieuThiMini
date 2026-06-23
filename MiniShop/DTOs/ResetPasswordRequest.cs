using System.ComponentModel.DataAnnotations;

namespace MiniShop.DTOs
{
    public class ResetPasswordRequest
    {
        [Required]
        public string NewPassword { get; set; } = null!;
    }
}
