using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

#nullable disable
namespace Meetup.GroupManagement.Data
{
    public class MeetupGroupManagementDbContext : DbContext
    {
        public MeetupGroupManagementDbContext(DbContextOptions<MeetupGroupManagementDbContext> options) : base(options)
        {
        }

        public DbSet<MeetupGroup> MeetupGroups { get; set; }
        public DbSet<GroupMember>      Members      { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MeetupGroup>(b =>
            {
                //https://docs.microsoft.com/en-us/ef/core/modeling/concurrency?tabs=data-annotations
                //https://www.npgsql.org/efcore/modeling/concurrency.html
                b.HasIndex(p => p.Slug).IsUnique();
                b.UseXminAsConcurrencyToken();
            });

            modelBuilder.Entity<GroupMember>(b =>
            {
                b.UseXminAsConcurrencyToken();
                b.HasIndex(p => p.GroupId);
                b.HasIndex(p => new {p.GroupId, p.UserId}).IsUnique();
            });
        }
    }

    public class MeetupGroupManagementDbContextFactory : IDesignTimeDbContextFactory<MeetupGroupManagementDbContext>
    {
        public MeetupGroupManagementDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<MeetupGroupManagementDbContext>();
            optionsBuilder.UseNpgsql(
                @"Host=localhost;Database=meetup;Username=meetup;Password=password;SearchPath=group_management");
            return new MeetupGroupManagementDbContext(optionsBuilder.Options);
        }
    }
}