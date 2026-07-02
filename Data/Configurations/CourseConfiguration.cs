using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TmsApi.Entities;

namespace TmsApi.Data.Configurations;

public class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Code)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(c => c.Title)
            .IsRequired()
            .HasMaxLength(200); // Updated to 200 based on the lab text

        // 🛠️ ADDED: Map and configure your new property name
        builder.Property(c => c.MaxCapacity)
            .IsRequired();

        // 🛡️ ADDED: Unique index on Code to enforce business rules in PostgreSQL
        builder.HasIndex(c => c.Code)
            .IsUnique();
            
        // 🔄 ADDED: Restrict deleting a course if it has active enrollments (From Exercise 5)
        builder.HasMany(c => c.Enrollments)
            .WithOne(e => e.Course)
            .HasForeignKey(e => e.CourseId)
            .OnDelete(DeleteBehavior.Restrict); 
    }
}