using IssueTracker.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IssueTracker.Data
{
    public class IssuesDbContext : IdentityDbContext<IdentityUser>
    {

        public IssuesDbContext(DbContextOptions<IssuesDbContext> options)
            : base(options)
        {

        }

        public DbSet<Issue> Issues { get; set; }
        public DbSet<Application> Applications { get; set; }
        public DbSet<Comment> Comments { get; set; }



        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure relationships
            builder.Entity<Issue>()
                .HasOne(i => i.Application)
                .WithMany(a => a.Issues)
                .HasForeignKey(i => i.ApplicationId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Issue>()
                .HasOne(i => i.Developer)
                .WithMany()
                .HasForeignKey(i => i.DeveloperId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Issue>()
                .HasOne(i => i.Tester)
                .WithMany()
                .HasForeignKey(i => i.TesterId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
