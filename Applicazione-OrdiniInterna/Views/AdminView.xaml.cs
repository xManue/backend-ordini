using System.Windows;
using System.Windows.Controls;
using Applicazione_OrdiniInterna.Models;
using Applicazione_OrdiniInterna.ViewModels;

namespace Applicazione_OrdiniInterna.Views;

public partial class AdminView : UserControl
{
    public AdminView()
    {
        InitializeComponent();
    }

    private AdminViewModel? Vm => DataContext as AdminViewModel;

    private void AdvanceOrderClick(object sender, RoutedEventArgs e)
    {
        if (Vm == null) return;
        if (sender is not Button btn || btn.Tag is not OrderDto order) return;

        var currentStatus = KitchenViewModel.NormalizeStatus(order.Status);
        var nextStatus = currentStatus switch
        {
            "pending" => "paid",
            "paid" => "preparing",
            "preparing" => "ready",
            "ready" => "completed",
            _ => null
        };

        if (nextStatus == null)
        {
            MessageBox.Show("L'ordine non può avanzare ulteriormente.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var param = $"{order.Id}|{nextStatus}";
        if (Vm.SetOrderStatusCommand.CanExecute(param))
            Vm.SetOrderStatusCommand.Execute(param);
    }
}
