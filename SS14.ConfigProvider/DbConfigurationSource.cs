using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SS14.ConfigProvider.Model;

namespace SS14.ConfigProvider;

/// <summary>
/// The configuration source used with <see cref="DbConfigurationProvider{TContext}"/>.
/// Provide a db configuration using <see cref="OptionsAction"/>
/// </summary>
/// <inheritdoc cref="OptionsAction"/>
/// <typeparam name="TContext">The database context type implementing <see cref="IConfigDbContext"/></typeparam>
[PublicAPI]
public sealed class DbConfigurationSource<TContext> : IConfigurationSource where TContext : DbContext, IConfigDbContext
{
    /// <summary>
    /// Configure the db context options for this db configuration source.
    /// </summary>
    /// <code language="csharp">
    ///     // Example using an in memory database
    ///     new DbConfigurationSource&lt;TContext&gt;
    ///     {
    ///         OptionsAction = b => b.UseInMemoryDatabase("TestDb")
    ///     });
    /// </code>
    public required Action<DbContextOptionsBuilder> OptionsAction { get; init;}
    
    /// <summary>
    /// Determines if the provider should reload the configuration periodically.
    /// </summary>
    public bool ReloadPeriodically { get; init; }

    /// <summary>
    /// The reload interval in seconds.
    /// </summary>
    public int PeriodInSeconds { get; init; } = 5;

    /// <inheritdoc />
    public IConfigurationProvider Build(IConfigurationBuilder builder) =>
        new DbConfigurationProvider<TContext>(this);
}