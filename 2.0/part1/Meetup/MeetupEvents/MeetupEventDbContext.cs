using System;
using Microsoft.EntityFrameworkCore;

namespace MeetupEvents
{
    public class MeetupEventDbContext : DbContext
    {
        public DbSet<MeetupEvent> MeetupEvents { get; set; }

        public MeetupEventDbContext(DbContextOptions<MeetupEventDbContext> options) : base(options)
        {
        }
    }

    public record MeetupEvent(Guid Id, string Title, int Capacity)
    {
        public bool Published { get; set; }
    };

    public record MeetupEventOptions()
    {
        public int DefaultCapacity { get; init; } = 100;
    }
}