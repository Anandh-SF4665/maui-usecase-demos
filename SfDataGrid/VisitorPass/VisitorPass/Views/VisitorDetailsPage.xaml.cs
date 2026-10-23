using VisitorPass.Controls;
using VisitorPass.Services;
using VisitorPass.ViewModels;

namespace VisitorPass.Views;

/// <summary>
/// VP-060..064 — Digital Pass / Visitor Details (Screen 3) page.
///
/// The XAML owns the layout (identity card, pass card, timeline,
/// action buttons). The code-behind implements the Shell navigation
/// lifecycle hooks plus the theme-toggle wiring (VP-080):
///
///   - ApplyQueryAttributes -> forwards the passId to the VM.
///   - OnAppearing -> triggers LoadAsync so the VM resolves the
///     visitor from IVisitorStore and rebuilds the timeline.
///   - OnNavigatedFrom (when popping back) -> clears the VM so the
///     next push of this route doesn't briefly show stale data.
///   - TopNavBar.ThemeToggleClicked -> forwards to
///     ThemeService.Toggle(); the nav bar's IsDarkMode is also
///     bound from the service so the sun/moon glyph flips.
///
/// Per HARNESS §6 rule 2 there is no business logic in this file —
/// action buttons bind to RelayCommands on the VM and the page
/// itself never reads or mutates VisitorStore directly.
/// </summary>
public partial class VisitorDetailsPage : ContentPage
{
    private readonly VisitorDetailsViewModel _viewModel;
    private readonly ThemeService _theme;

    public VisitorDetailsPage(VisitorDetailsViewModel viewModel, ThemeService theme)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _viewModel = viewModel;
        _theme = theme;

        if (this.FindTopNavBar() is { } navBar)
        {
            navBar.ThemeToggleClicked += OnThemeToggleClicked;
            navBar.IsDarkMode = _theme.IsDarkMode;
            _theme.PropertyChanged += OnThemePropertyChanged;
        }

        // Sidebar (desktop / tablet only).
        if (this.FindSideNavigation() is { } sidebar)
            sidebar.NavItemClicked += OnSidebarNavItemClicked;
    }

    private async void OnSidebarNavItemClicked(object? sender, string route)
    {
        // Digital Pass is a sub-route of the dashboard's flow; always
        // pop back to whichever main route the user picked. The page's
        // Shell query parameter is consumed only when arriving from
        // the dashboard / directory; for the sidebar nav we just
        // navigate to the chosen main screen.
        await Shell.Current.GoToAsync($"//{route}");
    }

    private void OnThemeToggleClicked(object? sender, EventArgs e)
        => _theme.Toggle();

    private void OnThemePropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (this.FindTopNavBar() is { } navBar && e.PropertyName == nameof(ThemeService.IsDarkMode))
            navBar.IsDarkMode = _theme.IsDarkMode;
    }

    /// <summary>
    /// Shell pushes the route with <c>?passId=…</c>; forward every
    /// query key to the VM which knows what to do with them.
    /// </summary>
    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        if (Shell.Current?.CurrentState?.Location is Uri location)
        {
            _viewModel.ApplyQueryAttributes(
                QueryStringToDictionary(location.Query));
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadAsync();
    }

    protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
    {
        base.OnNavigatedFrom(args);
        _viewModel.Clear();
    }

    /// <summary>
    /// Parses <c>?key=value&amp;key2=value2</c> into a plain dictionary
    /// for the VM. Lives in the page because Uri parsing is a
    /// navigation concern, not a domain concern.
    /// </summary>
    private static Dictionary<string, object> QueryStringToDictionary(string query)
    {
        var result = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrEmpty(query))
            return result;

        var trimmed = query.StartsWith('?') ? query[1..] : query;
        foreach (var pair in trimmed.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var idx = pair.IndexOf('=');
            if (idx < 0)
                continue;
            var key = Uri.UnescapeDataString(pair[..idx]);
            var value = Uri.UnescapeDataString(pair[(idx + 1)..]);
            result[key] = value;
        }
        return result;
    }
}
