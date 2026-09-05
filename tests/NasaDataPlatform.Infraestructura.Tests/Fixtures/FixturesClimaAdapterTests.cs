using FluentAssertions;
using NasaDataPlatform.Dominio.Entidades;
using NasaDataPlatform.Infraestructura.Fixtures;
using NasaDataPlatform.Infraestructura.Tests.Soporte;

namespace NasaDataPlatform.Infraestructura.Tests.Fixtures;

public class FixturesClimaAdapterTests
{
    private readonly FixturesClimaAdapter _adaptador = new(new FixturesConfiguracion(RutaFixtures.Carpeta));

    [Fact]
    public async Task BuscarAsync_Cme_DevuelveTodosLosEventosEnRangoAmplio()
    {
        var eventos = await _adaptador.BuscarAsync(
            TipoClima.Cme, new DateOnly(2025, 6, 1), new DateOnly(2025, 6, 7), CancellationToken.None);

        eventos.Should().HaveCount(13);
        eventos.Should().AllBeOfType<EventoClimaEspacial.Cme>();
    }

    [Fact]
    public async Task BuscarAsync_Flr_DevuelveTodosLosEventosEnRangoAmplio()
    {
        var eventos = await _adaptador.BuscarAsync(
            TipoClima.Flr, new DateOnly(2025, 6, 1), new DateOnly(2025, 6, 7), CancellationToken.None);

        eventos.Should().HaveCount(4);
        eventos.Should().AllBeOfType<EventoClimaEspacial.Flr>();
    }

    [Fact]
    public async Task BuscarAsync_FiltraPorRangoDeFechas()
    {
        // El 2025-06-03 solo cae la FLR "2025-06-03T12:52:00-FLR-001".
        var eventos = await _adaptador.BuscarAsync(
            TipoClima.Flr, new DateOnly(2025, 6, 3), new DateOnly(2025, 6, 3), CancellationToken.None);

        eventos.Should().ContainSingle();
    }
}
