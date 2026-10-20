using Microsoft.Extensions.Logging;
using StockChart.Services;
using StockChart.ViewModels;
using Syncfusion.Maui.Core.Hosting;
using Syncfusion.Licensing;
#if MAUI_DEVFLOW
using Microsoft.Maui.DevFlow.Agent;
#endif

namespace StockChart
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            SyncfusionLicenseProvider.RegisterLicense("YOUR_LICENSE_KEY");

            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureSyncfusionCore()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("MaterialIcons-Regular.ttf", "MaterialIcons");
                });

            builder.Services.AddSingleton<StockDataService>();
            builder.Services.AddTransient<StockChartViewModel>();

            return builder.Build();
        }
    }
}
