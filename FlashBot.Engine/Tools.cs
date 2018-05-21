using FlashBot.Engine.Models;
using Markdig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;

namespace FlashBot.Engine
{
    public class Tools
    {
        public static bool ValidateTag(string hashtag)
        {
            //check hashtag doesn't have spaces
            if (hashtag.Contains(" ")) return false;

            //check hashtag isn't empty or too long
            if (hashtag.Length < 1 || hashtag.Length > 32) return false;

            //check hashtag is alphanumeric only
            if (!Regex.IsMatch(hashtag, @"^[a-zA-Z0-9]+$")) return false;

            return true;
        }

        internal static long GetCountdownFromDateTime(long now)
        {
            // Sat, 20 Nov 2286 17:46:39 +0000 Epoch in milliseconds
            long futureUnixDate = 9999999999999;
            return futureUnixDate - now;
        }

        internal static long GetCountdownFromDateTime(DateTime now)
        {
            // Sat, 20 Nov 2286 17:46:39 +0000 Epoch in milliseconds
            long futureUnixDate = 9999999999999;
            return futureUnixDate - ConvertToEpoch(now);
        }

        internal static long GetCountdownFromDateTime(string now)
        {
            // Sat, 20 Nov 2286 17:46:39 +0000 Epoch in milliseconds
            long futureUnixDate = 9999999999999;
            return futureUnixDate - Convert.ToInt64(now);
        }

        public static long ConvertToEpoch(DateTime time)
        {
            return ((DateTimeOffset)time).ToUnixTimeMilliseconds();
        }

        public static bool IsValidGuid(string guid)
        {
            return Guid.TryParse(guid, out Guid o);
        }

        public static PostQuery GetQueryFromQueryParams(string trackId, string tagString, string continuation)
        {
            List<string> tags = string.IsNullOrEmpty(tagString) ? new List<string>() : ValidateTags(tagString.Split(',').Take(12).ToList());

            return new PostQuery()
            {
                tags = tags,
                track_id = trackId,
                sql = GenerateSQLQueryString(trackId, tags, continuation),
                continuation = continuation
            };
        }

        private static string GenerateSQLQueryString(string trackId, List<string> tags, string continuation = null)
        {
            var sqlString = $"SELECT r.id, r.date_created, r.tags, r.title, r.summary, r.track_id, r.track_name, r.url, r.type, r.has_image FROM r WHERE r.track_id = '{trackId}'";

            foreach (var tag in tags)
            {
                sqlString += $" and ARRAY_CONTAINS(r.tags, \"{tag}\")";
            }

            if (continuation != null)
            {
                sqlString += $" and r.date_created < {continuation}";
            }

            sqlString += " ORDER BY r.date_created DESC";

            return sqlString;
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

            return validatedHashtags.Take(12).ToList();
        }

        public static bool ValidateUri(string uri)
        {
            Uri uriResult;
            return Uri.TryCreate(uri, UriKind.Absolute, out uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }

        public static string Base64Encode(string plainTextString)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(plainTextString);

            var encodedString = Convert.ToBase64String(bytes);

            return encodedString;
        }

        public static string Base64Decode(string base64String)
        {
            // must be multiple of 4
            int mod4 = base64String.Length % 4;
            if (mod4 > 0)
            {
                base64String += new string('=', 4 - mod4);
            }

            byte[] bytes = Convert.FromBase64String(base64String);
            return Encoding.UTF8.GetString(bytes);
        }

        public static string GenerateSummary(string body)
        {
            var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
            var plainText = Markdown.ToPlainText(body, pipeline);

            return plainText.Length > 140 ? plainText.Substring(0, 140) : plainText;
        }

        internal static PostQueryDTO ConvertPostToPostQueryDTO(Post post)
        {
            return new PostQueryDTO()
            {
                date_created = post.date_created,
                id = post.RowKey,
                summary = post.summary,
                tags = post.tags.Split(',').ToList(),
                title = post.title,
                track_id = post.PartitionKey,
                track_name = post.track_name,
                type = post.type,
                url = post.url,
                has_image = post.has_image
            };
        }
    }
}
