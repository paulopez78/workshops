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

        public DbSet<MeetupEventDetails> MeetupEventDetails { get; set; }

        public DbSet<AttendantList> AttendantList { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MeetupEventDetails>(b =>
            {
                b.HasKey(p => p.Id);
                b.Property(p => p.Title);
                b.Property(p => p.Group);
                b.Property(p => p.Status).HasConversion(new EnumToStringConverter<MeetupEventStatus>());
                b.Property(p => p.Version).IsConcurrencyToken();
                // b.UseXminAsConcurrencyToken();
            });

            //https://docs.microsoft.com/en-us/ef/core/modeling/concurrency?tabs=data-annotations
            //https://www.npgsql.org/efcore/modeling/concurrency.html

            modelBuilder.Entity<AttendantList>(b =>
            {
                b.HasKey(p => p.Id);
                b.Property(p => p.MeetupEventId);
                b.Property(p => p.Capacity);
                b.HasMany(p => p.Attendants);
                b.Property(p => p.Version).IsConcurrencyToken();
                // b.UseXminAsConcurrencyToken();
            });

            modelBuilder.Entity<Attendant>(b =>
            {
                b.HasKey(p => p.Id);
                b.Property(p => p.Id).ValueGeneratedOnAdd();
                b.Property(p => p.UserId);
                b.Property(p => p.Status).HasConversion(new EnumToStringConverter<AttendantStatus>());
                b.Property(p => p.ModifiedAt);
                // b.UseXminAsConcurrencyToken();
                b.HasIndex("UserId", "AttendantListId")
                    .IsUnique();
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