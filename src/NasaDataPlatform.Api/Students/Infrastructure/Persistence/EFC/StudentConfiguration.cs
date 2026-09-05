using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NasaData.Platform.API.Students.Domain.Model;
namespace NasaData.Platform.API.Students.Infrastructure.Persistence.EFC;
public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.StudentId).HasMaxLength(100);
        builder.HasAlternateKey(x => x.StudentId);
        builder.Property(x => x.Revision).IsConcurrencyToken();
        builder.HasMany(x => x.Attempts).WithOne().HasForeignKey(x => x.StudentId).HasPrincipalKey(x => x.StudentId);
        builder.Navigation(x => x.Attempts).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
public class MissionAttemptConfiguration : IEntityTypeConfiguration<MissionAttempt>
{
    public void Configure(EntityTypeBuilder<MissionAttempt> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.StudentId).HasMaxLength(100);
        builder.Property(x => x.MissionId).HasMaxLength(100);
        builder.Property(x => x.OptionId).HasMaxLength(20);
        builder.HasIndex(x => new { x.StudentId, x.MissionId });
    }
}
