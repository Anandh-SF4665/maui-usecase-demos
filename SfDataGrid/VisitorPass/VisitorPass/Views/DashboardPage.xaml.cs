using Syncfusion.Maui.DataGrid;
using VisitorPass.Models;
using VisitorPass.ViewModels;

namespace VisitorPass.Views;

/// <summary>
/// VP-041 / VP-042 / VP-043 — Dashboard page code-behind.
///
/// The XAML is the single source of truth for layout; the code-behind
/// only wires up two events:
///
///   1. TopNavBar.RegisterClicked -> routes to the
///      registration surface. Per VP-073 the container is chosen by
///      idiom: SfPopup on desktop / tablet, a MAUI-native
///      Navigation.PushModalAsync slide-up page on phone. The
///      actual SfPopup instance is not wired up until VP-073; for now
///      the trigger raises the VM event and the page simply logs that
///      the registration surface should open.
///   2. SfDataGrid.CellTapped on the today's-activity
///      grid -> navigates to the Digital Pass page with the tapped
///      visitor's id.
///
/// Per HARNESS §6 rule 2 there is no business logic in this file —
/// it just routes events to the VM / Shell.
/// </summary>
public partial class DashboardPage : ContentPage
{
    /// <summary>Query-parameter key used when navigating to the details page.</summary>
    public const string VisitorIdQueryKey = "passId";

    public DashboardPage(DashboardViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;

        // The TopNavBar control lives inside the page's root layout;
        // walk the visual tree to find it (one element deep) and
        // subscribe to its RegisterClicked event.
        if (this.FindTopNavBar() is { } navBar)
            navBar.RegisterClicked += OnRegisterClicked;
    }

    private void OnRegisterClicked(object? sender, EventArgs e)
    {
        // Phase 5 wires the popup / modal slide-up page. For now we
        // rely on the DashboardViewModel.RegisterVisitorRequested
        // event the VM raises when its RegisterVisitorCommand fires.
        if (BindingContext is DashboardViewModel vm)
        {
            vm.RegisterVisitorCommand.Execute(null);
        }
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

/// <summary>
/// Tiny helper extension to find the single TopNavBar in the page's
/// visual tree. Keeps the code-behind free of x:Name lookups.
/// </summary>
internal static class DashboardPageHelpers
{
    public static VisitorPass.Controls.TopNavBar? FindTopNavBar(this ContentPage page)
    {
        if (page.Content is Layout layout)
            return FindInLayout<VisitorPass.Controls.TopNavBar>(layout);

        return null;
    }

    private static T? FindInLayout<T>(IView view) where T : IView
    {
        if (view is T match)
            return match;

        if (view is Layout layout)
        {
            foreach (var child in layout.Children)
            {
                var found = FindInLayout<T>(child);
                if (found is not null)
                    return found;
            }
        }

        if (view is IContentView contentView && contentView.Content is IView nested)
        {
            var found = FindInLayout<T>(nested);
            if (found is not null)
                return found;
        }

        return default;
    }
}
