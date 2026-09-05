using FluentAssertions;

namespace NasaDataPlatform.Dominio.Tests;

/// <summary>
/// NasaDataPlatform.Dominio no debe adquirir ninguna dependencia externa: ni NuGet ni
/// referencia a otro proyecto. Se verifica leyendo el .csproj directamente en vez
/// de inspeccionar el ensamblado compilado, porque el compilador puede resolver
/// tipos del framework base sin que eso implique una dependencia que nos importe.
/// </summary>
public class AislamientoDominioTests
{
    [Fact]
    public void CsprojDeDominio_NoTienePackageReferenceNiProjectReference()
    {
        var ruta = ResolverRutaCsprojDominio();

        File.Exists(ruta).Should().BeTrue($"se esperaba encontrar el csproj de Dominio en '{ruta}'");

        var contenido = File.ReadAllText(ruta);

        contenido.Should().NotContain("<PackageReference", "NasaDataPlatform.Dominio no debe depender de ningún paquete NuGet");
        contenido.Should().NotContain("<ProjectReference", "NasaDataPlatform.Dominio no debe depender de ningún otro proyecto");
    }

    private static string ResolverRutaCsprojDominio()
    {
        // bin/Debug/net9.0 -> NasaDataPlatform.Dominio.Tests -> tests -> raíz de la solución
        var raizSolucion = Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

        return Path.Combine(raizSolucion, "src", "NasaDataPlatform.Dominio", "NasaDataPlatform.Dominio.csproj");
    }
}
