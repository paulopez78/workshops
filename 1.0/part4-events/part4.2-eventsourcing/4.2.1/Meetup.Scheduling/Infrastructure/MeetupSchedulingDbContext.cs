using Meetup.Scheduling.AttendantList;
using Meetup.Scheduling.MeetupDetails;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable
namespace Meetup.Scheduling.Infrastructure
{
    public class MeetupSchedulingDbContext : DbContext
    {
        public MeetupSchedulingDbContext(DbContextOptions<MeetupSchedulingDbContext> options) : base(options)
        {
        }

        public DbSet<OutBox> Outbox { get; set; }

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
                
                //https://docs.microsoft.com/en-us/ef/core/modeling/concurrency
                b.Property(p => p.Version).IsConcurrencyToken();
            });


            modelBuilder.Entity<AttendantListAggregate>(b =>
            {
                b.ToTable("AttendantList");
                b.HasKey(p => p.Id);
                b.OwnsOne(p => p.Capacity, c => c.Property(p => p.Value).HasColumnName("Capacity"));
                
                //https://docs.microsoft.com/en-us/ef/core/modeling/concurrency
                b.Property(p => p.Version).IsConcurrencyToken();
            });

            modelBuilder.Entity<Attendant>(b =>
            {
                b.Property<int>("Id")
                    .HasColumnType("int")
                    .ValueGeneratedOnAdd();
                b.HasKey("Id");
            });

            modelBuilder.Entity<OutBox>(b =>
            {
                b.Property<int>("Id")
                    .HasColumnType("int")
                    .ValueGeneratedOnAdd();
                b.HasKey("Id");
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