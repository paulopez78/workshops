using System;
using MeetupEvents.Application;
using MeetupEvents.Contracts;
using MeetupEvents.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace MeetupEvents.Infrastructure
{
    public class MeetupEventDbContext : DbContext
    {
        public DbSet<MeetupEventAggregate> MeetupEvents { get; set; }

        public MeetupEventDbContext(DbContextOptions<MeetupEventDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MeetupEventAggregate>(b =>
            {
                b.Property(p => p.Status).HasConversion(new EnumToStringConverter<MeetupEventStatus>());
                b.Property(p => p.Version).IsConcurrencyToken();
                // b.Property(p => p.Title).IsConcurrencyToken();
                // b.UseXminAsConcurrencyToken();
            });

            modelBuilder.Entity<Attendant>(b =>
            {
                b.HasKey(x => x.Id);
                b.Property(x => x.Id).ValueGeneratedOnAdd();
                // b.UseXminAsConcurrencyToken();
            });

            // modelBuilder.Entity<Commands.V1.Attend>(b =>
            // {
            //     b.Property<Guid>("Id")
            //         .HasColumnType("uuid")
            //         .ValueGeneratedOnAdd();
            //     b.HasKey("Id");
            // });
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
}