using VisitorPass.ViewModels;

namespace VisitorPass.Views;

/// <summary>
/// VP-030 — Directory screen (Screen 2) skeleton. Phase 5 fills in
/// the search box, status filter chips, and SfDataGrid.
/// </summary>
public partial class DirectoryPage : ContentPage
{
    public DirectoryPage(DirectoryViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
