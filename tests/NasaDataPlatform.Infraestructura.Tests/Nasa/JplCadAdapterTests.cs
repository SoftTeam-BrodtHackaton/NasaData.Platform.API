using System.Net;
using FluentAssertions;
using NasaDataPlatform.Dominio.Errores;
using NasaDataPlatform.Infraestructura.Nasa;
using NasaDataPlatform.Infraestructura.Tests.Soporte;

namespace NasaDataPlatform.Infraestructura.Tests.Nasa;

public class JplCadAdapterTests
{
    private static readonly Uri BaseAddress = new("https://ssd-api.jpl.nasa.gov/");
    private static readonly DateOnly Desde = new(2025, 6, 1);
    private static readonly DateOnly Hasta = new(2025, 7, 1);
    private const double DistMaxUa = 0.05;

    [Fact]
    public async Task BuscarAsync_ParseaTodasLasFilasDelFixture()
    {
        var json = RutaFixtures.LeerJson("jpl-cad.json");
        using var cliente = FakeHttpMessageHandler.CrearCliente(HttpStatusCode.OK, json, BaseAddress);
        var adaptador = new JplCadAdapter(cliente);

        var acercamientos = await adaptador.BuscarAsync(Desde, Hasta, DistMaxUa, CancellationToken.None);

        acercamientos.Should().HaveCount(76);
        var primero = acercamientos.First(a => a.Designacion == "2025 KS8");
        primero.Distancia.UnidadesAstronomicas.Should().BeApproximately(0.0163724048208528, 1e-12);
        primero.VelocidadRelativa.KilometrosPorSegundo.Should().BeApproximately(10.3129981118553, 1e-9);
        primero.MagnitudAbsoluta.Should().BeApproximately(26.21, 1e-9);
    }

    [Theory]
    [InlineData(HttpStatusCode.TooManyRequests)]
    [InlineData(HttpStatusCode.InternalServerError)]
    public async Task BuscarAsync_ErrorDeServidor_LanzaFuenteNoDisponible(HttpStatusCode status)
    {
        using var cliente = FakeHttpMessageHandler.CrearCliente(status, contenido: null, BaseAddress);
        var adaptador = new JplCadAdapter(cliente);

        var accion = () => adaptador.BuscarAsync(Desde, Hasta, DistMaxUa, CancellationToken.None);

        await accion.Should().ThrowAsync<FuenteNoDisponibleException>();
    }

    [Fact]
    public async Task BuscarAsync_JsonInvalido_LanzaFuenteNoDisponibleYNoJsonException()
    {
        using var cliente = FakeHttpMessageHandler.CrearCliente(HttpStatusCode.OK, "esto no es json", BaseAddress);
        var adaptador = new JplCadAdapter(cliente);

        var accion = () => adaptador.BuscarAsync(Desde, Hasta, DistMaxUa, CancellationToken.None);

        await accion.Should().ThrowAsync<FuenteNoDisponibleException>();
    }
}
