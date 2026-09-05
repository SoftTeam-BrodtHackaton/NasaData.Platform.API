namespace NasaDataPlatform.Dominio.Valores;

/// <summary>
/// Distancia en su unidad canónica: unidades astronómicas (UA), que es lo que
/// entrega JPL CAD directamente. Expone conversión a distancias lunares (LD)
/// porque es la unidad más intuitiva para una audiencia de 14 a 18 años al
/// describir un acercamiento cercano.
/// </summary>
public readonly record struct Distancia
{
    private const double KilometrosPorUnidadAstronomica = 149_597_870.7;
    private const double KilometrosPorDistanciaLunar = 384_400;

    public double UnidadesAstronomicas { get; }

    public double DistanciasLunares =>
        UnidadesAstronomicas * KilometrosPorUnidadAstronomica / KilometrosPorDistanciaLunar;

    private Distancia(double unidadesAstronomicas)
    {
        if (!double.IsFinite(unidadesAstronomicas) || unidadesAstronomicas < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(unidadesAstronomicas),
                unidadesAstronomicas,
                "La distancia debe ser un número finito y no negativo.");
        }

        UnidadesAstronomicas = unidadesAstronomicas;
    }

    public static Distancia DesdeUnidadesAstronomicas(double valor) => new(valor);

    public static Distancia DesdeDistanciasLunares(double valor) =>
        new(valor * KilometrosPorDistanciaLunar / KilometrosPorUnidadAstronomica);
}
