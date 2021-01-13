using System;
using System.Reflection.Metadata;
using Meetup.Scheduling.Domain;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;

namespace Meetup.Scheduling.Infrastructure
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

            ConventionRegistry.Register("meetup", pack, _ => true);

            BsonClassMap.RegisterClassMap<Aggregate>(
                    x =>
                    {
                        x.AutoMap();
                        x.MapIdMember(doc => doc.Id);
                    }
                )
                .Freeze();
            
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