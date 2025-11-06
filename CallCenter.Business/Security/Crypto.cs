using System.Security.Cryptography;
using System.Text;

namespace CallCenter.Business.Security
{
    public static class Crypto
    {
        public static string Sha256(string input)
        {
            SHA256 sha = SHA256.Create();
            byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            StringBuilder sb = new StringBuilder(bytes.Length * 2);
            for (int i = 0; i < bytes.Length; i++) sb.Append(bytes[i].ToString("x2"));
            return sb.ToString();
        }

        public static string GenerateUrlToken()
        {
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            byte[] data = new byte[32];
            rng.GetBytes(data);
            string b64 = System.Convert.ToBase64String(data); 
            return b64.Replace("+", "-").Replace("/", "_").TrimEnd('=');
        }
    }
}
