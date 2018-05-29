using FlashBot.Engine.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FlashBot.Engine.Repositories
{
    public class FeedRepository
    {
        public static async Task<List<PostQueryDTO>> GetFeed(string userId, long fromTime = 0, long continuationTime = 0)
        {
            // select
            var sqlString = $"SELECT TOP 30 p.id, p.post_id, p.tags, p.track_id, p.track_name, p.date_created, p.type, p.has_image, p.title, p.summary, p.url FROM p";

            // where user id
            sqlString += $" WHERE ARRAY_CONTAINS(p.subscriber_list, \"{userId}\")";

            // continuation or fromTime
            if (fromTime != 0)
            {
                sqlString += $" and p.date_created > {fromTime}";
            }
            else if (continuationTime != 0)
            {
                sqlString += $" and p.date_created < {continuationTime}";
            }


            // chronological
            sqlString += " ORDER BY p.date_created DESC";

            List<PostCosmos> posts = await CosmosRepository<PostCosmos>.GetItemsSqlAsync(sqlString);

            return ConvertCosmosResultToQueryDTO(posts);
        }

        private static List<PostQueryDTO> ConvertCosmosResultToQueryDTO(List<PostCosmos> posts)
        {
            List<PostQueryDTO> postDTO = new List<PostQueryDTO>();

            foreach (PostCosmos post in posts)
            {
                postDTO.Add(new PostQueryDTO()
                {
                    id = post.post_id,
                    date_created = post.date_created,
                    has_image = post.has_image,
                    summary = post.summary,
                    tags = post.tags,
                    title = post.title,
                    track_id = post.track_id,
                    track_name = post.track_name,
                    type = post.type,
                    url = post.url
                });
            }

            return postDTO;
        }
    }
}
