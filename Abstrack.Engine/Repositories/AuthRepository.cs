using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Abstrack.Engine.Repositories
{
    // https://goo.gl/sShUhC
    public class AuthRepository
    {
        private static readonly string appSecret = Environment.GetEnvironmentVariable("ABSTRACK_SECRET_KEY");

        public static bool ValidateSHA256(string objectId, string objectKey, string sha256)
        {
            byte[] SHA256Result;

            var input = objectId + objectKey + appSecret;

            StringBuilder stringBuilder = new StringBuilder();
            using (HMACSHA256 shaM = new HMACSHA256())
            {
                SHA256Result = shaM.ComputeHash(Encoding.UTF8.GetBytes(input));
            }

            foreach (byte b in SHA256Result)
                stringBuilder.AppendFormat("{0:x2}", b);

            return sha256 == stringBuilder.ToString() ? true : false;
        }

        internal static string GenerateSHA256(string objectId, string objectKey)
        {
            byte[] SHA256Result;

            var input = objectId + objectKey + appSecret;

            StringBuilder stringBuilder = new StringBuilder();
            using (HMACSHA256 shaM = new HMACSHA256())
            {
                SHA256Result = shaM.ComputeHash(Encoding.UTF8.GetBytes(input));
            }

            foreach (byte b in SHA256Result)
                stringBuilder.AppendFormat("{0:x2}", b);

            return stringBuilder.ToString();
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
