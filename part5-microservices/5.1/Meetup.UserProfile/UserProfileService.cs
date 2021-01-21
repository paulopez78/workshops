using System.Threading.Tasks;
using Grpc.Core;
using Meetup.UserProfile.Contracts;
using MongoDB.Driver;

namespace Meetup.UserProfile
{
    public class UserProfileService : Contracts.UserProfile.UserProfileBase
    {
        public UserProfileService(IMongoDatabase database)
        {
            DbCollection = database.GetCollection<Data.UserProfile>(nameof(UserProfile));
        }

        readonly IMongoCollection<Data.UserProfile> DbCollection;

        public override async Task<CreateRequest.Types.CreateReply> Create(CreateRequest request, ServerCallContext context)
        {
            return null;
        }

        public override Task<GetRequest.Types.GetReply> Get(GetRequest request, ServerCallContext context)
        {
            return null;
        }
    }
}