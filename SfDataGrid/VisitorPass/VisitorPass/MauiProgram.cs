using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Core.Hosting;
using Syncfusion.Maui.Toolkit.Hosting;

namespace VisitorPass;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            // ----- Syncfusion hosts (HARNESS §3, Architecture Rule 5) -----
            // ConfigureSyncfusionCore loads the Syncfusion.Maui.Core
            // assembly, registers control themes, and makes
            // SfBadgeView / SfAvatarView / etc. available with the
            // MaterialLight palette pulled in from
            // App.xaml's SyncfusionThemeResourceDictionary.
            .ConfigureSyncfusionCore()
            // ConfigureSyncfusionToolkit loads the Syncfusion.Maui.Toolkit
            // assembly (SfStepProgressBar, SfChips, SfEffectsView, etc.).
            // Both calls are required so that every Syncfusion control
            // registered in Resources/Styles/Colors.xaml can resolve
            // its theme brushes at inflation time.
            .ConfigureSyncfusionToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
