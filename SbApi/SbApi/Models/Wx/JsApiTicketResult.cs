using System.Text.Json.Serialization;

namespace SbApi.Models.Wx;

public class JsApiTicketResult
{
  [JsonPropertyName("errcode")] public int? ErrCode { get; set; }
  [JsonPropertyName("errmsg")] public string? ErrMsg { get; set; }
  [JsonPropertyName("ticket")] public string? Ticket { get; set; }
  [JsonPropertyName("expires_in")] public int? ExpiresIn { get; set; }
}
