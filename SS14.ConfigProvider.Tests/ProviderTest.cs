using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SS14.ConfigProvider.Model;

namespace SS14.ConfigProvider.Tests;

public class ProviderTest : IClassFixture<TestFixture>
{
    private readonly TestFixture _fixture;

    public ProviderTest(TestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void SavingAndLoadingWorks()
    {
        var provider = _fixture.Provider;
        provider.Set("test", "value");
        
        provider.Load();
        
        var hasResult = provider.TryGet("test", out var value);
        Assert.True(hasResult);
        Assert.Equal("value", value);
    }

    [Fact]
    public void DbInterceptorCallsConfigurationProvider()
    {
        var services = _fixture.ServiceProvider();
        var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TestContext>();
        dbContext.ConfigurationStore.Add(new ConfigurationStore
        {
            Name = "test",
            Value = "value"
        });
        dbContext.SaveChanges();
        
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var result = configuration.GetSection("test").Value;
        Assert.NotNull(result);
        Assert.Equal("value", result);
    }
}