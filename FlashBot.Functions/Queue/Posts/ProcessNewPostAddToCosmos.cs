using FlashBot.Engine.Models;
using FlashBot.Engine.Repositories;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FlashBot.Functions.Queue.Posts
{
    public static class ProcessNewPostAddToCosmos
    {
        [FunctionName("ProcessNewPostAddToCosmos")]
        public static async void Run([QueueTrigger("process-new-post-add-to-cosmos", Connection = "QUEUESTORAGE_CONNECTION")]CloudQueueMessage myQueueItem, TraceWriter log)
        {
            Post post = JsonConvert.DeserializeObject<Post>(myQueueItem.AsString);

            List<string> tags = !string.IsNullOrEmpty(post.tags) ? post.tags.Split(',').ToList() : new List<string>();

            // get subscribers
            List<TrackFollow> followers = await FollowRepository.GetTrackFollows(post.PartitionKey, Enums.FollowMode.Feed);
            List<string> followersToFeed = new List<string>();

            // determine who gets feed
            foreach (TrackFollow follower in followers)
            {
                foreach (TagCriteria criterion in follower.criteria.Where(c => c.feed))
                {
                    if (ContainsAllItems(criterion.tags, tags))
                    {
                        followersToFeed.Add(follower.user_id);
                        break;
                    }
                }
            }

            IEnumerable<List<string>> splitFollowers = splitList(followersToFeed, 1000);
            int i = 1;

            if (splitFollowers.Count() == 0)
            {
                PostCosmos postCosmos = new PostCosmos()
                {
                    id = Guid.NewGuid().ToString(),
                    post_id = post.RowKey,
                    track_id = post.PartitionKey,
                    date_created = post.date_created,
                    track_name = post.track_name,
                    summary = post.summary,
                    tags = tags,
                    has_image = post.has_image,
                    title = post.title,
                    type = post.type,
                    url = post.url,
                    subscriber_list = new List<string>(),
                    is_root_post = true
                };

                await PostRepository.InsertPostToCosmos(postCosmos);
                log.Info($"Added post to Cosmos: {post.RowKey}");
            }
            else
            {
                foreach (List<string> list in splitFollowers)
                {
                    PostCosmos postCosmos = new PostCosmos()
                    {
                        id = Guid.NewGuid().ToString(),
                        post_id = post.RowKey,
                        track_id = post.PartitionKey,
                        date_created = post.date_created,
                        track_name = post.track_name,
                        summary = post.summary,
                        tags = tags,
                        has_image = post.has_image,
                        title = post.title,
                        type = post.type,
                        url = post.url,
                        subscriber_list = list,
                        is_root_post = i == 1 ? true : false
                    };

                    i++;
                    await PostRepository.InsertPostToCosmos(postCosmos);
                    log.Info($"Added post to Cosmos: {post.RowKey}");
                }
            }
        }

        private static bool ContainsAllItems<T>(IEnumerable<T> a, IEnumerable<T> b)
        {
            return !b.Except(a).Any();
        }

        private static IEnumerable<List<T>> splitList<T>(List<T> locations, int nSize = 30)
        {
            for (int i = 0; i < locations.Count; i += nSize)
            {
                yield return locations.GetRange(i, Math.Min(nSize, locations.Count - i));
            }
        }
    }
}
