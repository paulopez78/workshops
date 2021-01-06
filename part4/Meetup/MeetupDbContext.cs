using Meetup.Scheduling.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Meetup.Scheduling
{
    public class MeetupDbContext : DbContext
    {
        public MeetupDbContext(DbContextOptions<MeetupDbContext> options) : base(options)
        {
        }

        public DbSet<MeetupEvent> MeetupEvents { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MeetupEvent>(b =>
            {
                b.HasKey(p => p.Id);
                b.Property(p => p.Title);
                b.Property(p => p.Group);
                b.Property(p => p.Capacity);
                b.Property(p => p.Status).HasConversion(new EnumToStringConverter<MeetupEventStatus>());
                b.HasMany(p => p.Attendants);
                b.Property(p => p.Version).IsConcurrencyToken();
                // b.UseXminAsConcurrencyToken();
            });

            //https://docs.microsoft.com/en-us/ef/core/modeling/concurrency?tabs=data-annotations
            //https://www.npgsql.org/efcore/modeling/concurrency.html

            modelBuilder.Entity<Attendant>(b =>
            {
                b.HasKey(p => p.Id);
                b.Property(p => p.Id).ValueGeneratedOnAdd();
                b.Property(p => p.UserId);
                b.Property(p => p.Status).HasConversion(new EnumToStringConverter<AttendantStatus>());
                b.Property(p => p.ModifiedAt);
                // b.UseXminAsConcurrencyToken();
                b.HasIndex("UserId", "MeetupEventId")
                    .IsUnique();
            });
        }
    }

    public class MeetupDbContextFactory : IDesignTimeDbContextFactory<MeetupDbContext>
    {
        public MeetupDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<MeetupDbContext>();
            optionsBuilder.UseNpgsql(
                @"Host=localhost;Database=meetup;Username=meetup;Password=password;SearchPath=scheduling");
            return new MeetupDbContext(optionsBuilder.Options);
        }
    }
}