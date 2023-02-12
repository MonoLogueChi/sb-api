using Microsoft.AspNetCore.Mvc;
using SbApi.Models.Protos.Wx;
using SbApi.Models.Settings;
using SbApi.Utils.Wx;

namespace SbApi.Controllers.Api.Wx;

[Route("/api/wx/share")]
[ApiController]
public class WxShareController : ControllerBase
{
  private readonly AppSettings _appSettings;
  private readonly WxJsSdk _wxSdk;

  public WxShareController(AppSettings appSettings, WxJsSdk wxSdk)
  {
    _wxSdk = wxSdk;
    _appSettings = appSettings;
  }

  [HttpPost("signature")]
  public async Task<SignPackage?> Signature([FromBody] SignPackageRequest r)
  {
    var url = r.Url;
    if (!string.IsNullOrEmpty(url))
    {
      var uri = new Uri(url);
      if (_appSettings.IsInWhiteListDomains(uri.Host))
      {
        var a = await _wxSdk.GetSignPackageAsync(url);
        if (a != null) return a;
      }
    }

    return null;
  }

  [HttpGet("redirect")]
  public ActionResult RedirectUrl(string? url)
  {
    if (!string.IsNullOrEmpty(url))
    {
      var uri = new Uri(url);
      if (_appSettings.IsInWhiteListDomains(uri.Host))
        return Redirect(uri.AbsoluteUri);
    }

    return NotFound();
  }
}
