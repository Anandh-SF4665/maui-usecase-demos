using VisitorPass.ViewModels;

namespace VisitorPass.Views;

/// <summary>
/// VP-030 — Digital Pass / Visitor Details screen (Screen 3)
/// skeleton. Phase 6 fills in the visitor identity card, barcode,
/// timeline, summary, and Check In / Check Out / Cancel buttons.
/// </summary>
public partial class VisitorDetailsPage : ContentPage
{
    public VisitorDetailsPage(VisitorDetailsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
