using System.Net;
using FluentAssertions;
using NasaDataPlatform.Dominio.Entidades;
using NasaDataPlatform.Dominio.Puertos;
using NasaDataPlatform.Infraestructura.Fixtures;
using NasaDataPlatform.Infraestructura.Nasa;
using NasaDataPlatform.Infraestructura.Tests.Soporte;

namespace NasaDataPlatform.Infraestructura.Tests.Equivalencia;

/// <summary>
/// Alimentando al adaptador real y al de fixtures con los mismos datos, ambas
/// implementaciones de un puerto deben producir entidades equivalentes. Si esto
/// falla, la abstracción del puerto no está cumpliendo su propósito.
/// </summary>
public class EquivalenciaEntreImplementacionesTests
{
    [Theory]
    [InlineData(TipoClima.Cme, "donki-cme.json")]
    [InlineData(TipoClima.Flr, "donki-flr.json")]
    public async Task IProveedorClimaEspacial_AmbasImplementaciones_ProducenEventosEquivalentes(
        TipoClima tipo, string archivo)
    {
        var json = RutaFixtures.LeerJson(archivo);
        using var cliente = FakeHttpMessageHandler.CrearCliente(HttpStatusCode.OK, json, new Uri("https://api.nasa.gov/"));

        IProveedorClimaEspacial real = new DonkiAdapter(cliente, new NasaConfiguracion("DEMO_KEY"));
        IProveedorClimaEspacial fixtures = new FixturesClimaAdapter(new FixturesConfiguracion(RutaFixtures.Carpeta));

        var desde = new DateOnly(2025, 6, 1);
        var hasta = new DateOnly(2025, 6, 7);

        var eventosReal = await real.BuscarAsync(tipo, desde, hasta, CancellationToken.None);
        var eventosFixtures = await fixtures.BuscarAsync(tipo, desde, hasta, CancellationToken.None);

        eventosReal.Should().NotBeEmpty();
        eventosReal.Should().BeEquivalentTo(eventosFixtures);
    }

    [Fact]
    public async Task IProveedorAcercamientos_AmbasImplementaciones_ProducenAcercamientosEquivalentes()
    {
        var json = RutaFixtures.LeerJson("jpl-cad.json");
        using var cliente = FakeHttpMessageHandler.CrearCliente(
            HttpStatusCode.OK, json, new Uri("https://ssd-api.jpl.nasa.gov/"));

        IProveedorAcercamientos real = new JplCadAdapter(cliente);
        IProveedorAcercamientos fixtures = new FixturesAcercamientosAdapter(new FixturesConfiguracion(RutaFixtures.Carpeta));

        var desde = new DateOnly(2025, 6, 1);
        var hasta = new DateOnly(2025, 7, 1);
        const double distMaxUa = 0.05;

        var acercamientosReal = await real.BuscarAsync(desde, hasta, distMaxUa, CancellationToken.None);
        var acercamientosFixtures = await fixtures.BuscarAsync(desde, hasta, distMaxUa, CancellationToken.None);

        acercamientosReal.Should().NotBeEmpty();
        acercamientosReal.Should().BeEquivalentTo(acercamientosFixtures);
    }
}
