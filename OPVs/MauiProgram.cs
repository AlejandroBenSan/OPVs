using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OPVs.Servicios;

namespace OPVs
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder
            .Configuration
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            builder.Services.AddSingleton<ApiService>();

            builder.Services.AddMauiBlazorWebView();

            builder.Services.AddBlazorBootstrap();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif

            builder.Services.AddHttpClient<OpvService>();

            return builder.Build();
        }
    }
}
