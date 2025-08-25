using System.Globalization;
using Microsoft.Extensions.Logging;

namespace SnapRead
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
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo(Preferences.Get("AppLanguage", CultureInfo.CurrentUICulture.Name));

            return builder.Build();
        }
    }
}