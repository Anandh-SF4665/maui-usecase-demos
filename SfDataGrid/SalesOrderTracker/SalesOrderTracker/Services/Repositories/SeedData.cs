using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SalesOrderTracker.Contracts;
using SalesOrderTracker.Services.Storage;

namespace SalesOrderTracker.Services.Repositories
{
    public static class SeedData
    {
        /// <summary>
        /// Ensures the local database is seeded with sample orders so the
        /// SfDataGrid showcase has content on first launch.
        /// </summary>
        public static async Task EnsureSeedAsync(IServiceProvider services, int limit = 50, bool force = true)
        {
            try
            {
                using var scope = services.CreateScope();
                var svc = scope.ServiceProvider;
                var repo = svc.GetService<IOrderRepository>();
                var db = svc.GetService<Database>();

                if (force && db != null)
                {
                    try
                    {
                        await db.Connection.ExecuteAsync("DELETE FROM LineItem");
                        await db.Connection.ExecuteAsync("DELETE FROM OrderStatusEntry");
                        await db.Connection.ExecuteAsync("DELETE FROM [Order]");
                        await db.Connection.ExecuteAsync("DELETE FROM Customer");
                    }
                    catch
                    {
                        // best-effort: only some tables may exist on first run
                    }
                }

                if (repo != null)
                {
                    await repo.SeedSampleDataAsync();
                }
            }
            catch
            {
                // swallow seed errors at startup
            }
        }
    }
}
