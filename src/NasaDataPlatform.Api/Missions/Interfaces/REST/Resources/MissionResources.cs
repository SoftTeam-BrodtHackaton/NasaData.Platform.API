using System.ComponentModel.DataAnnotations;
using NasaData.Platform.API.Missions.Domain.Model;
namespace NasaData.Platform.API.Missions.Interfaces.REST.Resources;
public record GenerateMissionRequest([Required, MaxLength(160)] string EventId, [Required] MissionType? Type, [Required] Difficulty? Difficulty);
public record MissionSummaryResponse(string Id, string Title, MissionType Type, Difficulty Difficulty, string Source);
public record OptionResponse(string Id, string Text);
public record QuestionResponse(string Text, IEnumerable<OptionResponse> Options);
public record ScientificDataResponse(string? ClassType, string Date, string? ObjectName);
public record MissionSourceResponse(string Provider, string System, bool RealData, string EventId);
public record MissionDetailResponse(string Id, string Title, string Description, MissionType Type, Difficulty Difficulty,
    string Objective, ScientificDataResponse ScientificData, QuestionResponse Question, MissionSourceResponse Source)
{
    public static MissionDetailResponse From(Mission m) => new(m.MissionId, m.Title, m.Description, m.Type, m.Difficulty, m.Objective,
        new(m.ScientificData.ClassType, m.ScientificData.Date, m.ScientificData.ObjectName),
        new(m.Question.Text, m.Question.Options.Select(o => new OptionResponse(o.Id, o.Text))),
        new(m.Source.Provider, m.Source.System, m.Source.RealData, m.Source.EventId));
}
