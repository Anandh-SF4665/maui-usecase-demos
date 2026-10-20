using HotelManagerSample;
using System.Collections.Specialized;
using System.ComponentModel;

namespace HotelManagerSample;

/// <summary>
/// Code-behind for the desktop Rooms page. Reacts to view-model
/// property changes (filters, search text) and to direct collection
/// mutations on <see cref="RoomsViewModel.TableRooms"/> so the
/// SfDataGrid height shrinks and grows with the visible row count.
/// The actual room data flows through the shared singleton
/// <see cref="RoomsViewModel"/> resolved from DI.
/// </summary>
public partial class RoomsPage : ContentPage
{
    private readonly RoomsViewModel _vm;

    // Constants that mirror the RoomsTable XAML — HeaderRowHeight="32",
    // RowHeight="56" with a 1-DIP bottom grid-line stroke.
    private const double HeaderHeight = 32;
    private const double RowHeight = 56;
    private const double BottomBorder = 1;

    /// <summary>Initialises the XAML-inflated tree, resolves the
    /// shared view-model, computes the initial dynamic grid height,
    /// and subscribes to the VM so any subsequent filter pass / store
    /// mutation re-applies the height.</summary>
    public RoomsPage()
    {
        InitializeComponent();

        // TitleBarController.Apply() (App.xaml.cs OnLaunched) already
        // removes the OS title-bar strip and paints the chrome in the
        // page background colour, so the original XAML padding of 24,14
        // stays unchanged on every platform.

        App.CaptureServices();
        _vm = App.Services!.GetRequiredService<RoomsViewModel>();
        BindingContext = _vm;

        // Compute initial height now that the VM is wired up.
        UpdateGridHeight();

        // Listen for any VM property change so chip toggles, search text
        // updates, and store-driven refreshes all rebuild the table height.
        _vm.PropertyChanged += OnViewModelPropertyChanged;

        // Also react to the TableRooms collection changing directly — the
        // VM raises PropertyChanged(TableRooms) but ObservableCollection
        // changes don't fire it automatically, so we subscribe to
        // CollectionChanged for an immediate refresh on each add/remove.
        if (_vm.TableRooms is INotifyCollectionChanged tableSource)
            tableSource.CollectionChanged += (_, _) => UpdateGridHeight();
    }

    /// <summary>Property-change listener. Recomputes the grid height
    /// whenever <see cref="RoomsViewModel.TableRooms"/> or
    /// <see cref="RoomsViewModel.FilteredRooms"/> change.</summary>
    /// <param name="sender">The VM raising the event. Unused.</param>
    /// <param name="e">Property-change payload identifying which
    /// property changed.</param>
    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // The cheapest signal we have for "the table row count just changed"
        // is the VM raising PropertyChanged on TableRooms (or FilteredRooms,
        // which is rebuilt in lockstep). Refresh on either, and on the
        // setter-driven refresh path too.
        if (e.PropertyName is nameof(RoomsViewModel.TableRooms)
                            or nameof(RoomsViewModel.FilteredRooms))
        {
            UpdateGridHeight();
        }
    }

    /// <summary>Toggles the inline "×" clear-search label visibility
    /// for the rooms-page search entry.</summary>
    /// <param name="sender">The search entry. Unused.</param>
    /// <param name="e">Text-changed payload from MAUI bindings.</param>
    private void OnRoomsSearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (this.FindByName<Label>("RoomsSearchClear") is { } clear)
            clear.IsVisible = !string.IsNullOrEmpty(e.NewTextValue);
    }

    /// <summary>Clears the rooms-page search entry on tap of the
    /// trailing "×" glyph.</summary>
    /// <param name="sender">The tapped glyph. Unused.</param>
    /// <param name="e">Tapped-event payload. Unused.</param>
    private void OnRoomsSearchClearTapped(object? sender, TappedEventArgs e)
    {
        if (RoomsSearchEntry is not null)
            RoomsSearchEntry.Text = string.Empty;
    }

    /// <summary>
    /// Height bound to <c>SfDataGrid.HeightRequest</c> on the rooms table so
    /// the grid grows / shrinks with the filtered row count (e.g. when the
    /// user toggles a status chip). Formula:
    /// <c>header + (count × row) + bottomBorder</c>.
    /// On an empty result the height shrinks to just the header so the
    /// "no rows" block doesn't sit below an enormous blank cell.
    /// </summary>
    public double GridHeight
    {
        get => gridHeight;
        private set
        {
            if (gridHeight == value) return;
            gridHeight = value;
            OnPropertyChanged();
        }
    }

    private double gridHeight;

    /// <summary>Recomputes <see cref="GridHeight"/> from the current
    /// <see cref="RoomsViewModel.TableRooms"/> count. Always renders
    /// at least one row so the column header stays visible on an
    /// empty result.</summary>
    private void UpdateGridHeight()
    {
        var shown = Math.Max(1, _vm.TableRooms?.Count ?? 0);
        GridHeight = HeaderHeight + shown * RowHeight + BottomBorder;
    }
}
