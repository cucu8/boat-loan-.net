using System.Text.Json.Serialization;

namespace SadikTuranECommerce.Entities
{
    public class UserCredential
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public string PasswordHash { get; set; } = string.Empty;

        public string PasswordSalt { get; set; } = string.Empty;

        [JsonIgnore]
        public User User { get; set; } = null!;
    }
}
