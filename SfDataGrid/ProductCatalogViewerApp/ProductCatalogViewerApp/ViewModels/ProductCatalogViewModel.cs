using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProductCatalogViewerApp.Models;
using ProductCatalogViewerApp.Services;

namespace ProductCatalogViewerApp.ViewModels
{
    /// <summary>
    /// ViewModel for the Product Catalog page.
    /// Manages loading, filtering, sorting, and pagination of products.
    /// </summary>
    public partial class ProductCatalogViewModel : ObservableObject
    {
        private readonly IProductService _productService;

        // ─── Immutable source list ────────────────────────────────────────────
        private List<Product> _allProducts = [];

        // ─── Observable Properties ────────────────────────────────────────────

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string? _errorMessage;

        [ObservableProperty]
        private string _searchTerm = string.Empty;

        [ObservableProperty]
        private string _selectedCategory = "All";

        [ObservableProperty]
        private string? _sortColumn;

        [ObservableProperty]
        private SortDirection _sortDirection = SortDirection.Ascending;

        // Paging
        [ObservableProperty]
        private int _currentPageIndex;

        [ObservableProperty]
        private int _totalPages = 1;

        [ObservableProperty]
        private int _totalFilteredCount;

        public const int PageSize = 30;

        /// <summary>Categories derived from loaded products (with "All" prepended).</summary>
        public ObservableCollection<string> Categories { get; } = new();

        /// <summary>Products visible in the DataGrid (current page after filter/sort).</summary>
        public ObservableCollection<Product> FilteredProducts { get; } = new();

        // ─── Constructor ──────────────────────────────────────────────────────

        public ProductCatalogViewModel(IProductService productService)
        {
            _productService = productService;
        }

        // ─── Commands ─────────────────────────────────────────────────────────

        /// <summary>Loads products from the bundled sample asset on first appearance.</summary>
        [RelayCommand]
        public async Task LoadProductsAsync()
        {
            if (_allProducts.Count > 0) return; // Already loaded this session

            IsLoading = true;
            ErrorMessage = null;

            try
            {
                var products = await _productService.FetchProductsAsync();

                _allProducts = products;

                // Extract and sort categories
                var distinctCategories = products
                    .Select(p => p.Category)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(c => c)
                    .ToList();

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Categories.Clear();
                    Categories.Add("All");
                    foreach (var cat in distinctCategories)
                        Categories.Add(cat);
                });

                ApplyFiltersAndSort();
            }
            catch (System.Text.Json.JsonException)
            {
                ErrorMessage = "Invalid product data. Please reinstall the sample.";
            }
            catch (Exception)
            {
                ErrorMessage = "An unexpected error occurred while loading products.";
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Forces a reload from the bundled sample asset (e.g., on retry).
        /// </summary>
        [RelayCommand]
        public async Task ReloadProductsAsync()
        {
            _allProducts = [];
            CurrentPageIndex = 0;
            await LoadProductsAsync();
        }

        /// <summary>Updates the search term and re-applies filters.</summary>
        [RelayCommand]
        public void UpdateSearch(string searchTerm)
        {
            SearchTerm = searchTerm ?? string.Empty;
            CurrentPageIndex = 0;
            ApplyFiltersAndSort();
        }

        /// <summary>Updates the category filter and re-applies filters.</summary>
        [RelayCommand]
        public void FilterByCategory(string category)
        {
            SelectedCategory = category ?? "All";
            CurrentPageIndex = 0;
            ApplyFiltersAndSort();
        }



        // ─── Core filter/sort/pagination logic ───────────────────────────────

        /// <summary>
        /// Filters AllProducts by search term and category, applies sort.
        /// SfDataPager handles pagination. Updates FilteredProducts with ALL filtered results.
        /// </summary>
        public void ApplyFiltersAndSort()
        {
            IEnumerable<Product> result = _allProducts;

            // 1. Category filter
            if (!string.IsNullOrEmpty(SelectedCategory) && SelectedCategory != "All")
            {
                result = result.Where(p =>
                    string.Equals(p.Category, SelectedCategory, StringComparison.OrdinalIgnoreCase));
            }

            // 2. Search filter
            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                result = result.Where(p =>
                    p.Title.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase));
            }

            // 3. Sort
            if (SortColumn == "Title")
            {
                result = SortDirection == SortDirection.Ascending
                    ? result.OrderBy(p => p.Title, StringComparer.OrdinalIgnoreCase)
                    : result.OrderByDescending(p => p.Title, StringComparer.OrdinalIgnoreCase);
            }
            else if (SortColumn == "Price")
            {
                result = SortDirection == SortDirection.Ascending
                    ? result.OrderBy(p => p.Price)
                    : result.OrderByDescending(p => p.Price);
            }

            var filtered = result.ToList();
            TotalFilteredCount = filtered.Count;

            // 4. Calculate total pages (for reference)
            int pages = filtered.Count == 0 ? 1 : (int)Math.Ceiling((double)filtered.Count / PageSize);
            TotalPages = pages;

            // 5. Reset page index when filters change
            CurrentPageIndex = 0;

            // 6. Update UI on main thread with ALL filtered products
            // SfDataPager will handle pagination internally
            MainThread.BeginInvokeOnMainThread(() =>
            {
                FilteredProducts.Clear();
                foreach (var product in filtered)
                    FilteredProducts.Add(product);
            });
        }

        // ─── Derived display properties ───────────────────────────────────────

        /// <summary>True when paging controls should be visible (> PageSize products).</summary>
        public bool IsPagingVisible => TotalFilteredCount > PageSize;

        partial void OnTotalFilteredCountChanged(int value)
        {
            OnPropertyChanged(nameof(IsPagingVisible));
        }
    }

    /// <summary>Sort direction enum for column sorting.</summary>
    public enum SortDirection
    {
        Ascending,
        Descending
    }
}
