using Syncfusion.Maui.Maps;
using PatientCompanion.Services;
using PatientCompanion.ViewModels;

namespace PatientCompanion.Views;

public partial class AppointmentReviewView : ContentView
{
    public AppointmentReviewView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    public AppointmentReviewView(
        AppointmentViewModel appointmentViewModel)
    {
        InitializeComponent();
        Loaded += OnLoaded;

        BindingContext =
            new AppointmentReviewViewModel(
                appointmentViewModel,
                App.Resolve<AppointmentService>());
    }

    private void OnLoaded(object? sender, EventArgs e)
    {
        if (WorldMapLayer.ShapesSource is not null)
            return;

        var asset = App.Resolve<WorldMapAssetService>();
        var stream = asset.ShapesStream;
        if (stream is null)
        {
            // Asset hasn't finished loading yet — wait for it once.
            Dispatcher.Dispatch(async () =>
            {
                var loaded = await asset.GetStreamAsync();
                if (loaded is not null && WorldMapLayer.ShapesSource is null)
                    WorldMapLayer.ShapesSource = loaded;
            });
            return;
        }

        WorldMapLayer.ShapesSource = stream;
    }
}