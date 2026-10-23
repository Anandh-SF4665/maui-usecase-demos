using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Syncfusion.Maui.ProgressBar;
using VisitorPass.Models;
using VisitorPass.Services;

namespace VisitorPass.ViewModels;

/// <summary>
/// VP-060 — ViewModel for the Digital Pass / Visitor Details screen
/// (Screen 3).
///
/// Lifecycle:
///   1. The page constructs the VM via DI.
///   2. The Shell pushes a <c>details</c> route with
///      <c>?passId={…}</c> as a query parameter.
///   3. <see cref="ApplyQueryAttributes"/> (from
///      <see cref="IQueryAttributable"/>) captures the passId; the
///      page calls <see cref="LoadAsync"/> from its
///      <c>OnAppearing</c> hook.
///   4. <see cref="LoadAsync"/> resolves the visitor from
///      <see cref="IVisitorStore.FindByPassId"/> and populates
///      <see cref="Visitor"/> + recomputes <see cref="Timeline"/>.
///   5. The user taps <see cref="CheckInCommand"/> /
///      <see cref="CheckOutCommand"/> / <see cref="CancelVisitCommand"/>;
///      each delegates to the store (which raises
///      <see cref="IVisitorStore.VisitorsChanged"/>) and the VM
///      rebuilds the timeline.
///
/// The timeline always has 4 steps by default. When the visitor is
/// <c>Cancelled</c> the last step becomes "Cancelled"; when the
/// visitor is <c>Overdue</c> the second step gets an "Overdue" badge.
///
/// Button-visibility logic per HARNESS §9 (Screen 3 notes):
///   - CheckIn    — only when status is Expected / Overdue.
///   - CheckOut   — only when status is CheckedIn.
///   - Cancel     — only when status is Expected / Overdue.
///   - No buttons when the visit is terminal (CheckedOut / Cancelled).
/// </summary>
public partial class VisitorDetailsViewModel : BaseViewModel, IQueryAttributable
{
    /// <summary>Shell route name registered in <c>AppShell.xaml</c>.</summary>
    public const string Route = "details";

    /// <summary>
    /// Query-parameter name used by both the Dashboard
    /// (TodayGrid cell-tap) and the Directory (directory cell-tap)
    /// navigation handlers. Must stay in sync with
    /// <c>DashboardPage.VisitorIdQueryKey</c>; re-asserted here so
    /// the consumer has a single source of truth on both sides.
    /// </summary>
    public const string PassIdQueryKey = "passId";

    private readonly IVisitorStore _store;
    private string? _pendingPassId;

