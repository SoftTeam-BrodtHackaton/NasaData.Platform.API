using NasaDataPlatform.Dominio.Valores;

namespace NasaDataPlatform.Dominio.Entidades;

/// <summary>
/// Un acercamiento cercano de un objeto (JPL CAD). Se excluyen deliberadamente
/// campos que la fuente trae pero que ninguna misión educativa necesita todavía:
/// orbit_id (identificador interno de la solución orbital de JPL), v_inf
/// (velocidad hiperbólica de exceso, menos intuitiva que la relativa a la Tierra),
/// dist_min/dist_max (banda de incertidumbre) y t_sigma_f (incertidumbre temporal,
/// que además no es una magnitud simple: JPL la reporta como texto "días_hh:mm").
/// </summary>
public sealed record Acercamiento
{
    /// <summary>Designación del objeto, p. ej. "2025 KS8" (campo "des" de JPL CAD).</summary>
    public required string Designacion { get; init; }

    public required DateTime Momento { get; init; }

    public required Distancia Distancia { get; init; }

    public required Velocidad VelocidadRelativa { get; init; }

    /// <summary>Magnitud absoluta (h): brillo intrínseco, proxy del tamaño del objeto.</summary>
    public required double MagnitudAbsoluta { get; init; }
}
