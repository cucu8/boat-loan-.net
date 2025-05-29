using System.Security.Cryptography;
using System.Text;

namespace SadikTuranECommerce.Helpers
{
    public class PasswordHelper
    {
        public static void CreatePasswordHash(string password, out string passwordHash, out string passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                // Rastgele anahtar (key) oluşturuyoruz, bu bizim salt'ımız olacak
                passwordSalt = Convert.ToBase64String(hmac.Key);

                // Şifreyi UTF8 olarak byte dizisine çevirip hashliyoruz
                var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

                // Hash'i Base64 string olarak kaydediyoruz
                passwordHash = Convert.ToBase64String(hashBytes);
            }
        }

        public static bool VerifyPasswordHash(string password, string storedHash, string storedSalt)
        {
            var key = Convert.FromBase64String(storedSalt);
            using (var hmac = new HMACSHA512(key))
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                var computedHashString = Convert.ToBase64String(computedHash);
                return computedHashString == storedHash;
            }
        }
    }
}
