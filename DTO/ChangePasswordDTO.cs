using System.ComponentModel.DataAnnotations;

namespace SadikTuranECommerce.DTO
{
    public class ChangePasswordDTO
    {
        [Required(ErrorMessage = "Current password is required.")]
        public string CurrentPassword { get; set; }

        [Required(ErrorMessage = "New password is required.")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Confirm new password is required.")]
        [Compare("NewPassword", ErrorMessage = "New password and confirmation do not match.")]
        public string NewPasswordConfirm { get; set; }
    }
}
