using System.Reflection;

namespace SkyView.Architecture.Tests;

public class DependencyTests:IDisposable
{
    private const string DomainAssembly = "SkyView.Domain";
    private const string DataAssembly = "SkyView.Data";
    private const string ServicesAssembly = "SkyView.Services";
    private const string WebAssembly = "SkyView.Web";

    private static int counter = 0;

    public DependencyTests()
    {
        counter++;
    }

    public void Dispose()
    {
        TestContext.Current.TestOutputHelper.WriteLine($"Counter ={counter}");
    }

    [Fact]
    public void DomainShouldNotReferenceAnyOtherSolutionProject()
    {
        AssertDoesNotReference(DomainAssembly, DataAssembly, ServicesAssembly, WebAssembly);
    }

    [Fact]
    public void ServicesShouldNotReferenceDataOrWeb()
    {
        AssertDoesNotReference(ServicesAssembly, DataAssembly, WebAssembly);
    }

    [Fact]
    public void DataShouldNotReferenceServicesOrWeb()
    {
        AssertDoesNotReference(DataAssembly, ServicesAssembly, WebAssembly);
    }

    [Fact]
    public void DataAndServicesShouldNotReferenceEachOther()
    {
        AssertDoesNotReference(DataAssembly, ServicesAssembly);
        AssertDoesNotReference(ServicesAssembly, DataAssembly);
    }

    [Theory]
    [InlineData(DomainAssembly)]
    [InlineData(DataAssembly)]
    [InlineData(ServicesAssembly)]
    public void LowerLayersShouldNotReferenceWeb(string assemblyName)
    {
        AssertDoesNotReference(assemblyName, WebAssembly);
    }

    private static void AssertDoesNotReference(string source, params string[] forbidden)
    {
        var references = Assembly.Load(source)
            .GetReferencedAssemblies()
            .Select(r => r.Name)
            .ToHashSet(StringComparer.Ordinal);

        var violations = forbidden.Where(references.Contains).ToArray();

        Assert.True(violations.Length == 0,
            $"'{source}' must not reference: {string.Join(", ", violations)}");
    }
}
