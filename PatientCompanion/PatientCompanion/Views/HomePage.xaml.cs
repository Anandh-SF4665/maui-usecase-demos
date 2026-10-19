using PatientCompanion.ViewModels;

namespace PatientCompanion.Views;

public partial class HomePage : ContentPage
{
    private HomeViewModel? viewModel;

    public HomePage()
        : this(App.Resolve<HomeViewModel>())
    {
    }

    public HomePage(HomeViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = this.viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        viewModel?.Refresh();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        viewModel?.Dispose();
        viewModel = null;
    }
}