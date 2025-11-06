using System.Security.Cryptography;
using System.Text;

namespace CallCenter.Business.Security
{
    public static class PasswordHasher
    {
        public static string Sha256(string plain)
        {
            using (var sha = SHA256.Create())
            {
                var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(plain));
                var sb = new StringBuilder(bytes.Length * 2);
                foreach (var b in bytes) sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }
    }
}
