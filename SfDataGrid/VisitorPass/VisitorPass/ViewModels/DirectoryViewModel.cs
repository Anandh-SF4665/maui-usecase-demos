using VisitorPass.Services;

namespace VisitorPass.ViewModels;

/// <summary>
/// ViewModel for the Directory screen (Screen 2).
///
/// VP-030 ships only the skeleton. VP-050 fills in the search +
/// status chip filter logic and the filtered visitor collection
/// bound to the SfDataGrid.
/// </summary>
public partial class DirectoryViewModel : BaseViewModel
{
    private readonly IVisitorStore _store;

    /// <summary>Shell route name registered in <c>AppShell.xaml</c>.</summary>
    public const string Route = "directory";

    public DirectoryViewModel(IVisitorStore store)
    {
        _store = store;
    }
}
