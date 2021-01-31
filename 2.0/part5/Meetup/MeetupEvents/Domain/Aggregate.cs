using System;
using System.Collections.Generic;

namespace MeetupEvents.Domain
{
    public abstract class Aggregate
    {
        public Guid Id      { get; protected set; }
        public int  Version { get; private set; }
        public void IncreaseVersion() => Version += 1;

        protected List<object>        _changes = new();
        public    IEnumerable<object> Changes => _changes;

        public void ClearChanges() => _changes.Clear();
    }
}