using Meetup.Scheduling.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
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
                b.OwnsOne(p => p.Details, d =>
                {
                    d.Property(p => p.Title).HasColumnName("Title");
                    d.Property(p => p.Description).HasColumnName("Description");
                });
                b.OwnsOne(p => p.Group, g => g.Property(p => p.Value).HasColumnName("Group"));
                b.OwnsOne(p => p.Location,
                    l =>
                    {
                        l.Property(p => p.Url).HasColumnName("Url");
                        l.Property(p => p.IsOnline).HasColumnName("IsOnline");
                        l.OwnsOne(p => p.Address, a => a.Property(p => p.Value).HasColumnName("Address"));
                    });

                b.OwnsOne(p => p.ScheduleTime, st =>
                {
                    st.Property(p => p.Start).HasColumnName("Start");
                    st.Property(p => p.End).HasColumnName("End");
                });
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
                    a.OwnsOne(p => p.Capacity,
                        c => c.Property(p => p.Value).HasColumnName("Capacity")
                    );

                    //https://docs.microsoft.com/en-us/ef/core/modeling/owned-entities#collections-of-owned-types
                    a.OwnsMany(p => p.Attendants, at =>
                    {
                        at.WithOwner().HasForeignKey("AttendantListId");
                        at.Property(p => p.UserId);
                        at.Property(p => p.Status).HasConversion(new EnumToStringConverter<AttendantStatus>());
                        at.Property(p => p.ModifiedAt);
                        at.HasIndex("UserId", "AttendantListId").IsUnique();
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