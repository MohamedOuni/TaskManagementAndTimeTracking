using EY.TaskShare.Entities;
using Microsoft.EntityFrameworkCore;

namespace EY.TaskShare
{
    public class TaskShareContext : DbContext
    {
        public DbSet<Project> Projects { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Tasks> Tasks { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=DESKTOP-VARI5GO;Database=EYProject;Trusted_Connection=True;TrustServerCertificate=True;");
            }
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Tasks>()
                .HasOne(t => t.Project)
                .WithMany(p => p.Tasks)
                .HasForeignKey(t => t.ProjectId);
           
            modelBuilder.Entity<Tasks>()
            .HasOne(t => t.User)
            .WithMany(u => u.Tasks)
            .HasForeignKey(t => t.UserId);

            modelBuilder.Entity<Project>()
              .HasMany(e => e.Users)
              .WithMany(e => e.Projects)
              .UsingEntity(
            "ProjectUser",
            l => l.HasOne(typeof(User)).WithMany().HasForeignKey("UserId").HasPrincipalKey(nameof(User.Id)),
            r => r.HasOne(typeof(Project)).WithMany().HasForeignKey("ProjectId").HasPrincipalKey(nameof(Project.Id)),
            j => j.HasKey("UserId", "ProjectId"));

            base.OnModelCreating(modelBuilder);
        }
    }
}