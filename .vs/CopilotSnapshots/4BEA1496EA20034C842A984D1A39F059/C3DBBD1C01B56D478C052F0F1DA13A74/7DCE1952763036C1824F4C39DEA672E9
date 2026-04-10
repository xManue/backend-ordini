using System.Windows;
using Applicazione_OrdiniInterna.ViewModels;
using Applicazione_OrdiniInterna.Views;

namespace Applicazione_OrdiniInterna
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _vm;

        public MainWindow()
        {
            InitializeComponent();
            _vm = new MainViewModel();
            DataContext = _vm;
            Loaded += async (_, _) => await _vm.ConnectAsync();
            Closed += (_, _) => _vm.StopAll();
        }

        private void OpenCustomerDisplayClick(object sender, RoutedEventArgs e)
        {
            var win = new Window
            {
                Title = "Bella Cucina — Display Clienti",
                WindowState = WindowState.Maximized,
                WindowStyle = WindowStyle.None,
                Background = System.Windows.Media.Brushes.Black,
                Content = new CustomerDisplayView { DataContext = _vm.CustomerDisplayVm }
            };
            win.KeyDown += (_, args) => { if (args.Key == System.Windows.Input.Key.Escape) win.Close(); };
            win.Show();
        }

        private void OpenKitchenWindowClick(object sender, RoutedEventArgs e)
        {
            var kitchenVm = new KitchenViewModel(_vm.Api);
            kitchenVm.Start();

            var win = new Window
            {
                Title = "Bella Cucina — Cucina",
                WindowState = WindowState.Maximized,
                WindowStyle = WindowStyle.None,
                Background = System.Windows.Media.Brushes.Black,
                Content = new KitchenView { DataContext = kitchenVm }
            };
            win.KeyDown += (_, args) => { if (args.Key == System.Windows.Input.Key.Escape) win.Close(); };
            win.Closed += (_, _) => kitchenVm.Stop();
            win.Show();
        }
    }
}