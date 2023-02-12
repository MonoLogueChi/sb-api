using System.Text;
using Google.Protobuf;
using LiteDB.Async;
using SbApi.Models.Caching;
using static WebApiProtobufFormatter.Utils.Serialize;

namespace SbApi.Utils.Caching;

public class DefaultCaching
{
  private readonly ILiteCollectionAsync<DefaultCachingTable> _caching;

  public DefaultCaching(DefaultCachingContext context)
  {
    _caching = context.DefaultCaching;
  }

  #region Protobuf

  /// <summary>
  ///   获取或设置缓存，并使用protobuf序列化和反序列化
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <param name="key"></param>
  /// <param name="factory"></param>
  /// <param name="expiration"></param>
  /// <returns></returns>
  /// <exception cref="ArgumentOutOfRangeException"></exception>
  /// <exception cref="TypeAccessException"></exception>
  public async Task<T?> GetOrSetAsync<T>(string key, Func<Task<T?>> factory,
    TimeSpan expiration) where T : IMessage<T>
  {
    var type = typeof(T);
    var o = Activator.CreateInstance(type);
    var i = type.GetProperty("Parser")?.GetValue(o);
    if (i is MessageParser<T> m)
    {
      var a = await GetOrSetAsync(key, async () =>
      {
        var b = await factory.Invoke();
        return b.ToByteArray();
      }, expiration);

      if (a is { Length: > 0 })
        return Parse(a, m);
      throw new ArgumentOutOfRangeException();
    }

    throw new TypeAccessException(type.FullName);
  }

  /// <summary>
  ///   设置缓存 protobuf
  /// </summary>
  /// <param name="key"></param>
  /// <param name="value"></param>
  /// <returns></returns>
  public async Task<bool> SetAsync<T>(string key, T? value) where T : IMessage<T>
  {
    if (value != null) return await SetAsync(key, value.ToByteArray());
    return false;
  }

  #endregion

  #region 通用

  /// <summary>
  ///   获取或设置缓存
  /// </summary>
  /// <param name="key"></param>
  /// <param name="factory"></param>
  /// <param name="expiration"></param>
  /// <returns></returns>
  public async Task<byte[]> GetOrSetAsync(string key, Func<Task<byte[]>> factory,
    TimeSpan expiration)
  {
    var a = await _caching.Query().Where(x => x.Key == key).FirstOrDefaultAsync();
    if (a is { Value.Length: > 0 } && a.DateTime.Add(expiration) > DateTime.UtcNow) return a.Value;

    var b = await factory.Invoke();
    if (b is { Length: > 0 } && await SetAsync(key, b))
      return b;

    return Array.Empty<byte>();
  }

  /// <summary>
  ///   设置缓存
  /// </summary>
  /// <param name="key"></param>
  /// <param name="data"></param>
  /// <returns></returns>
  public async Task<bool> SetAsync(string key, byte[] data)
  {
    if (!string.IsNullOrWhiteSpace(key) && data is { Length: > 0 })
      return await _caching.UpsertAsync(new DefaultCachingTable
        { Key = key, Value = data, DateTime = DateTime.UtcNow });
    return false;
  }

  /// <summary>
  ///   获取或设置缓存
  /// </summary>
  /// <param name="key"></param>
  /// <param name="factory"></param>
  /// <param name="converter"></param>
  /// <param name="expiration"></param>
  /// <returns></returns>
  public async Task<T?> GetOrSetAsync<T>(string key,
    Func<Task<byte[]>> factory,
    Func<byte[], Task<T?>> converter,
    TimeSpan expiration)
  {
    var a = await GetOrSetAsync(key, async () => await factory.Invoke(), expiration);
    return await converter(a);
  }

  #endregion

  #region String

  /// <summary>
  ///   获取或设置缓存，string类型
  /// </summary>
  /// <param name="key"></param>
  /// <param name="factory"></param>
  /// <param name="expiration"></param>
  /// <returns></returns>
  public async Task<string> GetOrSetAsync(string key, Func<Task<string>> factory,
    TimeSpan expiration)
  {
    var a = await GetOrSetAsync(key, async () =>
    {
      var b = await factory.Invoke();
      return Encoding.UTF8.GetBytes(b);
    }, expiration);
    return Encoding.UTF8.GetString(a);
  }

  /// <summary>
  ///   设置缓存 string类
  /// </summary>
  /// <param name="key"></param>
  /// <param name="value"></param>
  /// <returns></returns>
  public async Task<bool> SetAsync(string key, string value)
  {
    if (!string.IsNullOrWhiteSpace(value)) return await SetAsync(key, Encoding.UTF8.GetBytes(value));
    return false;
  }

  #endregion

  #region int

  /// <summary>
  ///   获取或设置缓存，int类型
  /// </summary>
  /// <param name="key"></param>
  /// <param name="factory"></param>
  /// <param name="expiration"></param>
  /// <returns></returns>
  public async Task<int> GetOrSetAsync(string key, Func<Task<int>> factory,
    TimeSpan expiration)
  {
    var a = await GetOrSetAsync(key, async () =>
    {
      var b = await factory.Invoke();
      return BitConverter.GetBytes(b);
    }, expiration);
    return BitConverter.ToInt32(a);
  }

  /// <summary>
  ///   设置缓存 int类
  /// </summary>
  /// <param name="key"></param>
  /// <param name="value"></param>
  /// <returns></returns>
  public async Task<bool> SetAsync(string key, int value)
  {
    return await SetAsync(key, BitConverter.GetBytes(value));
  }

  #endregion
}
