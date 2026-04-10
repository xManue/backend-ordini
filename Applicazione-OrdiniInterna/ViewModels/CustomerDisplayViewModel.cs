using System.Collections.ObjectModel;
using System.Windows.Threading;
using Applicazione_OrdiniInterna.Models;
using Applicazione_OrdiniInterna.Services;

namespace Applicazione_OrdiniInterna.ViewModels;

public class CustomerDisplayViewModel : ObservableBase
{
    private readonly ApiService _api;
    private DispatcherTimer? _pollTimer;

    public CustomerDisplayViewModel(ApiService api)
    {
        _api = api;
    }

    public ObservableCollection<DisplayOrderVm> PreparingOrders { get; } = [];
    public ObservableCollection<DisplayOrderVm> ReadyOrders { get; } = [];

    private string _clock = "";
    public string Clock
    {
        get => _clock;
        set => SetField(ref _clock, value);
    }

    private bool _hasPreparingOrders;
    public bool HasPreparingOrders { get => _hasPreparingOrders; set => SetField(ref _hasPreparingOrders, value); }

    private bool _hasReadyOrders;
    public bool HasReadyOrders { get => _hasReadyOrders; set => SetField(ref _hasReadyOrders, value); }

    public void Start()
    {
        _pollTimer?.Stop();
        _pollTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
        _pollTimer.Tick += async (_, _) =>
        {
            Clock = DateTime.Now.ToString("HH:mm");
            await LoadOrders();
        };
        _pollTimer.Start();
        Clock = DateTime.Now.ToString("HH:mm");
        _ = LoadOrders();
    }

    public void Stop()
    {
        _pollTimer?.Stop();
    }

    private async Task LoadOrders()
    {
        try
        {
            var all = await _api.GetOrdersAsync();

            PreparingOrders.Clear();
            ReadyOrders.Clear();

            foreach (var o in all.OrderBy(o => o.CreatedAt))
            {
                var st = KitchenViewModel.NormalizeStatus(o.Status);
                var display = new DisplayOrderVm
                {
                    OrderId = o.Id,
                    TableLabel = o.TableNumber.HasValue ? $"Tavolo {o.TableNumber}" : null
                };

                if (st is "paid" or "preparing")
                    PreparingOrders.Add(display);
                else if (st == "ready")
                    ReadyOrders.Add(display);
            }

            HasPreparingOrders = PreparingOrders.Count > 0;
            HasReadyOrders = ReadyOrders.Count > 0;
        }
        catch { }
    }
}

public class DisplayOrderVm
{
    public int OrderId { get; set; }
    public string? TableLabel { get; set; }
}
