namespace ProductCatalogViewerApp.Models
{
    /// <summary>
    /// Represents a product in the catalog loaded from the bundled sample data.
    /// </summary>
    public class Product
    {
        /// <summary>Unique identifier.</summary>
        public int Id { get; set; }

        /// <summary>Product name displayed in the DataGrid.</summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>Category for filtering; normalized to lowercase/trimmed.</summary>
        public string Category { get; set; } = string.Empty;

        /// <summary>Currency value, displayed with 2 decimals.</summary>
        public decimal Price { get; set; }

        /// <summary>Detailed text shown on the Details page.</summary>
        public string? Description { get; set; }

        /// <summary>Product image URL displayed on the Details page.</summary>
        public string? Image { get; set; }

        /// <summary>User rating (0-5 scale).</summary>
        public decimal? Rating { get; set; }

        /// <summary>Number of ratings.</summary>
        public int? RatingCount { get; set; }

        /// <summary>Available stock quantity.</summary>
        public int? Stock { get; set; }
    }
}
