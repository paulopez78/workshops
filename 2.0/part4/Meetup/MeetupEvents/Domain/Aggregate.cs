using System;

namespace MeetupEvents.Domain
{
    public abstract class Aggregate
    {
        public Guid Id      { get; protected set; }
        public int  Version { get; private set; }
        public void IncreaseVersion() => Version += 1;
    }
}