using Abstrack.Engine.Models;

namespace Abstrack.Engine.Repositories
{
    public class RequestMetaRepository
    {
        public static void InsertRequestMeta(RequestMeta requestMeta)
        {
            TableStorageRepository.InsertRequestMeta(requestMeta);
        }
    }
}
