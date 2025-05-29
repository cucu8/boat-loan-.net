using SadikTuranECommerce.Entities;

namespace SadikTuranECommerce.DTO
{
    public class UserCreateDTO
    {
        public string Email { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }

        public string Name { get; set; }

        public UserType UserType { get; set; } = UserType.BoatOwner;

        public string Password { get; set; } = string.Empty;

        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
