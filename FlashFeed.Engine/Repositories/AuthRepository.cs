using FlashFeed.Engine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace FlashFeed.Engine.Repositories
{
    // https://goo.gl/sShUhC
    public class AuthRepository
    {
        private static readonly string appSecret = Environment.GetEnvironmentVariable("FLASHFEED_SECRET_KEY");

        public static bool ValidateSHA256(string objectId, KeySecret keySecret)
        {
            if (keySecret == null)
                return false;

            byte[] bytes = Encoding.UTF8.GetBytes(objectId + keySecret.Key + appSecret);
            SHA256Managed sha256HashString = new SHA256Managed();
            byte[] hash = sha256HashString.ComputeHash(bytes);
            var haha = ByteArrayToHexString(hash);

            return keySecret.Secret == ByteArrayToHexString(hash) ? true : false;
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

        public static string EncodeKeyAndSecretToBase64(string key, string secret)
        {
            byte[] bytes = Encoding.ASCII.GetBytes($"{key}.{secret}");

            return Convert.ToBase64String(bytes);
        }

        public static KeySecret DecodeKeyAndSecretFromBase64(string base64String)
        {
            byte[] bytes = Convert.FromBase64String(base64String);
            string decodedString = Encoding.ASCII.GetString(bytes);

            List<string> result = decodedString.Split('.').ToList();

            if (result.Count < 2)
                return null;

            return new KeySecret()
            {
                Key = result[0],
                Secret = result[1]
            };
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
