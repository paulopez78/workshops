using System;
using System.Collections.Generic;

namespace Meetup.Scheduling.Framework
{
    public abstract class Aggregate
    {
        public Guid Id      { get; init; }
        public int  Version { get; set; } = -1;

        readonly List<object> Events = new();

        public IEnumerable<object> Changes => Events;

        public void ClearChanges() => Events.Clear();

        public void Apply(object domainEvent)
        {
            Events.Add(domainEvent);
            Version += 1;
            When(domainEvent);
        }

        public abstract void When(object domainEvent);
    }
}