using FluentAssertions;
using NasaDataPlatform.Infraestructura.Fixtures;
using NasaDataPlatform.Infraestructura.Tests.Soporte;

namespace NasaDataPlatform.Infraestructura.Tests.Fixtures;

public class FixturesAcercamientosAdapterTests
{
    private readonly FixturesAcercamientosAdapter _adaptador = new(new FixturesConfiguracion(RutaFixtures.Carpeta));

    [Fact]
    public async Task BuscarAsync_DevuelveTodosLosAcercamientosEnRangoYDistanciaAmplios()
    {
        var acercamientos = await _adaptador.BuscarAsync(
            new DateOnly(2025, 6, 1), new DateOnly(2025, 7, 1), distMaxUa: 0.05, CancellationToken.None);

        acercamientos.Should().HaveCount(76);
    }

    [Fact]
    public async Task BuscarAsync_FiltraPorDistanciaMaxima()
    {
        var todos = await _adaptador.BuscarAsync(
            new DateOnly(2025, 6, 1), new DateOnly(2025, 7, 1), distMaxUa: 0.05, CancellationToken.None);

        var filtrados = await _adaptador.BuscarAsync(
            new DateOnly(2025, 6, 1), new DateOnly(2025, 7, 1), distMaxUa: 0.01, CancellationToken.None);

        filtrados.Should().HaveCountLessThan(todos.Count);
        filtrados.Should().OnlyContain(a => a.Distancia.UnidadesAstronomicas <= 0.01);
    }

    [Fact]
    public async Task BuscarAsync_FiltraPorRangoDeFechas()
    {
        var acercamientos = await _adaptador.BuscarAsync(
            new DateOnly(2025, 6, 1), new DateOnly(2025, 6, 1), distMaxUa: 0.05, CancellationToken.None);

        acercamientos.Should().NotBeEmpty();
        acercamientos.Should().OnlyContain(a => a.Momento.Date == new DateTime(2025, 6, 1));
    }
}
