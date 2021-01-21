using System;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using Meetup.UserProfile.Contracts;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;

namespace Meetup.UserProfile
{
    public class IntegrationEventsPublisher : BackgroundService
    {
        readonly IMongoCollection<Data.UserProfile> DbCollection;
        readonly IPublishEndpoint                   PublishEndpoint;

        public IntegrationEventsPublisher(IMongoDatabase database, IPublishEndpoint publishEndpoint)
        {
            DbCollection    = database.GetCollection<Data.UserProfile>(nameof(UserProfile));
            PublishEndpoint = publishEndpoint;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var options = new ChangeStreamOptions {FullDocument = ChangeStreamFullDocumentOption.UpdateLookup};
            var pipeline =
                new EmptyPipelineDefinition<ChangeStreamDocument<Data.UserProfile>>().Match(
                    "{ operationType: { $in: [ 'insert', 'update, 'delete' ] } }");

            using var changeStream = DbCollection.Watch(pipeline, options).ToEnumerable().GetEnumerator();

            while (changeStream.MoveNext())
            {
                var change      = changeStream.Current;
                var userProfile = change?.FullDocument;

                object @event = (change?.OperationType) switch
                {
                    ChangeStreamOperationType.Delete =>
                        new Events.V1.UserProfileDeleted(
                            Guid.Parse(userProfile.Id)
                        ),
                    ChangeStreamOperationType.Update or ChangeStreamOperationType.Insert =>
                        new Events.V1.UserProfileUpdated(
                            Guid.Parse(userProfile.Id), userProfile.FirstName, userProfile.LastName, userProfile.Email
                        ),
                    _ => null
                };

                // publish integration event
                if (@event is not null)
                    await PublishEndpoint.Publish(@event, stoppingToken);
            }
        }
    }
}