using Microsoft.EntityFrameworkCore;
using SS14.ConfigProvider.Model;

namespace SS14.ConfigProvider.Tests;

public class TestContext : DbContext, IConfigDbContext
{
    public DbSet<ConfigurationStore> ConfigurationStore { get; set; }

    public TestContext(DbContextOptions<TestContext> options) : base(options) {}
}