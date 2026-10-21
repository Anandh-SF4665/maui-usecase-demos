using VisitorPass.ViewModels;

namespace VisitorPass.Views;

/// <summary>
/// VP-030 — Dashboard screen (Screen 1) skeleton. Phase 4 fills in
/// KPI grid + donut + column chart + today's-activity SfDataGrid.
/// The page is constructed by DI with its <see cref="DashboardViewModel"/>
/// injected (see <c>MauiProgram.cs</c>).
/// </summary>
public partial class DashboardPage : ContentPage
{
    public DashboardPage(DashboardViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
