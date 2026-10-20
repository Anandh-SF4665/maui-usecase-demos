using HotelManagerSample;
using ChipSelectionChangedEventArgs = Syncfusion.Maui.Core.Chips.SelectionChangedEventArgs;
using MobileShellHost = HotelManagerSample.MobileShell;

namespace HotelManagerSample.Rooms;

/// <summary>
/// Code-behind for the mobile Rooms page. Pure navigation glue — the
/// heavy lifting is owned by <see cref="MobileRoomsViewModel"/>. Tap
/// handlers route through the visual tree to find the
/// <see cref="MobileShell"/> host so they don't accidentally break out
/// of the SfTabView surface. Shell.Current is never consulted.
/// </summary>
public partial class MobileRoomsPage : ContentView
{
    private readonly MobileRoomsViewModel _vm;

    /// <summary>Resolves the shared view-model from DI and binds it as
    /// the <see cref="BindableObject.BindingContext"/>.</summary>
    public MobileRoomsPage()
    {
        InitializeComponent();

        App.CaptureServices();
        _vm = App.Services!.GetRequiredService<MobileRoomsViewModel>();
        BindingContext = _vm;
    }

    /// <summary>Displays a placeholder "no new notifications" alert when
    /// the user taps the bell glyph on the top bar.</summary>
    /// <param name="sender">The tapped bell. Unused.</param>
    /// <param name="e">Tapped-event payload. Unused.</param>
    private async void OnBellTapped(object? sender, EventArgs e)
    {
        var host = ResolveShell();
        var alertHost = (Page?)host ?? Application.Current?.MainPage;
        if (alertHost is null) return;
        await alertHost.DisplayAlertAsync("Notifications", "No new notifications.", "OK");
    }

    /// <summary>Switches to the "More" tab when the user taps the
    /// profile glyph on the top bar.</summary>
    /// <param name="sender">The tapped glyph. Unused.</param>
    /// <param name="e">Tapped-event payload. Unused.</param>
    private void OnProfileTapped(object? sender, EventArgs e)
        => ResolveShell()?.SwitchToTab(MobileShellViewModel.MoreTabIndex);

    // ============== Floor chip selection ==============

    /// <summary>Forwards the chosen room filter chip's label into the
    /// VM's <see cref="MobileRoomsViewModel.ActiveFloorFilter"/>
    /// setter. The chip group runs in Choice mode so selecting a
    /// different chip fires exactly one invocation.</summary>
    /// <param name="sender">The SfChipGroup. Unused.</param>
    /// <param name="e">Chip-selection-changed payload from Syncfusion.</param>
    private void OnFloorChipSelectionChanged(object sender, ChipSelectionChangedEventArgs e)
    {
        var chip = e?.AddedItem as RoomFilterChip;
        _vm.ActiveFloorFilter = chip?.Label ?? MobileRoomsViewModel.FilterAll;
    }

    // ============== Status chip selection ==============

    /// <summary>Forwards the chosen status chip's label into the VM's
    /// <see cref="MobileRoomsViewModel.ActiveStatusFilter"/> setter.</summary>
    /// <param name="sender">The SfChipGroup. Unused.</param>
    /// <param name="e">Chip-selection-changed payload from Syncfusion.</param>
    private void OnStatusChipSelectionChanged(object sender, ChipSelectionChangedEventArgs e)
    {
        var chip = e?.AddedItem as RoomFilterChip;
        _vm.ActiveStatusFilter = chip?.Label ?? MobileRoomsViewModel.FilterAll;
    }

    /// <summary>Clears the inline search entry on tap of the trailing
    /// "×" glyph.</summary>
    /// <param name="sender">The tapped glyph. Unused.</param>
    /// <param name="e">Tapped-event payload. Unused.</param>
    private void OnClearSearchTapped(object? sender, TappedEventArgs e)
        => _vm.SearchText = string.Empty;

    /// <summary>Walks up the visual tree from this <see cref="ContentView"/>
    /// looking for the <see cref="MobileShellHost"/> that owns the
    /// navigation stack. Returns <c>null</c> when the page has not
    /// been parented under a shell yet.</summary>
    /// <returns>The discovered shell host, or <c>null</c> when not
    /// parented.</returns>
    private MobileShellHost? ResolveShell()
    {
        Element? walker = this.Parent;
        while (walker is not null)
        {
            if (walker is MobileShellHost shell) return shell;
            walker = walker.Parent;
        }
        return null;
    }
}
