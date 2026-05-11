using System.ComponentModel.DataAnnotations;

namespace Main.Application.Models.Cache;

public class CacheSettings
{
    [Required]
    public required UserCacheSettings User { get; init; }
}

public record CacheSetting
{
    [Required]
    public TimeSpan Duration { get; init; }
}