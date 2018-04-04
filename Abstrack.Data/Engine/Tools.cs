using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Abstrack.Data.Engine
{
    public class Tools
    {
        // https://goo.gl/sShUhC
        public static string CreateSHA256(string input)
        {
            byte[] SHA256Result;

            StringBuilder stringBuilder = new StringBuilder();
            using (HMACSHA256 shaM = new HMACSHA256())
            {
                SHA256Result = shaM.ComputeHash(Encoding.UTF8.GetBytes(input));
            }

            foreach (byte b in SHA256Result)
                stringBuilder.AppendFormat("{0:x2}", b);

            return stringBuilder.ToString();
        }

        public static bool ValidateTag(string hashtag)
        {
            //check hashtag doesn't have spaces
            if (hashtag.Contains(" ")) return false;

            //check hashtag isn't empty or too long
            if (hashtag.Length < 1 || hashtag.Length > 40) return false;

            //check hashtag is alphanumeric only
            if (!Regex.IsMatch(hashtag, @"^[a-zA-Z0-9]+$")) return false;

            return true;
        }

        public static List<string> ValidateTags(List<string> hashtags)
        {
            var validatedHashtags = new List<string>();
            var rawHashtags = hashtags.Select(h => h).Distinct().ToList();

            foreach (var hashtag in rawHashtags)
            {
                if (!ValidateTag(hashtag))
                    continue;

                validatedHashtags.Add(hashtag);
            }

            return validatedHashtags;
        }
    }
}
