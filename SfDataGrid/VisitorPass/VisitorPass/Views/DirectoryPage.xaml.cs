using Syncfusion.Maui.DataGrid;
using VisitorPass.Controls;
using VisitorPass.Models;
using VisitorPass.Services;
using VisitorPass.ViewModels;

namespace VisitorPass.Views;

/// <summary>
/// VP-051 / VP-052 — Directory page code-behind.
///
/// The XAML owns all layout. The code-behind handles two events:
///
///   - SfDataGrid.CellTapped on the visitor grid -> navigates to
///     the Digital Pass page with the tapped visitor's pass id as
///     a Shell query parameter (see <see cref="DashboardPage.VisitorIdQueryKey"/>).
///   - TopNavBar.ThemeToggleClicked -> forwards to
///     ThemeService.Toggle(); the nav bar's IsDarkMode is also
///     bound from the service so the sun/moon glyph flips.
///
/// Per HARNESS §6 rule 2 there is no business logic in this file —
/// it just routes events to the Shell and DI container.
/// </summary>
public partial class DirectoryPage : ContentPage
{
    private readonly ThemeService _theme;

    public DirectoryPage(DirectoryViewModel viewModel, ThemeService theme)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _theme = theme;

        if (this.FindTopNavBar() is { } navBar)
        {
            navBar.ThemeToggleClicked += OnThemeToggleClicked;
            navBar.IsDarkMode = _theme.IsDarkMode;
            _theme.PropertyChanged += OnThemePropertyChanged;
        }
    }

    private void OnThemeToggleClicked(object? sender, EventArgs e)
        => _theme.Toggle();

    private void OnThemePropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (this.FindTopNavBar() is { } navBar && e.PropertyName == nameof(ThemeService.IsDarkMode))
            navBar.IsDarkMode = _theme.IsDarkMode;
    }

    private async void OnCellTapped(object? sender, DataGridCellTappedEventArgs e)
    {
        // RowData carries the bound item; when it's a Visitor we
        // navigate to the details page with its PassId as a query
        // parameter so Phase 6's VisitorDetailsViewModel can load it.
        if (e.RowData is not Visitor visitor)
            return;

        await Shell.Current.GoToAsync(
            $"{VisitorDetailsViewModel.Route}?{DashboardPage.VisitorIdQueryKey}={Uri.EscapeDataString(visitor.PassId)}");
    }
}
