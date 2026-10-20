using HospitalManagement.ViewModels;

namespace HospitalManagement.View;

public partial class DataGridPage : ContentPage
{
	private PatientRecordsViewModel viewModel;

	public DataGridPage()
	{
		InitializeComponent();
		viewModel = new PatientRecordsViewModel();
		BindingContext = viewModel;
		PatientService.PatientsChanged += OnPatientsChanged;
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		viewModel.LoadPatients();
	}

	private void OnPatientsChanged()
	{
		MainThread.BeginInvokeOnMainThread(() => viewModel.LoadPatients());
	}

	private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
	{
		if (BindingContext is PatientRecordsViewModel vm)
		{
			vm.SearchText = e.NewTextValue ?? string.Empty;
		}
	}

	private void OnFilterChanged(object sender, EventArgs e)
	{
		var picker = (Picker)sender;
		if (BindingContext is PatientRecordsViewModel vm && picker.SelectedItem is string selectedStatus)
		{
			vm.SelectedFilter = selectedStatus;
		}
	}
}
