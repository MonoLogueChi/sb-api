namespace SbApi.Models.Caching;

public class DefaultCachingTable
{
  public string Key { get; set; } = null!;
  public byte[] Value { get; set; } = null!;
  public DateTime DateTime { get; set; } = DateTime.UtcNow;
}
