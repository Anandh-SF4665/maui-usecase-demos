using Microsoft.Extensions.Logging;
using PatientCare.Services;
using Syncfusion.Maui.Core.Hosting;

namespace PatientCare
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureSyncfusionCore();

            builder.Services.AddSingleton<DashboardDataService>();
            builder.Services.AddSingleton<NotificationService>();
            builder.Services.AddSingleton<PatientDataService>();
            builder.Services.AddSingleton<ScheduleDataService>();
            builder.Services.AddSingleton<CarePlanDataService>();

            builder
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    // App icons use the bundled Google Material Icons font directly.
                    fonts.AddFont("MaterialIcons-Regular.ttf", "MaterialIcons");
                    // MauiMaterialAssets.ttf is not in this repo: it ships inside the Syncfusion.Maui.Core NuGet
                    // package (buildTransitive/Resources/Fonts/MauiMaterialAssets.ttf) and is registered as a
                    // MauiFont automatically by that package's props file. It is only needed for Syncfusion controls'
                    // own icons.
                    fonts.AddFont("MauiMaterialAssets.ttf", "MauiMaterialAssets");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}

