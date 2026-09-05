using System.Globalization;
using NasaDataPlatform.Dominio.Entidades;
using NasaDataPlatform.Dominio.Puertos;
using Microsoft.Extensions.Caching.Memory;

namespace NasaDataPlatform.Infraestructura.Nasa.Cache;

/// <summary>
/// Decoradores de caché para los dos puertos. Registrados a mano en
/// DependencyInjection.cs (sin Scrutor: la implementación real/de fixtures se
/// registra bajo su propio tipo concreto y el puerto resuelve al decorador que la
/// envuelve, así que no hace falta reescribir descriptors de DI por reflexión).
/// TTL de 15 minutos: una clave de NASA tiene tope de 1000 solicitudes/hora
/// compartidas entre el equipo, y esto evita que tres personas repitiendo la
/// misma búsqueda la agoten.
/// </summary>
internal sealed class ProveedorClimaConCache : IProveedorClimaEspacial
{
    private static readonly TimeSpan Ttl = TimeSpan.FromMinutes(15);

    private readonly IProveedorClimaEspacial _interno;
    private readonly IMemoryCache _cache;

    public ProveedorClimaConCache(IProveedorClimaEspacial interno, IMemoryCache cache)
    {
        _interno = interno;
        _cache = cache;
    }

    public Task<IReadOnlyList<EventoClimaEspacial>> BuscarAsync(
        TipoClima tipo, DateOnly desde, DateOnly hasta, CancellationToken ct)
    {
        var clave = $"ClimaEspacial:{tipo}:{desde:O}:{hasta:O}";

        return _cache.GetOrCreateAsync(clave, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = Ttl;
            return _interno.BuscarAsync(tipo, desde, hasta, ct);
        })!;
    }
}

internal sealed class ProveedorAcercamientosConCache : IProveedorAcercamientos
{
    private static readonly TimeSpan Ttl = TimeSpan.FromMinutes(15);

    private readonly IProveedorAcercamientos _interno;
    private readonly IMemoryCache _cache;

    public ProveedorAcercamientosConCache(IProveedorAcercamientos interno, IMemoryCache cache)
    {
        _interno = interno;
        _cache = cache;
    }

    public Task<IReadOnlyList<Acercamiento>> BuscarAsync(
        DateOnly desde, DateOnly hasta, double distMaxUa, CancellationToken ct)
    {
        var clave = $"Acercamientos:{desde:O}:{hasta:O}:{distMaxUa.ToString(CultureInfo.InvariantCulture)}";

        return _cache.GetOrCreateAsync(clave, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = Ttl;
            return _interno.BuscarAsync(desde, hasta, distMaxUa, ct);
        })!;
    }
}
