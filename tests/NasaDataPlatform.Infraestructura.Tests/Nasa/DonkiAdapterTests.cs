using System.Net;
using FluentAssertions;
using NasaDataPlatform.Dominio.Entidades;
using NasaDataPlatform.Dominio.Errores;
using NasaDataPlatform.Infraestructura.Nasa;
using NasaDataPlatform.Infraestructura.Tests.Soporte;

namespace NasaDataPlatform.Infraestructura.Tests.Nasa;

public class DonkiAdapterTests
{
    private static readonly Uri BaseAddress = new("https://api.nasa.gov/");
    private static readonly DateOnly Desde = new(2025, 6, 1);
    private static readonly DateOnly Hasta = new(2025, 6, 7);

    [Fact]
    public async Task BuscarAsync_Cme_ParseaTodosLosEventosDelFixture()
    {
        var json = RutaFixtures.LeerJson("donki-cme.json");
        using var cliente = FakeHttpMessageHandler.CrearCliente(HttpStatusCode.OK, json, BaseAddress);
        var adaptador = new DonkiAdapter(cliente, new NasaConfiguracion("DEMO_KEY"));

        var eventos = await adaptador.BuscarAsync(TipoClima.Cme, Desde, Hasta, CancellationToken.None);

        eventos.Should().HaveCount(13);
        var primero = eventos.OfType<EventoClimaEspacial.Cme>().First(e => e.Id == "2025-06-01T18:00:00-CME-001");
        primero.Velocidad.KilometrosPorSegundo.Should().Be(609.0);
    }

    [Fact]
    public async Task BuscarAsync_Flr_ParseaTodosLosEventosDelFixture()
    {
        var json = RutaFixtures.LeerJson("donki-flr.json");
        using var cliente = FakeHttpMessageHandler.CrearCliente(HttpStatusCode.OK, json, BaseAddress);
        var adaptador = new DonkiAdapter(cliente, new NasaConfiguracion("DEMO_KEY"));

        var eventos = await adaptador.BuscarAsync(TipoClima.Flr, Desde, Hasta, CancellationToken.None);

        eventos.Should().HaveCount(4);
        var primero = eventos.OfType<EventoClimaEspacial.Flr>().First(e => e.Id == "2025-06-02T10:59:00-FLR-001");
        primero.ClaseDestello.Should().Be("M3.3");
        primero.RegionActiva.Should().Be(14100);
    }

    [Theory]
    [InlineData(HttpStatusCode.TooManyRequests)]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    public async Task BuscarAsync_ErrorDeServidor_LanzaFuenteNoDisponible(HttpStatusCode status)
    {
        using var cliente = FakeHttpMessageHandler.CrearCliente(status, contenido: null, BaseAddress);
        var adaptador = new DonkiAdapter(cliente, new NasaConfiguracion("DEMO_KEY"));

        var accion = () => adaptador.BuscarAsync(TipoClima.Cme, Desde, Hasta, CancellationToken.None);

        await accion.Should().ThrowAsync<FuenteNoDisponibleException>();
    }

    [Fact]
    public async Task BuscarAsync_JsonInvalido_LanzaFuenteNoDisponibleYNoJsonException()
    {
        using var cliente = FakeHttpMessageHandler.CrearCliente(HttpStatusCode.OK, "esto no es json", BaseAddress);
        var adaptador = new DonkiAdapter(cliente, new NasaConfiguracion("DEMO_KEY"));

        var accion = () => adaptador.BuscarAsync(TipoClima.Cme, Desde, Hasta, CancellationToken.None);

        await accion.Should().ThrowAsync<FuenteNoDisponibleException>();
    }
}
