namespace NasaData.Platform.API.Students.Domain.Model;

public sealed record SkillContribution(int Astronomy, int Physics, int DataAnalysis, int Programming);
public sealed record Progress(int CompletedMissions, int TotalPoints, string Level);
public sealed record SkillProfile(int Astronomy, int Physics, int DataAnalysis, int Programming);
public static class PointsPolicy { public const int FirstCompletion = 100; }
public class MissionAttempt
{
    public int Id { get; private set; }
    public string StudentId { get; private set; } = "";
    public string MissionId { get; private set; } = "";
    public string OptionId { get; private set; } = "";
    public bool Correct { get; private set; }
    public int PointsEarned { get; private set; }
    public DateTimeOffset AttemptedAt { get; private set; }
    private MissionAttempt() { }
    internal MissionAttempt(string studentId, string missionId, string optionId, bool correct, int points)
    { StudentId = studentId; MissionId = missionId; OptionId = optionId; Correct = correct; PointsEarned = points; AttemptedAt = DateTimeOffset.UtcNow; }
}
public class Student
{
    public int Id { get; private set; }
    public string StudentId { get; private set; } = "";
    public int Revision { get; private set; }
    public int TotalPoints { get; private set; }
    public int CompletedMissions { get; private set; }
    public int Astronomy { get; private set; }
    public int Physics { get; private set; }
    public int DataAnalysis { get; private set; }
    public int Programming { get; private set; }
    private readonly List<MissionAttempt> _attempts = [];
    public IReadOnlyCollection<MissionAttempt> Attempts => _attempts.AsReadOnly();
    private Student() { }
    public Student(string studentId)
    {
        if (string.IsNullOrWhiteSpace(studentId) || studentId.Length > 100) throw new ArgumentException("studentId inválido.");
        StudentId = studentId;
    }
    public MissionAttempt Answer(string missionId, string optionId, bool correct, SkillContribution skills)
    {
        var first = correct && !_attempts.Any(x => x.MissionId == missionId && x.Correct);
        var earned = first ? PointsPolicy.FirstCompletion : 0;
        if (first)
        {
            CompletedMissions++;
            TotalPoints += earned;
            Astronomy = AddSkill(Astronomy, skills.Astronomy);
            Physics = AddSkill(Physics, skills.Physics);
            DataAnalysis = AddSkill(DataAnalysis, skills.DataAnalysis);
            Programming = AddSkill(Programming, skills.Programming);
        }
        Revision++;
        var attempt = new MissionAttempt(StudentId, missionId, optionId, correct, earned);
        _attempts.Add(attempt);
        return attempt;
    }
    private static int AddSkill(int current, int contribution) => (int)Math.Clamp((long)current + Math.Max(0, contribution), 0, 100);
    public Progress GetProgress() => new(CompletedMissions, TotalPoints, TotalPoints >= 300 ? "MISSION_SPECIALIST" : TotalPoints >= 100 ? "SPACE_EXPLORER" : "SPACE_CADET");
    public SkillProfile GetProfile() => new(Astronomy, Physics, DataAnalysis, Programming);
    public IEnumerable<string> RecommendedAreas() => new[] { (DataAnalysis, "Data Science"), (Astronomy, "Astronomy"), (Physics, "Physics"), (Programming, "Software espacial") }
        .Where(x => x.Item1 > 0).OrderByDescending(x => x.Item1).Take(3).Select(x => x.Item2);
}
