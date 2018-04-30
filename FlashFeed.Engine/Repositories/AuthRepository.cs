using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace FlashFeed.Engine.Repositories
{
    // https://goo.gl/sShUhC
    public class AuthRepository
    {
        private static readonly string appSecret = Environment.GetEnvironmentVariable("ABSTRACK_SECRET_KEY");

        public static bool ValidateSHA256(string objectId, string objectKey, string sha256)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(objectId + objectKey + appSecret);
            SHA256Managed sha256HashString = new SHA256Managed();
            byte[] hash = sha256HashString.ComputeHash(bytes);

            var result = ByteArrayToHexString(hash);

            return sha256 == result ? true : false;
        }

        internal static string GenerateSHA256(string objectId, string objectKey)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(objectId + objectKey + appSecret);
            SHA256Managed sha256HashString = new SHA256Managed();
            byte[] hash = sha256HashString.ComputeHash(bytes);

            return ByteArrayToHexString(hash);
        }

        private static string ByteArrayToHexString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);

            return hex.ToString();
        }

        internal static string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private static Random random = new Random();
    }
}
