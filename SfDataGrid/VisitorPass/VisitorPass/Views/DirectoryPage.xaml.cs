using Syncfusion.Maui.DataGrid;
using VisitorPass.Models;
using VisitorPass.ViewModels;

namespace VisitorPass.Views;

/// <summary>
/// VP-051 / VP-052 — Directory page code-behind.
///
/// The XAML owns all layout. The code-behind handles exactly one
/// event:
///
///   - <see cref="SfDataGrid.CellTapped"/> on the visitor grid ->
///     navigates to the Digital Pass page with the tapped visitor's
///     pass id as a Shell query parameter (see
///     <see cref="DashboardPage.VisitorIdQueryKey"/>).
///
/// Per HARNESS §6 rule 2 there is no business logic in this file —
/// it just routes the row-tap event to the Shell.
/// </summary>
public partial class DirectoryPage : ContentPage
{
    public DirectoryPage(DirectoryViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private async void OnCellTapped(object? sender, DataGridCellTappedEventArgs e)
    {
        // RowData carries the bound item; when it's a Visitor we
        // navigate to the details page with its PassId as a query
        // parameter so Phase 6's VisitorDetailsViewModel can load it.
        if (e.RowData is not Visitor visitor)
            return;

        // SelectionController already moves the selection cursor to
        // the tapped row; we just issue the Shell navigation.
        // VisitorDetailsPage.Route ("details") is hard-coded here for
        // now — Phase 6 adds the Route constant on that page.
        await Shell.Current.GoToAsync(
            $"details?{DashboardPage.VisitorIdQueryKey}={Uri.EscapeDataString(visitor.PassId)}");
    }
}
