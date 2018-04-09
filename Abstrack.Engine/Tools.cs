﻿using Abstrack.Engine.Models;
using Markdig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;

namespace Abstrack.Engine
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

        public static bool IsValidGuid(string guid)
        {
            return Guid.TryParse(guid, out Guid o);
        }

        public static RequestQuery GetQueryFromQueryParams(IEnumerable<KeyValuePair<string, string>> queryParams)
        {
            // parse query params
            string trackId = queryParams
                .FirstOrDefault(q => string.Compare(q.Key, "trackId", true) == 0)
                .Value;

            // check invalid trackId
            if (!IsValidGuid(trackId))
                return null;

            string tagString = queryParams.FirstOrDefault(q => string.Compare(q.Key, "tags", true) == 0).Value;
            List<string> tags = string.IsNullOrEmpty(tagString) ? new List<string>() : ValidateTags(tagString.Split(',').Take(12).ToList());

            return new RequestQuery()
            {
                trackId = trackId,
                tags = tags,
                sql = GenerateSQLQueryString(trackId, tags)
            };
        }

        private static string GenerateSQLQueryString(string trackId, List<string> tags)
        {
            var sqlString = $"SELECT r.id, r.date_created, r.tags, r.title, r.summary FROM r WHERE r.track_id = '{trackId}'";

            foreach (var tag in tags)
            {
                sqlString += $" and ARRAY_CONTAINS(r.tags, \"{tag}\")";
            }

            sqlString += " ORDER BY r._ts DESC";

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

        public static string GetHeaderValue(HttpRequestHeaders headers, string value)
        {
            var header = headers.Where(t => t.Key == value).FirstOrDefault();

            if (header.Value == null)
                return null;

            var key = header.Value.ToList()[0];

            if (key == null || (key.Length != 64 && key.Length != 128))
                return null;

            return key;
        }

        public static string GenerateSummary(string body)
        {
            var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
            var plainText = Markdown.ToPlainText(body, pipeline);

            return plainText.Length > 140 ? plainText.Substring(0, 140) : plainText;
        }
    }
}
