﻿using Abstrack.Engine.Models;
using Markdig;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Abstrack.Engine.Repositories
{
    public class RequestRepository
    {
        public static async Task<Request> InsertRequest(Request request)
        {
            if (request == null) return null;
            if (request.body == null) return null;
            if (request.tags.Count > 12) return null;

            request.title = request.title.Length > 80 ? request.title.Substring(0, 80) : request.title;
            request.body = request.body.Length > 5000 ? request.body.Substring(0, 5000) : request.body;
            request.summary = GenerateSummary(request.body);
            request.tags = Tools.ValidateTags(request.tags);
            request.date_created = DateTime.UtcNow;

            return await (dynamic)CosmosRepository<Request>.CreateItemAsync(request);
        }

        public static async Task<List<Request>> GetListOfRequestIdsInTrack(string trackId)
        {
            return await CosmosRepository<Request>.GetItemsSqlAsync($"SELECT r.id FROM r WHERE r.track_id = '{trackId}'");
        }

        public static async void DeleteRequest(string requestId)
        {
            await CosmosRepository<Request>.DeleteItemAsync(requestId);
        }

        private static string GenerateSummary(string body)
        {
            var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
            var plainText = Markdown.ToPlainText(body, pipeline);

            return plainText.Length > 140 ? plainText.Substring(0, 140) : plainText;
        }
    }
}
