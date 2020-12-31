using System;
using Meetup.Scheduling.Domain;
using Microsoft.EntityFrameworkCore;

namespace Meetup.Scheduling
{
    public class MeetupDbContext : DbContext
    {
        public MeetupDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Domain.MeetupEvent> MeetupEvents { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Domain.MeetupEvent>(b =>
            {
                b.HasKey(p => p.Id);
                b.Property(p => p.Title);
                b.Property(p => p.Group);
                b.Property(p => p.Capacity);
                b.Property(p => p.Status);
                b.HasMany(p => p.Invitations);
            });

            modelBuilder.Entity<Invitation>(b =>
            {
                b.HasKey(p => p.Id);
                b.Property(p => p.Id).ValueGeneratedOnAdd();
                b.Property(p => p.UserId);
                b.Property(p => p.Going);
            });
        }
    }
}