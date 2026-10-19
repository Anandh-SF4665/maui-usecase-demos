using PatientCompanion.Services;
using PatientCompanion.ViewModels;

namespace PatientCompanion.Views;

public partial class AppointmentPage : ContentPage
{
    private AppointmentViewModel? viewModel;

    public AppointmentPage()
        : this(App.Resolve<AppointmentService>(), null)
    {
    }

    public AppointmentPage(AppointmentService appointmentService, AppointmentViewModel? viewModel)
    {
        InitializeComponent();

        this.viewModel = viewModel ?? new AppointmentViewModel(appointmentService.ConsumePendingReschedule());
        BindingContext = this.viewModel;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        viewModel?.Dispose();
        viewModel = null;
    }
}