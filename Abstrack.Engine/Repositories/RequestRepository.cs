﻿using Abstrack.Engine.Models;
using Markdig;
using System;
using System.Threading.Tasks;

namespace Abstrack.Engine.Repositories
{
    public class RequestRepository
    {
        public static async Task<Request> CreateAsync(Request request)
        {
            if (request == null) return null;
            if (request.Body == null) return null;
            if (request.Tags.Count > 12) return null;

            request.Title = request.Title.Length > 80 ? request.Title.Substring(0, 80) : request.Title;
            request.Body = request.Body.Length > 5000 ? request.Body.Substring(0, 5000) : request.Body;
            request.Summary = GenerateSummary(request.Body);
            request.Tags = Tools.ValidateTags(request.Tags);
            request.Date_Created = DateTime.UtcNow;

            return await (dynamic)CosmosRepository<Request>.CreateItemAsync(request);
        }

        private static string GenerateSummary(string body)
        {
            var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
            var plainText = Markdown.ToPlainText(body, pipeline);

            return plainText.Length > 140 ? plainText.Substring(0, 140) : plainText;
        }
    }
}