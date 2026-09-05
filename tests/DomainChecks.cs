using System;
using System.Linq;
using System.Text.Json;
using NasaData.Platform.API.Missions.Domain.Model;
using NasaData.Platform.API.Missions.Interfaces.REST.Resources;
using NasaData.Platform.API.Students.Domain.Model;

static class DomainChecks
{
    private static int checks;
    private static void Check(bool condition, string name)
    {
        if (!condition) throw new Exception(name);
        checks++;
        Console.WriteLine("PASS " + name);
    }
    public static void Main()
    {
        var source = new MissionSource("Demo", "Fixture", false, "EVENT");
        var solar = Mission.Generate("SOLAR", MissionType.SOLAR_STORM, Difficulty.EASY, new("M5.2", "2026-09-05", null), source);
        var planetary = Mission.Generate("NEO", MissionType.PLANETARY_DEFENSE, Difficulty.MEDIUM, new(null, "2026-09-05", "Asteroid"), source);
        Check(solar.Question.Options.Single(x => x.Id == solar.Question.CorrectOptionId).Text == "M5.2", "Solar generation and internal answer");
        Check(planetary.Question.Options.Single(x => x.Id == planetary.Question.CorrectOptionId).Text == "Asteroid", "Planetary generation");
        foreach (var mission in new[] { solar, planetary })
        {
            var json = JsonSerializer.Serialize(MissionDetailResponse.From(mission)).ToLowerInvariant();
            Check(!json.Contains("correctoption") && !json.Contains("iscorrect") && !json.Contains("skillweights"), "Public DTO excludes evaluation metadata");
            Check(mission.Question.Options.Select(x => x.Text).Distinct().Count() == 3, "Distinct options");
        }
        var student = new Student("TEST");
        var weights = new SkillContribution(2, 2, 1, 0);
        Check(student.GetProgress().Level == "SPACE_CADET", "Initial level");
        Check(student.Answer("SOLAR", "A", false, weights).PointsEarned == 0 && student.GetProfile().Astronomy == 0, "Wrong answer earns no points or skills");
        Check(student.Answer("SOLAR", "B", true, weights).PointsEarned == 100, "First correct earns 100");
        Check(student.GetProgress().Level == "SPACE_EXPLORER", "Explorer level");
        Check(student.Answer("SOLAR", "B", true, weights).PointsEarned == 0 && student.GetProfile().Astronomy == 2, "Repeat cannot reward points or skills");
        Check(student.GetProgress().CompletedMissions == 1 && student.Attempts.Count == 3, "Attempts and distinct completion");
        for (var i = 0; i < 150; i++) student.Answer("OTHER-" + i, "A", true, weights);
        var profile = student.GetProfile();
        Check(profile.Astronomy == 100 && profile.Physics == 100 && profile.DataAnalysis == 100 && profile.Programming == 0, "Skills saturate at 100");
        Check(student.GetProgress().Level == "MISSION_SPECIALIST" && student.GetProgress().TotalPoints == 15100, "Specialist level and accumulated points");
        Console.WriteLine($"{checks} domain checks passed.");
    }
}
