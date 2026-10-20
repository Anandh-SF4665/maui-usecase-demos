namespace SalesOrderTracker
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute("OrderDetailsPage", typeof(Views.OrderDetailsPage));
        }
    }
}
