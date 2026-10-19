using PatientCompanion.ViewModels;

namespace PatientCompanion.Views;

public partial class VisitsPage : ContentPage
{
    private VisitsViewModel? viewModel;

    public VisitsPage()
        : this(App.Resolve<VisitsViewModel>())
    {
    }

    public VisitsPage(VisitsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = this.viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        viewModel?.Subscribe();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        viewModel?.Unsubscribe();
    }
}