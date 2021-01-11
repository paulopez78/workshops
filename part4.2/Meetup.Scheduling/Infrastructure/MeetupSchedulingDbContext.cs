using Meetup.Scheduling.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Meetup.Scheduling.Infrastructure
{
    public class MeetupSchedulingDbContext : DbContext
    {
        public MeetupSchedulingDbContext(DbContextOptions<MeetupSchedulingDbContext> options) : base(options)
        {
        }

        public DbSet<MeetupEventDetailsAggregate> MeetupEventDetails { get; set; }

        public DbSet<AttendantListAggregate> AttendantList { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MeetupEventDetailsAggregate>(b =>
            {
                b.ToTable("MeetupEvent");
                b.HasKey(p => p.Id);
                b.OwnsOne(p => p.Details);
                b.OwnsOne(p => p.Group);
                b.OwnsOne(p => p.Location, ca => ca.OwnsOne(p => p.Address));
                b.OwnsOne(p => p.ScheduleTime);
                b.Property(p => p.Status).HasConversion(new EnumToStringConverter<MeetupEventStatus>());
                b.Property(p => p.Version).IsConcurrencyToken();
            });

            //https://docs.microsoft.com/en-us/ef/core/modeling/concurrency?tabs=data-annotations
            //https://www.npgsql.org/efcore/modeling/concurrency.html

            modelBuilder.Entity<AttendantListAggregate>(b =>
            {
                b.ToTable("AttendantList");
                b.HasKey(p => p.Id);
                b.OwnsOne(p => p.AttendantList, a =>
                {
                    a.WithOwner();
                    a.OwnsOne(p => p.Capacity);

                    //https://docs.microsoft.com/en-us/ef/core/modeling/owned-entities#collections-of-owned-types
                    a.OwnsMany(p => p.Attendants, at =>
                    {
                        at.Property(p => p.UserId);
                        at.Property(p => p.Status).HasConversion(new EnumToStringConverter<AttendantStatus>());
                        at.Property(p => p.ModifiedAt);
                        // at.HasIndex("UserId", "AttendantListAggregateId").IsUnique();
                    });
                });
                b.Property(p => p.Version).IsConcurrencyToken();
            });
        }
    }

    public class MeetupSchedulingDbContextFactory : IDesignTimeDbContextFactory<MeetupSchedulingDbContext>
    {
        public MeetupSchedulingDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<MeetupSchedulingDbContext>();
            optionsBuilder.UseNpgsql(
                @"Host=localhost;Database=meetup;Username=meetup;Password=password;SearchPath=scheduling");
            return new MeetupSchedulingDbContext(optionsBuilder.Options);
        }
    }
}