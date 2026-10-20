using ProductCatalogViewerApp.Models;

namespace ProductCatalogViewerApp.Services
{
    /// <summary>
    /// Contract for loading product data from the bundled sample asset.
    /// </summary>
    public interface IProductService
    {
        /// <summary>
        /// Loads all products from the bundled Products.json asset.
        /// </summary>
        /// <returns>List of validated products.</returns>
        Task<List<Product>> FetchProductsAsync();
    }
}
