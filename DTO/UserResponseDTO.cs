using SadikTuranECommerce.Entities;

namespace SadikTuranECommerce.DTO
{
    public class UserResponseDTO
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string  Name { get; set; }
        public UserType UserType { get; set; }
     
    }
}
