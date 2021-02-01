using System;
using System.Collections.Generic;

namespace Meetup.Scheduling.Domain
{
    public abstract class Aggregate
    {
        public Guid Id      { get; protected set; }
        public int  Version { get; private set; } = -1;

        protected readonly List<object> Events = new();

        public IReadOnlyList<object> Changes => Events;
        public void IncreaseVersion() => Version += 1;

        public void ClearChanges() => Events.Clear();
    }

    public abstract class Entity
    {
        public Guid Id { get; protected set; }
    }
}