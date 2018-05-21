using FlashBot.Engine.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FlashBot.Engine.Repositories
{
    // https://goo.gl/sShUhC
    public class AuthRepository
    {
        private static readonly string appSecret = Environment.GetEnvironmentVariable("FlashBot_SECRET_KEY");

        public static async Task<JWTObject> GenerateJWTObject(string userId)
        {
            long expiration = Tools.ConvertToEpoch(DateTime.UtcNow.AddMinutes(15));
            string refreshToken = await GenerateRefreshToken(userId);
            string jwt = GenerateJWT(userId, expiration);

            return new JWTObject()
            {
                expiration = expiration,
                refresh_token = refreshToken,
                token = jwt
            };
        }

        public static async Task<string> GenerateRefreshToken(string userId)
        {
            // create
            var token = GenerateSHA256(userId + Tools.ConvertToEpoch(DateTime.UtcNow).ToString());

            // insert to db
            await TableStorageRepository.InsertRefreshToken(new RefreshToken(userId, token));

            return EncodeKeyAndSecret(userId, token);
        }

        public static async Task<string> GetRefreshTokenUserAndDestroyToken(string encodedToken)
        {
            KeySecret keySecret = DecodeKeyAndSecret(encodedToken);

            var refreshToken = await TableStorageRepository.GetRefreshToken(keySecret.Key, keySecret.Secret);
            if (refreshToken == null)
                return null;

            TableStorageRepository.DeleteRefreshToken(refreshToken);

            return keySecret.Key;
        }

        public static AuthClaim ValidateAuthClaim(string token)
        {
            string[] jwt = token.Split('.');
            string authHeader = jwt[0];
            string authClaim = jwt[1];
            string secret = jwt[2];

            // check expiration
            AuthHeader header = JsonConvert.DeserializeObject<AuthHeader>(Tools.Base64Decode(authHeader));
            if (Tools.ConvertToEpoch(DateTime.UtcNow) > header.expiration) return null;

            // check SHA256 hash
            if (!ValidateSHA256(authHeader + authClaim, secret)) return null;

            return JsonConvert.DeserializeObject<AuthClaim>(Tools.Base64Decode(authClaim));
        }

        public static bool ValidateSHA256(string objectToCompare, string secret)
        {
            if (secret == null || objectToCompare == null)
                return false;

            byte[] bytes = Encoding.UTF8.GetBytes(objectToCompare + appSecret);
            SHA256Managed sha256HashString = new SHA256Managed();
            byte[] hash = sha256HashString.ComputeHash(bytes);

            return secret == ByteArrayToHexString(hash) ? true : false;
        }

        private static string GenerateJWT(string userId, long expiration)
        {
            string authHeader = JsonConvert.SerializeObject(new AuthHeader(expiration));

            string authClaim = JsonConvert.SerializeObject(new AuthClaim()
            {
                user_id = userId
            });

            string secret = GenerateSHA256(Tools.Base64Encode(authHeader) + Tools.Base64Encode(authClaim));

            return $"{Tools.Base64Encode(authHeader)}.{Tools.Base64Encode(authClaim)}.{secret}";
        }

        internal static string GenerateSHA256(string objectString)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(objectString + appSecret);
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

        public static string EncodeKeyAndSecret(string key, string secret)
        {
            return Tools.Base64Encode($"{key}.{secret}");
        }

        public static KeySecret DecodeKeyAndSecret(string base64String)
        {
            string base64 = Tools.Base64Decode(base64String);

            List<string> result = base64.Split('.').ToList();

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