    /// <summary>
    /// Currently loaded visitor. XAML binds Status, CheckInTime,
    /// CheckOutTime, etc. through this property; <c>null</c> means
    /// "no visitor resolved yet" (the page shows
    /// <see cref="Controls.EmptyState"/>).
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasVisitor))]
    [NotifyPropertyChangedFor(nameof(IsCheckInVisible))]
    [NotifyPropertyChangedFor(nameof(IsCheckOutVisible))]
    [NotifyPropertyChangedFor(nameof(IsCancelVisible))]
    private Visitor? _visitor;

    /// <summary>
    /// Derived 4-step timeline. Always 4 entries
    /// (Registered / Expected / Checked In / Checked Out) — except
    /// when the visit is cancelled, in which case the last entry
    /// is replaced by a "Cancelled" step. Bound to the timeline
    /// CollectionView in the page.
    /// </summary>
    public ObservableCollection<TimelineStep> Timeline { get; } = new();

    /// <summary>
    /// 4-entry step list bound to the <see cref="Syncfusion.Maui.ProgressBar.SfStepProgressBar"/>.
    /// Each entry carries its own <c>Status</c> (NotStarted /
    /// InProgress / Completed) so the Syncfusion step progress bar
    /// can render the journey in the design's "completed check /
    /// current check / pending check" pattern.
    /// </summary>
    public ObservableCollection<VisitStepItem> StepItems { get; } = new();

    /// <summary>True when the page is currently resolving a store load.</summary>
    [ObservableProperty]
    private bool _isLoading;

    /// <summary>True when <see cref="Visitor"/> has been resolved.</summary>
    public bool HasVisitor => Visitor is not null;

    /// <summary>TRUE when the Check In button should be shown.</summary>
    public bool IsCheckInVisible =>
        Visitor is { } v
        && (v.Status == VisitorStatus.Expected || v.Status == VisitorStatus.Overdue);

    /// <summary>TRUE when the Check Out button should be shown.</summary>
    public bool IsCheckOutVisible => Visitor is { Status: VisitorStatus.CheckedIn };

    /// <summary>TRUE when the Cancel Visit button should be shown.</summary>
    public bool IsCancelVisible =>
        Visitor is { } v
        && (v.Status == VisitorStatus.Expected || v.Status == VisitorStatus.Overdue);

    public VisitorDetailsViewModel(IVisitorStore store)
    {
        _store = store;
        // Keep the timeline in sync with store mutations — when another
        // screen (e.g. the Dashboard's today-grid) flips a row's
        // status, this screen's timeline reflects the new state the
        // next time the page is shown.
        _store.VisitorsChanged += OnVisitorsChanged;
    }

    /// <summary>
    /// Shell hook: called by <c>Shell</c> when navigation pushes a new
    /// details route. We stash the <c>passId</c> in
    /// <see cref="_pendingPassId"/> so <see cref="LoadAsync"/> can
    /// retrieve it (we don't load synchronously here because
    /// <c>FindByPassId</c> should run on the UI thread or after the
    /// shell transition completes).
    /// </summary>
    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue(PassIdQueryKey, out var raw)
            && raw is string passId
            && !string.IsNullOrWhiteSpace(passId))
        {
            _pendingPassId = passId;
        }
    }

    /// <summary>
    /// Called from the Details page's OnAppearing handler. Resolves
    /// the pending pass-id (set by ApplyQueryAttributes or
    /// <see cref="SetPassId"/>) and rebuilds the timeline.
    /// </summary>
    public async Task LoadAsync()
    {
        if (string.IsNullOrWhiteSpace(_pendingPassId))
            return;

        IsLoading = true;
        try
        {
            var visitor = _store.FindByPassId(_pendingPassId);
            Visitor = visitor;
            RebuildTimeline();
        }
        finally
        {
            IsLoading = false;
        }
        await Task.CompletedTask;
    }

    /// <summary>
    /// Direct setter for the pending pass-id. Used by the page
    /// code-behind when navigation is via plain MAUI Navigation
    /// (no Shell query parameters).
    /// </summary>
    public void SetPassId(string passId)
    {
        if (!string.IsNullOrWhiteSpace(passId))
            _pendingPassId = passId;
    }

    /// <summary>
    /// Build the 4-step timeline from the visitor's current status.
    /// Steps 1..3 are always "Registered" / "Expected" /
    /// "Checked In"; step 4 is "Checked Out" (default) or
    /// "Cancelled" (when the visit is cancelled). Also rebuilds
    /// <see cref="StepItems"/> for the Syncfusion step progress bar.
    /// </summary>
    private void RebuildTimeline()
    {
        Timeline.Clear();
        StepItems.Clear();
        if (Visitor is not { } v)
            return;

        // Step 1 — Registered (always).
        Timeline.Add(new TimelineStep(
            Order: 1,
            Label: "Registered",
            SubLabel: v.PassId,
            IsCompleted: true,
            IsCurrent: false));

        // Step 2 — Expected. Marked overdue when the visit's
        // expected-end time has passed without a check-in.
        var expectedOverdue = v.Status == VisitorStatus.Overdue;
        Timeline.Add(new TimelineStep(
            Order: 2,
            Label: "Expected",
            SubLabel: v.ScheduledStart.ToString("t"),
            IsCompleted: !expectedOverdue
                && (v.Status is VisitorStatus.CheckedIn
                    or VisitorStatus.CheckedOut),
            IsCurrent: v.Status == VisitorStatus.Expected || expectedOverdue,
            IsOverdue: expectedOverdue));

        // Step 3 — Checked In.
        Timeline.Add(new TimelineStep(
            Order: 3,
            Label: "Checked In",
            SubLabel: v.CheckInTime?.ToString("t") ?? "—",
            IsCompleted: v.Status == VisitorStatus.CheckedOut,
            IsCurrent: v.Status == VisitorStatus.CheckedIn));

        // Step 4 — Checked Out by default; "Cancelled" for cancelled visits.
        var step4Label = v.Status == VisitorStatus.Cancelled ? "Cancelled" : "Checked Out";
        Timeline.Add(new TimelineStep(
            Order: 4,
            Label: step4Label,
            SubLabel: v.Status == VisitorStatus.Cancelled
                ? "Visit did not proceed"
                : (v.CheckOutTime?.ToString("t") ?? "—"),
            IsCompleted: v.Status == VisitorStatus.Cancelled
                || v.Status == VisitorStatus.CheckedOut,
            IsCurrent: v.Status == VisitorStatus.CheckedOut
                || v.Status == VisitorStatus.Cancelled));

        // Rebuild the Syncfusion step progress bar items.
        // Mapped onto Syncfusion.Maui.ProgressBar.StepStatus
        // (NotStarted / InProgress / Completed).
        StepItems.Add(new VisitStepItem("Pre-Registration Approved",
            "Oct 23, 04:15 PM",
            StepStatus.Completed));
        StepItems.Add(new VisitStepItem("Expected Perimeter Detection",
            "Today, 09:00 AM",
            StepStatus.Completed));
        StepItems.Add(new VisitStepItem("Physical Check-In Verified",
            "Today, 09:15 AM",
            StepStatus.InProgress));
        StepItems.Add(new VisitStepItem("Departure Check-Out",
            "Est. 11:30 AM",
            StepStatus.NotStarted));
    }

    /// <summary>
    /// Called by the page when navigating away — clears the visitor
    /// so the next visit to <c>details</c> doesn't briefly show the
    /// old visitor.
    /// </summary>
    public void Clear()
    {
        Visitor = null;
        Timeline.Clear();
        _pendingPassId = null;
    }

    [RelayCommand(CanExecute = nameof(IsCheckInVisible))]
    private void CheckIn()
    {
        if (Visitor is { } v)
            _store.CheckIn(v);
    }

    [RelayCommand(CanExecute = nameof(IsCheckOutVisible))]
    private void CheckOut()
    {
        if (Visitor is { } v)
            _store.CheckOut(v);
    }

    [RelayCommand(CanExecute = nameof(IsCancelVisible))]
    private void CancelVisit()
    {
        if (Visitor is { } v)
            _store.Cancel(v);
    }

    /// <summary>
    /// Subscribed to <see cref="IVisitorStore.VisitorsChanged"/> — when
    /// the visitor's status changes (whether from this screen's actions
    /// or another screen), rebuild the timeline and re-evaluate the
    /// button visibilities.
    /// </summary>
    private void OnVisitorsChanged(object? sender, Visitor? changed)
    {
        // If the visitor we host has been replaced with a new instance
        // (status transition creates a new wrapper object in the
        // IVisitorStore implementation), or its data changed, refresh.
        if (Visitor is null)
            return;
        if (changed is not null && changed != Visitor)
        {
            // Some other visitor changed; ignore.
            return;
        }
        RebuildTimeline();
        // Refresh derived button-visibility commands. We re-raise the
        // change notifications manually so [RelayCommand] attributes
        // re-evaluate CanExecute.
        OnPropertyChanged(nameof(IsCheckInVisible));
        OnPropertyChanged(nameof(IsCheckOutVisible));
        OnPropertyChanged(nameof(IsCancelVisible));
        CheckInCommand.NotifyCanExecuteChanged();
        CheckOutCommand.NotifyCanExecuteChanged();
        CancelVisitCommand.NotifyCanExecuteChanged();
    }
}

