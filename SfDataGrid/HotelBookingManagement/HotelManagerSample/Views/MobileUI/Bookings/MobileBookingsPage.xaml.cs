using ChipSelectionChangedEventArgs = Syncfusion.Maui.Core.Chips.SelectionChangedEventArgs;
using MobileShellHost = HotelManagerSample.MobileShell;

namespace HotelManagerSample.Bookings;

/// <summary>
/// Code-behind for the mobile bookings page.  Reuses the master
/// <see cref="MobileBookingsViewModel"/>, which itself already listens
/// to the shared <see cref="IHotelStore"/> so reservation changes are
/// reflected without ceremony.
///
/// Tap handlers delegate to the <see cref="MobileShellHost"/> for
/// tab switching — standard MAUI page navigation (PushAsync / PopAsync)
/// is used for the details page so the run-time never touches
/// <c>Shell.Current</c>.
/// </summary>
public partial class MobileBookingsPage : ContentView
{
    private readonly MobileBookingsViewModel _vm;

    /// <summary>Resolves the shared view-model from DI and binds it as
    /// the <see cref="BindableObject.BindingContext"/>.</summary>
    public MobileBookingsPage()
    {
        InitializeComponent();

        App.CaptureServices();
        _vm = App.Services!.GetRequiredService<MobileBookingsViewModel>();
        BindingContext = _vm;
    }

    // ============== Top-bar taps ==============

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

    // ============== Filter chip selection ==============

    /// <summary>SfChipGroup fires SelectionChanged with the chosen
    /// BookingFilterChip instance. We forward the chip's label into the
    /// VM's ActiveFilter setter, which kicks off ApplyFilters.</summary>
    /// <param name="sender">The SfChipGroup. Unused.</param>
    /// <param name="e">Chip-selection-changed payload from Syncfusion.</param>
    private void OnChipGroupSelectionChanged(object sender, ChipSelectionChangedEventArgs e)
    {
        // AddedItem is the selected chip. With ChoiceMode=Single there is
        // always exactly one. If the AddedItem is null the user
        // deselected everything — fall back to "All".
        var chip = e?.AddedItem as BookingFilterChip;
        _vm.ActiveFilter = chip?.Label ?? MobileBookingsViewModel.FilterAll;
    }

    /// <summary>Clears the inline search entry on tap of the trailing
    /// "×" glyph.</summary>
    /// <param name="sender">The tapped glyph. Unused.</param>
    /// <param name="e">Tapped-event payload. Unused.</param>
    private void OnClearSearchTapped(object? sender, TappedEventArgs e)
        => _vm.SearchText = string.Empty;

    /// <summary>Handled by the chip's secondary "Folio" / "Check In"
    /// button — not the whole card.</summary>
    /// <param name="sender">The tapped button. Used to extract its
    /// <c>CommandParameter</c>.</param>
    /// <param name="e">Standard event payload. Unused.</param>
    private async void OnSecondaryCtaClicked(object? sender, EventArgs e)
    {
        if (sender is not Button button || button.CommandParameter is not Booking booking)
            return;

        var ctaLabel = MobileBookingCtaHelper.LabelFor(booking.Status);
        var host = ResolveShell();
        var alertHost = (Page?)host ?? Application.Current?.MainPage;
        if (alertHost is null) return;

        await alertHost.DisplayAlertAsync(ctaLabel, $"{booking.Guest} • {booking.Room}", "OK");
    }

    /// <summary>Walks up the visual tree from this <see cref="ContentView"/>
    /// looking for the <see cref="MobileShellHost"/> that owns the
    /// navigation stack. Used by the top-bar tap handlers and the
    /// chip-CTA handler to switch tabs without consulting
    /// <c>Shell.Current</c>.</summary>
    /// <returns>The discovered shell host, or <c>null</c> when the page
    /// has not been parented under one yet.</returns>
    private MobileShellHost? ResolveShell()
    {
        // Walk up the visual tree from this ContentView to find the
        // ContentPage host that owns the Navigation stack — this works
        // without any dependency on Shell.Current.
        Element? walker = this.Parent;
        while (walker is not null)
        {
            if (walker is MobileShellHost shell)
                return shell;
            walker = walker.Parent;
        }
        return null;
    }
}

/// <summary>Static helper for status-CTA label resolution so a non-VM
/// caller (e.g. the button click handler) can resolve the same labels
/// the Syncfusion converter emits. Keeps a single source of truth.</summary>
internal static class MobileBookingCtaHelper
{
    /// <summary>Resolves the friendly CTA label rendered on a
    /// booking card based on the booking's lifecycle status. Mirrors
    /// the <c>MobileBookingCtaConverter</c> so non-XAML callers get
    /// the same label.</summary>
    /// <param name="status">Booking status string. May be null.</param>
    /// <returns>The CTA label — "Check In", "Folio", "Send Link",
    /// "Re-Book" or "View" depending on the status.</returns>
    public static string LabelFor(string? status) => (status ?? string.Empty).Trim().ToUpperInvariant() switch
    {
        "Confirmed"   => "Check In",
        "Checked in"  => "Folio",
        "Pending"     => "Send Link",
        "Checked out" => "Folio",
        "Cancelled"   => "Re-Book",
        _             => "View"
    };
}
