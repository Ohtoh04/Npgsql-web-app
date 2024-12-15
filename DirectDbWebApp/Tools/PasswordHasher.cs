using System.Security.Cryptography;
using System.Text;

namespace DirectDbWebApp.Tools {
    public class PasswordHasher {
        public static string GenerateSalt() {
            var randomBytes = new byte[16];
            RandomNumberGenerator.Fill(randomBytes); 
            return Convert.ToBase64String(randomBytes);
        }

        public static string HashPassword(string password, string salt) {
            using (var sha256 = SHA256.Create()) {
                var combinedBytes = Encoding.UTF8.GetBytes(password + salt);
                var hash = sha256.ComputeHash(combinedBytes);
                return Convert.ToBase64String(hash);
            }
        }
    }
}
