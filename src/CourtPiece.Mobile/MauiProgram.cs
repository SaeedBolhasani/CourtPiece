using CourtPiece.Common.Model;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace CourtPiece.Mobile
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
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                })


                ;


#if DEBUG
            builder.Logging.AddDebug();
#endif
            builder.Services.AddTransient<HttpClient>();

            builder.Services.AddTransient<PlayerService>();

            Routing.RegisterRoute("MainPage", typeof(MainPage));
            return builder.Build();
        }
    }
}
