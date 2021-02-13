using System;
using System.Reflection.Metadata;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using static Meetup.Notifications.Contracts.ReadModels.V1;

namespace Meetup.Notifications.Queries
{
    public static class MongoConventions
    {
        public static void RegisterConventions()
        {
            if (BsonClassMap.IsClassMapRegistered(typeof(Document))) return;

            var pack = new ConventionPack
            {
                new CamelCaseElementNameConvention(),
                new IgnoreIfNullConvention(true),
                new EnumStringRepresentationConvention(),
                new IgnoreExtraElementsConvention(true)
            };

            ConventionRegistry.Register("notifications_queries", pack, _ => true);
            BsonSerializer.RegisterSerializer(new DateTimeSerializer(DateTimeKind.Utc));
            BsonSerializer.RegisterSerializer(new NullableSerializer<DateTime>());
            BsonSerializer.RegisterSerializer(new DateTimeOffsetSerializer(BsonType.String));
            BsonSerializer.RegisterSerializer(new NullableSerializer<DateTimeOffset>());
        }
    }

    public class EnumStringRepresentationConvention : EnumRepresentationConvention
    {
        public EnumStringRepresentationConvention() : base(BsonType.String)
        {
        }
    }
}