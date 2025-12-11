using System.Security.Cryptography;
using System.Text;
using System.IO;

namespace UserService.Helpers
{
    public static class HashHelper
    {
        public static string ComputeSha256Hex(string input)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            var sb = new StringBuilder(64);
            foreach (var b in bytes) sb.Append(b.ToString("x2"));
            return sb.ToString();
        }
    }
}
