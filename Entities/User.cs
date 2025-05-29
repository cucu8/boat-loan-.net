namespace SadikTuranECommerce.Entities
{
    public enum UserType
    {
        Admin,
        BoatOwner
    }

    public class User
    {
        public int Id { get; set; }

        public string Email { get; set; } = string.Empty;

        public required string Name { get; set; }

        public string? PhoneNumber { get; set; }

        public UserType UserType { get; set; } = UserType.BoatOwner;

        public ICollection<Boat> Boats { get; set; } = new List<Boat>();

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public UserCredential? UserCredential { get; set; }
    }
}
