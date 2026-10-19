using PatientCompanion.Services;
using PatientCompanion.ViewModels;

namespace PatientCompanion.Views;

public partial class AppointmentPage : ContentPage
{
    public AppointmentPage()
        : this(App.Resolve<AppointmentService>(), null)
    {
    }

    public AppointmentPage(AppointmentService appointmentService, AppointmentViewModel? viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel ?? new AppointmentViewModel(appointmentService.ConsumePendingReschedule());
    }
}