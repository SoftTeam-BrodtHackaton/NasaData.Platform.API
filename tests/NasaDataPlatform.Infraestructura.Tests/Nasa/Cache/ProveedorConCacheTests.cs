using FluentAssertions;
using NasaDataPlatform.Dominio.Entidades;
using NasaDataPlatform.Dominio.Puertos;
using NasaDataPlatform.Infraestructura.Nasa.Cache;
using Microsoft.Extensions.Caching.Memory;

namespace NasaDataPlatform.Infraestructura.Tests.Nasa.Cache;

public class ProveedorConCacheTests
{
    [Fact]
    public async Task Clima_SegundaLlamadaConMismosArgumentos_NoDisparaOtraSolicitud()
    {
        var interno = new ProveedorClimaFalso();
        using var cache = new MemoryCache(new MemoryCacheOptions());
        IProveedorClimaEspacial decorado = new ProveedorClimaConCache(interno, cache);

        var desde = new DateOnly(2025, 6, 1);
        var hasta = new DateOnly(2025, 6, 7);

        var primeraLlamada = await decorado.BuscarAsync(TipoClima.Cme, desde, hasta, CancellationToken.None);
        var segundaLlamada = await decorado.BuscarAsync(TipoClima.Cme, desde, hasta, CancellationToken.None);

        interno.Llamadas.Should().Be(1);
        segundaLlamada.Should().BeSameAs(primeraLlamada);
    }

    [Fact]
    public async Task Clima_ArgumentosDistintos_SiDisparaOtraSolicitud()
    {
        var interno = new ProveedorClimaFalso();
        using var cache = new MemoryCache(new MemoryCacheOptions());
        IProveedorClimaEspacial decorado = new ProveedorClimaConCache(interno, cache);

        await decorado.BuscarAsync(TipoClima.Cme, new DateOnly(2025, 6, 1), new DateOnly(2025, 6, 7), CancellationToken.None);
        await decorado.BuscarAsync(TipoClima.Flr, new DateOnly(2025, 6, 1), new DateOnly(2025, 6, 7), CancellationToken.None);

        interno.Llamadas.Should().Be(2);
    }

    [Fact]
    public async Task Acercamientos_SegundaLlamadaConMismosArgumentos_NoDisparaOtraSolicitud()
    {
        var interno = new ProveedorAcercamientosFalso();
        using var cache = new MemoryCache(new MemoryCacheOptions());
        IProveedorAcercamientos decorado = new ProveedorAcercamientosConCache(interno, cache);

        var desde = new DateOnly(2025, 6, 1);
        var hasta = new DateOnly(2025, 7, 1);

        var primeraLlamada = await decorado.BuscarAsync(desde, hasta, 0.05, CancellationToken.None);
        var segundaLlamada = await decorado.BuscarAsync(desde, hasta, 0.05, CancellationToken.None);

        interno.Llamadas.Should().Be(1);
        segundaLlamada.Should().BeSameAs(primeraLlamada);
    }

    private sealed class ProveedorClimaFalso : IProveedorClimaEspacial
    {
        public int Llamadas { get; private set; }

        public Task<IReadOnlyList<EventoClimaEspacial>> BuscarAsync(
            TipoClima tipo, DateOnly desde, DateOnly hasta, CancellationToken ct)
        {
            Llamadas++;
            IReadOnlyList<EventoClimaEspacial> resultado = [];
            return Task.FromResult(resultado);
        }
    }

    private sealed class ProveedorAcercamientosFalso : IProveedorAcercamientos
    {
        public int Llamadas { get; private set; }

        public Task<IReadOnlyList<Acercamiento>> BuscarAsync(
            DateOnly desde, DateOnly hasta, double distMaxUa, CancellationToken ct)
        {
            Llamadas++;
            IReadOnlyList<Acercamiento> resultado = [];
            return Task.FromResult(resultado);
        }
    }
}
