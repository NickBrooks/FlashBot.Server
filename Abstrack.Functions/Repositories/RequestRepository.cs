using Abstrack.Entities;
using Markdig;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Abstrack.Functions.Repositories
{
    class RequestRepository
    {
        public static async Task<Request> CreateAsync(Request request)
        {
            if (request == null) return null;
            if (request.body == null) return null;
            if (request.tags.Count > 12) return null;

            request.title = request.title.Length > 80 ? request.title.Substring(0, 80) : request.title;
            request.body = request.body.Length > 5000 ? request.body.Substring(0, 5000) : request.body;
            request.summary = GenerateSummary(request.body);
            request.tags = Entities.Engine.Tools.ValidateTags(request.tags);
            request.date_created = DateTime.UtcNow;

            // add tags to queue
            AddToTrackTagsQueue(request);

            return await (dynamic)CosmosRepository<Request>.CreateItemAsync(request);
        }

        private static void AddToTrackTagsQueue(Request request)
        {
            var trackTagsQueueItem = new TrackTagsQueueItem()
            {
                trackId = request.track_id,
                tags = request.tags
            };

            TableStorageRepository.AddMessageToQueue("process-tracktags", JsonConvert.SerializeObject(trackTagsQueueItem));
        }

        private static string GenerateSummary(string body)
        {
            var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
            var plainText = Markdown.ToPlainText(body, pipeline);

            return plainText.Length > 140 ? plainText.Substring(0, 140) : plainText;
        }
    }
}
