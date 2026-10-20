using System.Text.Json;
using System.Text.Json.Serialization;
using ProductCatalogViewerApp.Models;

namespace ProductCatalogViewerApp.Services
{
    /// <summary>
    /// Loads sample products from a bundled JSON asset shipped with the app
    /// so the SfDataGrid showcase runs fully offline with deterministic data.
    /// </summary>
    public class ProductService : IProductService
    {
        private const string ProductsAsset = "Products.json";

        private readonly JsonSerializerOptions _jsonOptions;

        public ProductService()
        {
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                NumberHandling = JsonNumberHandling.AllowReadingFromString
            };
        }

        /// <inheritdoc />
        public async Task<List<Product>> FetchProductsAsync()
        {
            await using var stream = await FileSystem.OpenAppPackageFileAsync(ProductsAsset);
            var products = await JsonSerializer.DeserializeAsync<List<Product>>(stream, _jsonOptions)
                ?? throw new JsonException("Deserialized null from Products.json asset.");

            return products
                .Where(IsValid)
                .Select(Normalize)
                .ToList();
        }

        private static bool IsValid(Product p) =>
            !string.IsNullOrWhiteSpace(p.Title) &&
            !string.IsNullOrWhiteSpace(p.Category) &&
            p.Price >= 0;

        private static Product Normalize(Product p) => new()
        {
            Id = p.Id,
            Title = p.Title.Trim(),
            Category = p.Category.Trim().ToLowerInvariant(),
            Price = p.Price,
            Description = p.Description,
            Image = string.IsNullOrWhiteSpace(p.Image) ? null : p.Image,
            Rating = p.Rating is >= 0 and <= 5 ? p.Rating : null,
            RatingCount = p.RatingCount,
            Stock = p.Stock
        };
    }
}
