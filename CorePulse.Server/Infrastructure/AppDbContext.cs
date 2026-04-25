using CorePulse.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace CorePulse.Server.Infrastructure
{
    /// <summary>
    /// The primary database context for the CorePulse application.
    /// Handles the Object-Relational Mapping (ORM) between C# models and the SQL Server database.
    /// </summary>
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        /// <summary>
        /// Gets or sets the Users table.
        /// </summary>
        public DbSet<User> Users { get; set; }

        /// <summary>
        /// Configures the database schema and constraints using Fluent API.
        /// </summary>
        /// <param name="modelBuilder">The builder used to define the shape of the entities.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                // Primary Key configuration
                entity.HasKey(x => x.Id);

                // UserName constraints: required, unique, and limited length
                entity.Property(x => x.UserName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.HasIndex(x => x.UserName)
                    .IsUnique();

                // Email constraints: required, unique, and basic format validation
                entity.Property(x => x.Email)
                    .IsRequired()
                    .HasMaxLength(100);

                // SQL-level check constraint to ensure basic email structure
                entity.ToTable(t => t.HasCheckConstraint("CK_User_Email_Basic", "Email LIKE '%_@__%.__%'"));

                entity.HasIndex(x => x.Email)
                    .IsUnique();

                // Security: PasswordHash is mandatory
                entity.Property(x => x.PasswordHash)
                    .IsRequired();

                // Role conversion: Stores the Enum as a string in the database for better readability
                entity.Property(x => x.Role)
                    .HasConversion<string>();
            });
        }
    }
}