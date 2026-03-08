using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SS14.ConfigProvider.Model;

namespace SS14.ConfigProvider;

/// <summary>
/// Provides configuration data from a database context implementing <see cref="IConfigDbContext"/>.
/// </summary>
/// <typeparam name="TContext">The type of the database context that implements <see cref="IConfigDbContext"/>.</typeparam>
[PublicAPI]
public sealed class DbConfigurationProvider<TContext> : ConfigurationProvider, IDisposable, IAsyncDisposable where TContext: DbContext, IConfigDbContext 
{
    private DbConfigurationSource<TContext> Source { get; }
    private readonly Timer? _timer;
    private readonly DbContextOptions<TContext> _contextOptions;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="DbConfigurationProvider{TContext}"/> class.
    /// </summary>
    /// <param name="source">The db configuration source to use</param>
    public DbConfigurationProvider(DbConfigurationSource<TContext> source)
    {
        Source = source;
        
        var contextBuilder = new DbContextOptionsBuilder<TContext>();
        source.OptionsAction?.Invoke(contextBuilder);
        _contextOptions = contextBuilder.Options;
        
        if (!Source.ReloadPeriodically) return;
        
        _timer = new Timer
        (
            callback: ReloadSettings,
            dueTime: TimeSpan.FromSeconds(10),
            period: TimeSpan.FromSeconds(Source.PeriodInSeconds),
            state: null
        );
    }

    /// <inheritdoc />
    public override void Load()
    {
        using var context = (TContext)Activator.CreateInstance(_contextOptions.ContextType, _contextOptions)!;
        
        Data = context.ConfigurationStore.ToDictionary<ConfigurationStore, string, string?>(
            c => c.Name, 
            c => c.Value, 
            StringComparer.OrdinalIgnoreCase);
        
    }

    /// <inheritdoc />
    public override void Set(string key, string? value)
    {
        base.Set(key, value);
        
        using var context = (TContext)Activator.CreateInstance(_contextOptions.ContextType, _contextOptions)!;

        var configurationValue = context.ConfigurationStore.SingleOrDefault(c => c.Name == key);

        configurationValue ??= new ConfigurationStore
        {
            Name = key,
            Value = value
        };

        context.ConfigurationStore.Update(configurationValue);
        context.SaveChanges();
    }

    // TODO: Handle removed configuration values
    /// <summary>
    /// Reloads the settings for the specified collection of keys from the configuration database.
    /// </summary>
    /// <param name="keys">The collection of keys whose corresponding settings should be reloaded.</param>
    public void ReloadSettings(IReadOnlyCollection<string> keys)
    {
        using var context = (TContext)Activator.CreateInstance(_contextOptions.ContextType, _contextOptions)!;
        
        var loadedSettings = context.ConfigurationStore
            .Where(c => keys.Contains(c.Name))
            .ToDictionary(c => c.Name, c => c.Value);

        foreach (var setting in loadedSettings)
        {
            Data[setting.Key] = setting.Value;
        }
        
        OnReload();
    }
    
    private void ReloadSettings(object? state)
    {
        Load();
        OnReload();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _timer?.Dispose();
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_timer != null) await _timer.DisposeAsync();
    }
}