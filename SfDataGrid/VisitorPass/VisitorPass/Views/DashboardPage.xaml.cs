using Microsoft.Extensions.DependencyInjection;
using Syncfusion.Maui.DataGrid;
using VisitorPass.Controls;
using VisitorPass.Models;
using VisitorPass.Services;
using VisitorPass.ViewModels;

namespace VisitorPass.Views;

/// <summary>
/// VP-041 / VP-042 / VP-043 — Dashboard page code-behind.
///
/// The XAML is the single source of truth for layout; the code-behind
/// wires up three events:
///
///   1. TopNavBar.RegisterClicked -> routes to the registration
///      surface. Per VP-073 the container is chosen by idiom:
///      a Navigation.PushModalAsync to RegisterVisitorPage (the
///      platform renders the modal idiom-appropriate — centred modal
///      on desktop / tablet, full-screen slide-up sheet on phone).
///   2. TopNavBar.ThemeToggleClicked -> forwards to
///      ThemeService.Toggle(). The page also binds the nav bar's
///      IsDarkMode from the service so the sun/moon glyph flips.
///   3. SfDataGrid.CellTapped on the today's-activity grid ->
///      navigates to the Digital Pass page with the tapped visitor's
///      id.
///
/// Per HARNESS §6 rule 2 there is no business logic in this file —
/// it just routes events to the Shell and DI container.
/// </summary>
public partial class DashboardPage : ContentPage
{
    /// <summary>Query-parameter key used when navigating to the details page.</summary>
    public const string VisitorIdQueryKey = "passId";

    private readonly IServiceProvider _services;
    private readonly ThemeService _theme;

    public DashboardPage(
        DashboardViewModel viewModel,
        IServiceProvider services,
        ThemeService theme)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _services = services;
        _theme = theme;

        // The TopNavBar control lives inside the page's root layout;
        // walk the visual tree to find it and subscribe to its events.
        if (this.FindTopNavBar() is { } navBar)
        {
            navBar.RegisterClicked += OnRegisterClicked;
            navBar.ThemeToggleClicked += OnThemeToggleClicked;
            navBar.IsDarkMode = _theme.IsDarkMode;
            _theme.PropertyChanged += OnThemePropertyChanged;
        }
    }

    private void OnThemeToggleClicked(object? sender, EventArgs e)
        => _theme.Toggle();

    private void OnThemePropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        // Re-bind IsDarkMode so the sun/moon glyph flips on every
        // theme change. Subscribed in the constructor.
        if (this.FindTopNavBar() is { } navBar && e.PropertyName == nameof(ThemeService.IsDarkMode))
            navBar.IsDarkMode = _theme.IsDarkMode;
    }

    /// <summary>
    /// VP-073 — register trigger handler.
    ///
    /// Resolves a fresh <see cref="RegisterVisitorViewModel"/> from
    /// DI (transient, see MauiProgram.cs), wraps it in a
    /// <see cref="RegisterVisitorPage"/>, pushes the modal idiom-aware
    /// (centred modal on desktop / tablet, full-screen sheet on
    /// phone), wires up <see cref="RegisterVisitorPage.PassGenerated"/>
    /// to navigate to the Digital Pass page after a successful
    /// submission, and dismisses the modal afterwards.
    /// </summary>
    private async void OnRegisterClicked(object? sender, EventArgs e)
    {
        var vm = _services.GetRequiredService<RegisterVisitorViewModel>();
        var page = new RegisterVisitorPage(vm);

        // VP-074 — when Generate Pass succeeds, dismiss the modal
        // and navigate to the new visitor's Digital Pass page.
        page.PassGenerated += async (_, visitor) =>
        {
            await page.Navigation.PopModalAsync();
            await Shell.Current.GoToAsync(
                $"{VisitorDetailsViewModel.Route}?{VisitorIdQueryKey}={Uri.EscapeDataString(visitor.PassId)}");
        };
        page.DismissRequested += async (_, _) =>
            await page.Navigation.PopModalAsync();

        // Wrap in NavigationPage on desktop / tablet so the modal
        // renders with a navigation bar that respects the platform
        // idiom (centred modal on Mac Catalyst / Windows, top-bar
        // sheet on iPad / Android tablet). On phone, push as a
        // borderless full-screen modal — MAUI's default phone
        // modal slide-up animation applies.
        Page host = DeviceInfo.Idiom == DeviceIdiom.Phone
            ? page
            : new NavigationPage(page);

        await Navigation.PushModalAsync(host);
    }

    /// <summary>
    /// VP-043 — row tap handler on the today's-activity SfDataGrid.
    /// Routes to the Digital Pass page with the tapped visitor's id.
    /// </summary>
    private async void OnTodayGridCellTapped(object? sender, DataGridCellTappedEventArgs e)
    {
        if (e.RowData is not Visitor visitor)
            return;

        await Shell.Current.GoToAsync(
            $"{VisitorDetailsViewModel.Route}?{VisitorIdQueryKey}={Uri.EscapeDataString(visitor.PassId)}");
    }
}
