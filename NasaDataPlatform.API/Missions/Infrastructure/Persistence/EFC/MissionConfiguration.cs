using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NasaData.Platform.API.Missions.Domain.Model;
namespace NasaData.Platform.API.Missions.Infrastructure.Persistence.EFC;
public class MissionConfiguration : IEntityTypeConfiguration<Mission>
{
    public void Configure(EntityTypeBuilder<Mission> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.MissionId).HasMaxLength(100);
        builder.HasIndex(x => x.MissionId).IsUnique();
        builder.Property(x => x.Title).HasMaxLength(200);
        builder.Property(x => x.Type).HasConversion<string>().HasMaxLength(40);
        builder.Property(x => x.Difficulty).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.ScientificData).HasConversion(x => JsonSerializer.Serialize(x, (JsonSerializerOptions?)null), x => JsonSerializer.Deserialize<ScientificData>(x, (JsonSerializerOptions?)null)!);
        builder.Property(x => x.Question).HasConversion(x => JsonSerializer.Serialize(x, (JsonSerializerOptions?)null), x => JsonSerializer.Deserialize<Question>(x, (JsonSerializerOptions?)null)!);
        builder.Property(x => x.Source).HasConversion(x => JsonSerializer.Serialize(x, (JsonSerializerOptions?)null), x => JsonSerializer.Deserialize<MissionSource>(x, (JsonSerializerOptions?)null)!);
        builder.Property(x => x.SkillWeights).HasConversion(x => JsonSerializer.Serialize(x, (JsonSerializerOptions?)null), x => JsonSerializer.Deserialize<SkillWeights>(x, (JsonSerializerOptions?)null)!);
    }
}
