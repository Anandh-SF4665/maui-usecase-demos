using Microsoft.Extensions.DependencyInjection;
using Syncfusion.Maui.DataGrid;
using VisitorPass.Controls;
using VisitorPass.Models;
using VisitorPass.Services;
using VisitorPass.ViewModels;

namespace VisitorPass.Views;

/// <summary>
/// VP-051 / VP-052 — Directory page code-behind.
///
/// Events:
///   - SfDataGrid.CellTapped on the visitor grid -> pushes
///     VisitorDetailsPage via Navigation.PushAsync (no Shell).
///   - TopNavBar.ThemeToggleClicked -> ThemeService.Toggle();
///     nav bar's IsDarkMode rebinds from the service.
///   - Sidebar.NavItemClicked -> pops back to root then pushes
///     Dashboard (no Shell).
///
/// Per HARNESS §6 rule 2 there is no business logic here.
/// </summary>
public partial class DirectoryPage : ContentPage
{
    private readonly IServiceProvider _services;
    private readonly ThemeService _theme;

    public DirectoryPage(
        DirectoryViewModel viewModel,
        IServiceProvider services,
        ThemeService theme)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _services = services;
        _theme = theme;

        if (this.FindTopNavBar() is { } navBar)
        {
            navBar.ThemeToggleClicked += OnThemeToggleClicked;
            navBar.IsDarkMode = _theme.IsDarkMode;
            _theme.PropertyChanged += OnThemePropertyChanged;
        }

        if (this.FindSideNavigation() is { } sidebar)
            sidebar.NavItemClicked += OnSidebarNavItemClicked;
    }

    private async void OnSidebarNavItemClicked(object? sender, string route)
    {
        if (route == DirectoryViewModel.Route)
            return; // already here
        // Pop back to root then push the target page so we don't
        // stack duplicates (uses Navigation, not Shell).
        await Navigation.PopToRootAsync();
        if (route == DashboardViewModel.Route
            && Navigation.NavigationStack[^1].GetType() != typeof(DashboardPage))
        {
            await Navigation.PushAsync(_services.GetRequiredService<DashboardPage>());
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
        if (e.RowData is not Visitor visitor)
            return;

        // Push the Digital Pass page directly via Navigation
        // (no Shell). The VM is a singleton — DI resolves the
        // same instance so any state set on the dashboard grid
        // (e.g. Check In from this page) is reflected here.
        await Navigation.PushAsync(
            new VisitorDetailsPage(
                _services.GetRequiredService<VisitorDetailsViewModel>(),
                _theme,
                passId: visitor.PassId));
    }
}
