using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SS14.ConfigProvider.Model;


[PrimaryKey(nameof(Id)), Index(nameof(Name), IsUnique = true)]
public sealed class ConfigurationStore
{
    public int Id { get; init; }

    [Required, MaxLength(255)]
    public string Name { get; init; } = string.Empty;

    [MaxLength(2000)]
    public string? Value { get; set; }

    // TODO: Make this auto update?
    public DateTime UpdatedOn { get; init; } = DateTime.UtcNow;
}