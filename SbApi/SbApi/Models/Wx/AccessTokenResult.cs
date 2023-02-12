using System.Text.Json.Serialization;

namespace SbApi.Models.Wx;

public class AccessTokenResult
{
  [JsonPropertyName("errcode")] public int? ErrCode { get; set; }
  [JsonPropertyName("errmsg")] public string? ErrMsg { get; set; }
  [JsonPropertyName("access_token")] public string? AccessToken { get; set; }
  [JsonPropertyName("expires_in")] public int? ExpiresIn { get; set; }
}
