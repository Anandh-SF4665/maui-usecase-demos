using System;
using System.IO;
using System.Threading.Tasks;

namespace PatientCompanion.Services;

/// <summary>
/// Loads the bundled world-map JSON asset from the app package at startup so
/// it can be assigned to SfMaps ShapesSource synchronously without async-void
/// in event handlers.
/// </summary>
public class WorldMapAssetService
{
    public const string AssetName = "world-map.json";

    private readonly Task<Stream?> loadTask;

    public WorldMapAssetService()
    {
        loadTask = LoadAsync();
    }

    public Stream? ShapesStream => loadTask.IsCompletedSuccessfully
        ? loadTask.Result
        : null;

    private static async Task<Stream?> LoadAsync()
    {
        try
        {
            return await FileSystem.OpenAppPackageFileAsync(AssetName);
        }
        catch
        {
            return null;
        }
    }

    public async Task<Stream?> GetStreamAsync()
    {
        return await loadTask;
    }
}