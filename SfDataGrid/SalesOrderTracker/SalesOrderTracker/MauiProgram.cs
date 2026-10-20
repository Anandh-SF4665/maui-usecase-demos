using Microsoft.Extensions.Logging;
using SalesOrderTracker.Services.Logging;
using Microsoft.Extensions.DependencyInjection;
using SalesOrderTracker.Contracts;
using SalesOrderTracker.Services.Repositories;
using SalesOrderTracker.Services.Storage;
using Syncfusion.Maui.Core.Hosting;

namespace SalesOrderTracker
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureSyncfusionCore()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Register services and repositories
            var dbPath = System.IO.Path.Combine(FileSystem.AppDataDirectory, "orders.db");
            builder.Services.AddSingleton(sp => new Database(dbPath));
            builder.Services.AddSingleton<IOrderRepository, SqliteOrderRepository>();

            // Register ViewModels and Pages for DI injection
            builder.Services.AddTransient<ViewModels.OrderListViewModel>();
            builder.Services.AddTransient<ViewModels.OrderDetailsViewModel>();
            builder.Services.AddTransient<Views.OrderListPage>();
            builder.Services.AddTransient<Views.OrderDetailsPage>();

            // Configure application logging
            builder.Logging.AddAppLogging();
            // also enable debug output for startup tracing
            builder.Logging.AddDebug();

            var app = builder.Build();

            // Log startup trace if logger available
            try
            {
                var logger = app.Services.GetService<ILoggerFactory>()?.CreateLogger("Startup");
                logger?.LogInformation("Maui app built — starting startup sequence");
            }
            catch
            {
                // ignore logging failures
            }

            // expose DI container for pages/viewmodels created manually
            App.Services = app.Services;

            // Initialize DB and seed local sample data asynchronously — NEVER block the UI thread
            var services = app.Services;
            Task.Run(async () =>
            {
                try
                {
                    var db = services.GetRequiredService<Database>();
                    await db.InitAsync();
                    try
                    {
                        await SeedData.EnsureSeedAsync(services, limit: 50, force: true);
                    }
                    catch
                    {
                        // best-effort: seeding failures shouldn't block runs
                    }
                }
                catch
                {
                    // startup failures shouldn't block runs
                }
            });

            return app;
        }
    }
}
