using System;

namespace Meetup.Scheduling.Domain
{
    public abstract class Aggregate
    {
        public string Id { get; protected set; }

        public int Version { get; private set; } = -1;

        public void IncreaseVersion() => Version += 1;
    }

    public abstract class Entity 
    {
        public Guid Id { get; protected set; }
    }
}