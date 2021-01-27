using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace MeetupEvents.Infrastructure
{
    public class MeetupEventDbContext : DbContext
    {
        public DbSet<MeetupEvent> MeetupEvents { get; set; }

        public MeetupEventDbContext(DbContextOptions<MeetupEventDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MeetupEvent>(b =>
            {
                b.Property(p => p.Status).HasConversion(new EnumToStringConverter<MeetupEventStatus>());
            });
        }
    }

    public class MeetupEventDbContextCreateDbContext : IDesignTimeDbContextFactory<MeetupEventDbContext>
    {
        public MeetupEventDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<MeetupEventDbContext>();
            optionsBuilder.UseNpgsql(
                @"Host=localhost;Database=meetup_events;Username=postgres;Password=mysecretpassword");
            return new MeetupEventDbContext(optionsBuilder.Options);
        }
    }


    public record MeetupEvent(Guid Id, string Title, int Capacity)
    {
        public MeetupEventStatus Status            { get; set; }
        public string?           CancelationReason { get; set; }
    };

    public enum MeetupEventStatus
    {
        Draft,
        Published,
        Cancelled
    }

    public record MeetupEventOptions()
    {
        public int DefaultCapacity { get; init; } = 100;
    }
}