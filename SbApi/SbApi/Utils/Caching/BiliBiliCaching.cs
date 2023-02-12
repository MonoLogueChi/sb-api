using Google.Protobuf;
using LiteDB.Async;
using SbApi.Models.Caching;
using SbApi.Models.Protos.BiliBili.Pages;
using static WebApiProtobufFormatter.Utils.Serialize;

namespace SbApi.Utils.Caching;

public class BiliBiliCaching
{
  private readonly ILiteStorageAsync<int> _dmCaching;
  private readonly ILiteCollectionAsync<DefaultCachingTable> _pagesCaching;

  public BiliBiliCaching(DefaultCachingContext context)
  {
    _pagesCaching = context.BiliBiliPagesCaching;
    _dmCaching = context.BiliDanMuCache;
  }

  public async Task<BiliBiliPages?> PagesGetOrSetAsync(string key, Func<Task<BiliBiliPages>> factory,
    TimeSpan expiration)
  {
    var a = await _pagesCaching.Query().Where(x => x.Key == key).FirstOrDefaultAsync();

    if (a is { Value.Length: > 0 } && a.DateTime.Add(expiration) > DateTime.UtcNow)
      return Parse(a.Value, BiliBiliPages.Parser);

    var b = await factory.Invoke();

    if (b is { Data.Count: > 0 })
      await _pagesCaching.UpsertAsync(new DefaultCachingTable
      {
        Key = key,
        Value = b.ToByteArray(),
        DateTime = DateTime.UtcNow
      });
    else
      b = Parse(a.Value, BiliBiliPages.Parser);

    return b;
  }

  public async Task<Stream?> DmGetOrSetAsync(int key, Func<Task<Stream?>> factory,
    TimeSpan expiration)
  {
    var a = await _dmCaching.FindByIdAsync(key);
    if (a != null && a.UploadDate.Add(expiration) > DateTime.UtcNow)
    {
      var b = await _dmCaching.FindByIdAsync(key);

      if (b != null)
      {
        var ms = new MemoryStream();
        b.CopyTo(ms);
        return ms;
      }
    }

    var f = await factory.Invoke();
    if (f != null)
    {
      f.Position = 0;
      await _dmCaching.UploadAsync(key, key.ToString(), f);
    }

    return f;
  }
}
