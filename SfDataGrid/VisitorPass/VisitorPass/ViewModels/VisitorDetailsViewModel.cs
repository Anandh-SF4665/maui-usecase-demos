using VisitorPass.Services;

namespace VisitorPass.ViewModels;

/// <summary>
/// ViewModel for the Digital Pass / Visitor Details screen (Screen 3).
///
/// VP-030 ships only the skeleton. VP-060 fills in the selected
/// visitor loading, the CheckIn / CheckOut / CancelVisit commands,
/// and the timeline step derivation. The visitor id is supplied via
/// Shell <c>GoToAsync("details", visitorId)</c> query parameter.
/// </summary>
public partial class VisitorDetailsViewModel : BaseViewModel
{
    private readonly IVisitorStore _store;

    /// <summary>Shell route name registered in <c>AppShell.xaml</c>.</summary>
    public const string Route = "details";

    public VisitorDetailsViewModel(IVisitorStore store)
    {
        _store = store;
    }
}
