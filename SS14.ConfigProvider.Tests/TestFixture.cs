using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SS14.ConfigProvider.Tests;

public class TestFixture
{
    public DbConfigurationProvider<TestContext> Provider => new(
        new DbConfigurationSource<TestContext>
        {
            OptionsAction = b => b.UseInMemoryDatabase("TestDb")
        });

    public IServiceProvider ServiceProvider()
    {
        var services = new ServiceCollection();
        
        var configuration = new ConfigurationManager();
        configuration.AddConfigurationDb<TestContext>(o => o.UseInMemoryDatabase("TestDb"));
        
        services.AddSingleton<IConfiguration>(configuration);
        
        services.AddDbContext<TestContext>(o => 
            o.UseInMemoryDatabase("TestDb")
                .AddInterceptors(new DbConfigurationInterceptor<TestContext>(configuration)));

        return services.BuildServiceProvider();
    }
    
}