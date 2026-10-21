using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Core.Hosting;
using Syncfusion.Maui.Toolkit.Hosting;
using VisitorPass.Services;

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

        // ----- Services (HARNESS §6 rule 1 — single source of truth) -----
        // IVisitorStore is the single source of truth for visitor records.
        // Registered as a SINGLETON so every page + ViewModel that resolves
        // it gets the same instance — mutations on one screen (e.g. a
        // Check In on the Digital Pass page) are immediately visible to
        // every other screen bound to the same collection.
        builder.Services.AddSingleton<IVisitorStore, VisitorStore>();

        // PassCodeGenerator is also a SINGLETON. It holds the per-day
        // running counter that hands out distinct VP-yyyyMMdd-#### codes.
        // A second instance would race the first one and mint duplicate
        // codes; singleton ensures every AddVisitor sees the same
        // counter state.
        builder.Services.AddSingleton<PassCodeGenerator>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
