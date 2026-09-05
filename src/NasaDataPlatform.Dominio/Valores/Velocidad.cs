namespace NasaDataPlatform.Dominio.Valores;

/// <summary>
/// Velocidad en su unidad canónica: kilómetros por segundo.
/// Todas las fuentes reconocidas (DONKI CME, JPL CAD) reportan velocidad
/// directamente en km/s, así que no hay conversión de unidad de origen que hacer aquí.
/// </summary>
public readonly record struct Velocidad
{
    public double KilometrosPorSegundo { get; }

    private Velocidad(double kilometrosPorSegundo)
    {
        if (!double.IsFinite(kilometrosPorSegundo) || kilometrosPorSegundo < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(kilometrosPorSegundo),
                kilometrosPorSegundo,
                "La velocidad debe ser un número finito y no negativo.");
        }

        KilometrosPorSegundo = kilometrosPorSegundo;
    }

    public static Velocidad DesdeKilometrosPorSegundo(double valor) => new(valor);
}
