using VisitorPass.Services;

namespace VisitorPass.ViewModels;

/// <summary>
/// ViewModel for the Dashboard screen (Screen 1).
///
/// VP-030 ships only the skeleton (title + subtitle + register
/// trigger wiring). VP-040 fills in KPI counts, donut + column
/// chart series, and the today's-activity list derived from
/// <see cref="IVisitorStore"/>.
/// </summary>
public partial class DashboardViewModel : BaseViewModel
{
    private readonly IVisitorStore _store;

    /// <summary>Shell route name registered in <c>AppShell.xaml</c>.</summary>
    public const string Route = "dashboard";

    public DashboardViewModel(IVisitorStore store)
    {
        _store = store;
    }
}
