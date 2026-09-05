using NasaDataPlatform.Dominio.Valores;

namespace NasaDataPlatform.Dominio.Entidades;

/// <summary>
/// Un evento de clima espacial. CME y FLR responden la misma pregunta ("¿qué pasó
/// en el Sol y cuándo?") pero cada uno trae una magnitud de naturaleza distinta
/// (velocidad de eyección vs. clase de destello): en vez de una bolsa de campos
/// nullables, se modela como jerarquía sellada para que el consumidor haga
/// pattern matching exhaustivo por tipo en lugar de preguntar "¿esto es null?".
/// </summary>
public abstract record EventoClimaEspacial
{
    public required string Id { get; init; }
    public required TipoClima Tipo { get; init; }
    public required DateTime Momento { get; init; }
    public required string Enlace { get; init; }

    private EventoClimaEspacial()
    {
    }

    /// <summary>Eyección de masa coronal (DONKI CME).</summary>
    public sealed record Cme : EventoClimaEspacial
    {
        public required Velocidad Velocidad { get; init; }

        /// <summary>Región solar de origen (sourceLocation). Null si la fuente no la reportó.</summary>
        public string? RegionOrigen { get; init; }
    }

    /// <summary>Fulguración solar (DONKI FLR).</summary>
    public sealed record Flr : EventoClimaEspacial
    {
        /// <summary>Clase de la fulguración tal como la reporta DONKI, p. ej. "M3.3".</summary>
        public required string ClaseDestello { get; init; }

        /// <summary>Número de región activa (activeRegionNum). Null si la fuente no la reportó.</summary>
        public int? RegionActiva { get; init; }
    }
}
