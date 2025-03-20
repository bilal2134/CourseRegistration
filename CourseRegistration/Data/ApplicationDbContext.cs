using CourseRegistration.Models;
using Microsoft.EntityFrameworkCore;

namespace CourseRegistration.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<CourseRegistration.Models.CourseRegistration> CourseRegistrations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // You can configure composite keys or relationships if needed.
            modelBuilder.Entity<CourseRegistration.Models.CourseRegistration>()
                .HasIndex(cr => new { cr.UserId, cr.CourseId })
                .IsUnique();
        }
    }
}
