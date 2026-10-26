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
            navBar.RegisterClicked += OnRegisterClicked;
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
        //if (route == DashboardViewModel.Route
        //    && Navigation.NavigationStack[^1].GetType() != typeof(DashboardPage))
        //{
        //    await Navigation.PushAsync(_services.GetRequiredService<DashboardPage>());
        //}
    }

    private async void OnRegisterClicked(object? sender, EventArgs e)
        => await OpenRegistrationAsync();

    private async void OnRegisterVisitor(object? sender, TappedEventArgs e)
        => await OpenRegistrationAsync();

    private async Task OpenRegistrationAsync()
    {
        var page = new RegisterVisitorPage(_services.GetRequiredService<RegisterVisitorViewModel>());
        page.DismissRequested += async (_, _) => await Navigation.PopAsync();
        page.PassGenerated += async (_, visitor) =>
        {
            await Navigation.PopAsync();
            await OpenVisitorDetailsAsync(visitor);
        };
        await Navigation.PushAsync(page);
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

        if (BindingContext is DirectoryViewModel vm)
            vm.SelectedVisitor = visitor;

        await OpenVisitorDetailsAsync(visitor);
    }

    /// <summary>
    /// "Open Full Pass Details" button on the Active Dossier
    /// preview card. Pushes the Digital Pass page with the
    /// currently selected visitor's pass id.
    /// </summary>
    private async void OnOpenFullPass(object? sender, EventArgs e)
    {
        if (BindingContext is not DirectoryViewModel vm
            || vm.SelectedVisitor is not { } visitor)
            return;

        await OpenVisitorDetailsAsync(visitor);
    }

    private Task OpenVisitorDetailsAsync(Visitor visitor)
        => Navigation.PushAsync(
            new VisitorDetailsPage(
                _services.GetRequiredService<VisitorDetailsViewModel>(),
                _theme,
                passId: visitor.PassId));
}
