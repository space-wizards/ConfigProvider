using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SS14.ConfigProvider.Model;

/// <summary>
/// Represents a configuration storage entity that holds key-value pairs of configuration data.
/// </summary>
[PrimaryKey(nameof(Id)), Index(nameof(Name), IsUnique = true)]
public sealed class ConfigurationStore
{
    /// <summary>
    /// The unique identifier for the configuration entry.   
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// The name of the configuration entry. Must be unique and not exceed 255 characters.
    /// </summary>
    [Required, MaxLength(255)]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// The value of the configuration entry. Can be null.
    /// </summary>
    [MaxLength(2000)]
    public string? Value { get; set; }

    /// <summary>
    /// The date and time when the configuration entry was last updated.
    /// </summary>
    // TODO: Make this auto update?
    public DateTime UpdatedOn { get; init; } = DateTime.UtcNow;
}