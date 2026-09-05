using NasaDataPlatform.Dominio.Puertos;
using NasaDataPlatform.Infraestructura.Fixtures;
using NasaDataPlatform.Infraestructura.Nasa;
using NasaDataPlatform.Infraestructura.Nasa.Cache;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace NasaDataPlatform.Infraestructura;

public static class DependencyInjection
{
    private const int TimeoutSegundos = 15;

    /// <summary>
    /// Registra IProveedorClimaEspacial e IProveedorAcercamientos. La clave
    /// "FuenteDatos" (valores "Nasa" o "Fixtures") decide si se usan los
    /// adaptadores reales o el respaldo de demo que lee de disco.
    /// </summary>
    public static IServiceCollection AgregarProveedoresDeDatosEspaciales(
        this IServiceCollection servicios, IConfiguration configuracion)
    {
        var fuente = configuracion["FuenteDatos"];

        if (string.Equals(fuente, "Fixtures", StringComparison.OrdinalIgnoreCase))
        {
            AgregarFixtures(servicios, configuracion);
        }
        else if (string.Equals(fuente, "Nasa", StringComparison.OrdinalIgnoreCase))
        {
            AgregarNasa(servicios, configuracion);
        }
        else
        {
            throw new InvalidOperationException(
                $"Configuración 'FuenteDatos' inválida: '{fuente ?? "(vacío)"}'. Valores esperados: 'Nasa' o 'Fixtures'.");
        }

        servicios.AddMemoryCache();

        return servicios;
    }

    private static void AgregarNasa(IServiceCollection servicios, IConfiguration configuracion)
    {
        var apiKey = configuracion["Nasa:ApiKey"] ?? "DEMO_KEY";
        servicios.AddSingleton(new NasaConfiguracion(apiKey));

        // Se registra el adaptador bajo su propio tipo concreto, no bajo el
        // puerto: el puerto lo resuelve el decorador de caché más abajo.
        servicios.AddHttpClient<DonkiAdapter>(cliente =>
        {
            cliente.BaseAddress = new Uri("https://api.nasa.gov/");
            cliente.Timeout = TimeSpan.FromSeconds(TimeoutSegundos);
        });

        servicios.AddHttpClient<JplCadAdapter>(cliente =>
        {
            cliente.BaseAddress = new Uri("https://ssd-api.jpl.nasa.gov/");
            cliente.Timeout = TimeSpan.FromSeconds(TimeoutSegundos);
        });

        servicios.AddSingleton<IProveedorClimaEspacial>(sp =>
            new ProveedorClimaConCache(sp.GetRequiredService<DonkiAdapter>(), sp.GetRequiredService<IMemoryCache>()));
        servicios.AddSingleton<IProveedorAcercamientos>(sp =>
            new ProveedorAcercamientosConCache(sp.GetRequiredService<JplCadAdapter>(), sp.GetRequiredService<IMemoryCache>()));
    }

    private static void AgregarFixtures(IServiceCollection servicios, IConfiguration configuracion)
    {
        var carpeta = configuracion["Fixtures:Carpeta"]
            ?? Path.Combine(AppContext.BaseDirectory, "Fixtures", "Datos");

        servicios.AddSingleton(new FixturesConfiguracion(carpeta));
        servicios.AddSingleton<FixturesClimaAdapter>();
        servicios.AddSingleton<FixturesAcercamientosAdapter>();

        servicios.AddSingleton<IProveedorClimaEspacial>(sp =>
            new ProveedorClimaConCache(sp.GetRequiredService<FixturesClimaAdapter>(), sp.GetRequiredService<IMemoryCache>()));
        servicios.AddSingleton<IProveedorAcercamientos>(sp =>
            new ProveedorAcercamientosConCache(sp.GetRequiredService<FixturesAcercamientosAdapter>(), sp.GetRequiredService<IMemoryCache>()));
    }
}
