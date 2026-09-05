namespace NasaData.Platform.API.Missions.Domain.Model;

public enum MissionType { SOLAR_STORM, PLANETARY_DEFENSE }
public enum Difficulty { EASY, MEDIUM, HARD }
public sealed record Option(string Id, string Text);
public sealed record ScientificData(string? ClassType, string Date, string? ObjectName);
public sealed record MissionSource(string Provider, string System, bool RealData, string EventId);
public sealed record SkillWeights(int Astronomy, int Physics, int DataAnalysis, int Programming);
public sealed record Question(string Text, IReadOnlyList<Option> Options, string CorrectOptionId);

public class Mission
{
    public int Id { get; private set; }
    public string MissionId { get; private set; } = "";
    public string Title { get; private set; } = "";
    public string Description { get; private set; } = "";
    public MissionType Type { get; private set; }
    public Difficulty Difficulty { get; private set; }
    public string Objective { get; private set; } = "";
    public ScientificData ScientificData { get; private set; } = null!;
    public Question Question { get; private set; } = null!;
    public MissionSource Source { get; private set; } = null!;
    public SkillWeights SkillWeights { get; private set; } = null!;
    private Mission() { }

    public static Mission Generate(string id, MissionType type, Difficulty difficulty, ScientificData data, MissionSource source)
    {
        if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(source.EventId) || !Enum.IsDefined(type) || !Enum.IsDefined(difficulty))
            throw new ArgumentException("Identificador, tipo o dificultad inválidos.");
        var solar = type == MissionType.SOLAR_STORM;
        var answer = solar ? data.ClassType : data.ObjectName;
        if (string.IsNullOrWhiteSpace(answer)) throw new ArgumentException("El evento no contiene los datos necesarios.");
        var distractors = (solar ? new[] { "X10", "C1", "B2" } : new[] { "Objeto de práctica Alfa", "Objeto de práctica Beta", "Objeto de práctica Gamma" })
            .Where(x => x != answer).Take(2).Append(answer).OrderBy(x => x, StringComparer.Ordinal).ToArray();
        // The answer position varies with the event; it is never encoded in a public option id.
        var options = distractors.Select((text, i) => new Option(((char)('A' + i)).ToString(), text)).ToArray();
        return new Mission
        {
            MissionId = id, Type = type, Difficulty = difficulty,
            Title = solar ? "Alerta Solar" : "Defensa Planetaria",
            Description = source.RealData ? "Analiza el evento científico registrado." : "Analiza un evento simulado para practicar; no es un registro NASA real.",
            Objective = solar ? "Identifica la clasificación de la llamarada solar." : "Identifica el objeto del acercamiento registrado.",
            ScientificData = data, Source = source,
            Question = new Question(solar ? "¿Qué clasificación tiene la llamarada solar?" : "¿Qué objeto aparece en el registro de acercamiento?", options, options.Single(x => x.Text == answer).Id),
            SkillWeights = solar ? new(2, 2, 1, 0) : new(2, 1, 2, 0)
        };
    }
}
