using StockChart.ViewModels;

namespace StockChart
{
    public partial class AppShell : Shell
    {
        private readonly StockChartViewModel _viewModel;

        public AppShell(StockChartViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;

            // Shell's DataTemplate(Type) path uses Activator.CreateInstance (no DI),
            // so the content template is a factory closure supplying the injected ViewModel.
            HomeContent.ContentTemplate = new DataTemplate(() => new MainPage(_viewModel));
        }
    }
}
