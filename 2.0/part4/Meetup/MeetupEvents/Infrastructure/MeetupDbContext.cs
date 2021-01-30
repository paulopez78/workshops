using System;
using MeetupEvents.Application;
using MeetupEvents.Contracts;
using MeetupEvents.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace MeetupEvents.Infrastructure
{
    public class MeetupDbContext : DbContext
    {
        public MeetupDbContext(DbContextOptions<MeetupDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MeetupEventAggregate>(b =>
            {
                b.ToTable("MeetupEvent");
                b.OwnsOne(p => p.Details, d =>
                {
                    d.Property(p => p.Title).HasColumnName("Title");
                    d.Property(p => p.Description).HasColumnName("Description");
                });
                b.OwnsOne(p => p.ScheduleTime, d =>
                {
                    d.Property(p => p.Start).HasColumnName("Start");
                    d.Property(p => p.End).HasColumnName("End");
                });
                b.OwnsOne(p => p.Location,
                    l =>
                    {
                        l.Property(p => p.Url).HasColumnName("Url");
                        l.Property(p => p.IsOnline).HasColumnName("IsOnline");
                        l.OwnsOne(p => p.Address, a => a.Property(p => p.Value).HasColumnName("Address"));
                    }
                );
                b.Property(p => p.Status).HasConversion(new EnumToStringConverter<MeetupEventStatus>());
                b.Property(p => p.Version).IsConcurrencyToken();
            });

            modelBuilder.Entity<AttendantListAggregate>(b =>
            {
                b.ToTable("AttendantList");
                b.Property(p => p.Status).HasConversion(new EnumToStringConverter<AttendantListStatus>());
                b.Property(p => p.Version).IsConcurrencyToken();
                b.OwnsOne(p => p.Capacity, d => d.Property(p => p.Value).HasColumnName("Capacity"));
            });

            modelBuilder.Entity<Attendant>(b =>
            {
                b.HasKey(x => x.Id);
                b.Property(x => x.Id).ValueGeneratedOnAdd();
            });

            // modelBuilder.Entity<Commands.V1.Attend>(b =>
            // {
            //     b.Property<int>("Id")
            //         .HasColumnType("uuid")
            //         .ValueGeneratedOnAdd();
            //     b.HasKey("Id");
            // });
        }
    }

    public class MeetupEventDbContextCreateDbContext : IDesignTimeDbContextFactory<MeetupDbContext>
    {
        public MeetupDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<MeetupDbContext>();
            optionsBuilder.UseNpgsql(
                @"Host=localhost;Database=meetup_events;Username=postgres;Password=mysecretpassword");
            return new MeetupDbContext(optionsBuilder.Options);
        }
    }
}