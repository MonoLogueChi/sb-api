using LiteDB.Async;
using SbApi.Models.Caching;
using SbApi.Models.Settings;

namespace SbApi.Utils.Caching;

public class DefaultCachingContext
{
  public ILiteCollectionAsync<DefaultCachingTable> BiliBiliPagesCaching;
  public ILiteStorageAsync<int> BiliDanMuCache;
  public ILiteCollectionAsync<DefaultCachingTable> DefaultCaching;

  public DefaultCachingContext(AppSettings appSettings)
  {
    Database = new LiteDatabaseAsync(Path.Combine(appSettings.DataBase.Directory, appSettings.DataBase.CachingDb));
    DefaultCaching = Database.GetCollection<DefaultCachingTable>("DefaultCaching");
    BiliBiliPagesCaching = Database.GetCollection<DefaultCachingTable>("BiliPages");
    BiliDanMuCache = Database.GetStorage<int>("BiliDanMu");

    DefaultCaching.EnsureIndexAsync(x => x.Key);
  }

  public LiteDatabaseAsync Database { get; }
}
