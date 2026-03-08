using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using SS14.ConfigProvider.Model;

namespace SS14.ConfigProvider;

/// <summary>
/// This interceptor tracks changes to the configuration store and causes the
/// <see cref="DbConfigurationProvider{TContext}"/> to reload the changed configuration values.
/// </summary>
/// <code lang="csharp">
/// // In a DbContext class implementing IConfigDbContext
/// protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
/// {
///     optionsBuilder.AddInterceptors(new DbConfigurationInterceptor(this));
/// }
/// </code>
public class DbConfigurationInterceptor<TContext> : ISaveChangesInterceptor
    where TContext : DbContext, IConfigDbContext
{
    private readonly IConfiguration _configuration;
    private readonly ConcurrentBag<string> _modifiedConfigurations = [];

    /// <summary>
    /// Intercepts database operations to track changes in configurations, ensuring that modified
    /// configuration values in the <see cref="IConfigDbContext"/> are handled appropriately.
    /// </summary>
    public DbConfigurationInterceptor(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <inheritdoc />
    public InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        HandleChangesSaving(eventData);
        return result;
    }

    /// <inheritdoc />
    public ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result,
        CancellationToken cancellationToken = new())
    {
        HandleChangesSaving(eventData);
        return new ValueTask<InterceptionResult<int>>(result);
    }
    
    /// <inheritdoc />
    public void SaveChangesFailed(DbContextErrorEventData eventData)
    {
        _modifiedConfigurations.Clear();
    }
    

    /// <inheritdoc />
    public Task SaveChangesFailedAsync(DbContextErrorEventData eventData,
        CancellationToken cancellationToken = new())
    {
        _modifiedConfigurations.Clear();
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public void SaveChangesCanceled(DbContextEventData eventData)
    {
        _modifiedConfigurations.Clear();
    }

    /// <inheritdoc />
    public Task SaveChangesCanceledAsync(DbContextEventData eventData,
        CancellationToken cancellationToken = new())
    {
        _modifiedConfigurations.Clear();
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        HandleChangesSaved();
        return result;
    }

    /// <inheritdoc />
    public ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result,
        CancellationToken cancellationToken = new())
    {
        HandleChangesSaved();
        return  new ValueTask<int>(result);
    }

    private void HandleChangesSaving(DbContextEventData eventData)
    {
        if (eventData.Context == null)
            return;
        
        var entries = eventData.Context.ChangeTracker
            .Entries<ConfigurationStore>()
            .Where(e => e.State != EntityState.Unchanged && e.State != EntityState.Detached)
            .Select(e => e.Entity.Name);
        
        foreach (var entry in entries)
        {
            if (!_modifiedConfigurations.Contains(entry)) 
                _modifiedConfigurations.Add(entry);
        }
    }
    
    private void HandleChangesSaved()
    {
        if (_modifiedConfigurations.IsEmpty)
            return;

        GetProvider().ReloadSettings(_modifiedConfigurations);
        _modifiedConfigurations.Clear();
    }

    private DbConfigurationProvider<TContext> GetProvider()
    {
        var configRoot = _configuration as IConfigurationRoot;
        var provide = configRoot?.Providers
            .FirstOrDefault(p => p is DbConfigurationProvider<TContext>);

        return provide as DbConfigurationProvider<TContext> 
                    ?? throw new InvalidOperationException("The DbConfigurationProvider<TContext> is not registered in the configuration.");
    }
}