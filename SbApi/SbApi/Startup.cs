using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using FluentScheduler;
using Microsoft.AspNetCore.HttpOverrides;
using RestSharp;
using SbApi.Models.Settings;
using SbApi.Utils.Caching;
using SbApi.Utils.Wx;

namespace SbApi;

public class Startup
{
  private readonly AppSettings _appSettings;


  public Startup(IConfiguration configuration)
  {
    _appSettings = configuration.Get<AppSettings>()!;
  }

  public void ConfigureServices(IServiceCollection services)
  {
    if (!Directory.Exists(_appSettings.DataBase.Directory))
      Directory.CreateDirectory(_appSettings.DataBase.Directory);


    services.AddControllersWithViews().AddJsonOptions(opt =>
    {
      opt.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
      opt.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
      opt.JsonSerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
    });

    services.AddRazorPages().AddJsonOptions(opt =>
    {
      opt.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
      opt.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
      opt.JsonSerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
    });

    // IIS配置
    services.Configure<IISServerOptions>(options => { options.AutomaticAuthentication = false; });

    // 转接头，代理
    services.Configure<ForwardedHeadersOptions>(options =>
    {
      options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    });

    //配置跨域
    services.AddCors(options =>
    {
      options.AddDefaultPolicy(b => b
        .SetIsOriginAllowedToAllowWildcardSubdomains()
        .WithOrigins(_appSettings.WithOrigins.ToArray())
        .WithMethods("GET", "POST", "OPTIONS")
        .AllowAnyHeader());
    });

    services.AddSingleton(HtmlEncoder.Create(UnicodeRanges.All));
    services.AddSingleton(_appSettings);
    services.AddSingleton(new RestClient(new HttpClient()));
    services.AddSingleton(new DefaultCachingContext(_appSettings));
    services.AddSingleton<DefaultCaching>();
    services.AddSingleton<BiliBiliCaching>();
    services.AddSingleton<WxJsSdk>();
  }

  public void Configure(IApplicationBuilder app, IWebHostEnvironment env, WxJsSdk wxJsSdk)
  {
    if (env.IsDevelopment()) app.UseDeveloperExceptionPage();

    app.UseCors();
    app.UseDefaultFiles();
    app.UseStaticFiles();

    app.UseForwardedHeaders();
    app.UseRouting();

    app.UseEndpoints(endpoints =>
    {
      endpoints.MapControllerRoute("default",
        "{controller=Home}/{action=Index}/{id?}");
    });

    JobManager.Initialize();

    async void Job()
    {
      var a = 0;
      while (!await wxJsSdk.RefreshAccessTokenAsync())
      {
        a++;
        if (a > 3) break;
      }

      var b = 0;
      while (!await wxJsSdk.RefreshJsApiTicketAsync())
      {
        b++;
        if (b > 3) break;
      }
    }

    JobManager.AddJob(
      Job,
      s => s.ToRunEvery(1).Hours()
    );
  }
}