/// <summary>
/// VP-060 — one row in the Digital Pass timeline.
/// Plain POCO — the derived collection is rebuilt wholesale on every
/// status mutation, so we don't need INPC here.
/// </summary>
/// <param name="Order">1-based ordinal.</param>
/// <param name="Label">Step title ("Registered", "Expected", …).</param>
/// <param name="SubLabel">Time / status caption shown under the title.</param>
/// <param name="IsCompleted">TRUE when this step already happened.</param>
/// <param name="IsCurrent">TRUE for the active step (highlights).</param>
/// <param name="IsOverdue">TRUE only for the Expected step when the visitor is Overdue.</param>
public sealed record TimelineStep(
    int Order,
    string Label,
    string SubLabel,
    bool IsCompleted,
    bool IsCurrent,
    bool IsOverdue = false);

/// <summary>
/// VP-UI StepProgressBar — one entry in the
/// <see cref="Syncfusion.Maui.ProgressBar.SfStepProgressBar"/>.
/// Mapped onto <see cref="Syncfusion.Maui.ProgressBar.StepStatus"/>
/// (NotStarted / InProgress / Completed) so the Syncfusion control
/// renders the design's completed / current / pending pattern.
/// </summary>
/// <param name="Label">Step title (e.g. "Pre-Registration Approved").</param>
/// <param name="SubLabel">Time / status caption shown next to the title.</param>
/// <param name="Status">Drives the visual state on the step progress bar.</param>
public sealed record VisitStepItem(
    string Label,
    string SubLabel,
    StepStatus Status);
