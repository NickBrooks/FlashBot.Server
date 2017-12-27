using Abstrack.Entities;
using Abstrack.Entities.Engine;
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
            request.tags = Tools.ValidateTags(request.tags);
            request.date_created = DateTime.UtcNow;

            return await (dynamic)CosmosRepository<Request>.CreateItemAsync(request);
        }
    }
}
