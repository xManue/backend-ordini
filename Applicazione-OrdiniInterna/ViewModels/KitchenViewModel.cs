using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Applicazione_OrdiniInterna.Helpers;
using Applicazione_OrdiniInterna.Models;
using Applicazione_OrdiniInterna.Services;

namespace Applicazione_OrdiniInterna.ViewModels;

public class KitchenViewModel : ObservableBase
{
    private readonly ApiService _api;
    private DispatcherTimer? _pollTimer;
    private DispatcherTimer? _clockTimer;

    public KitchenViewModel(ApiService api)
    {
        _api = api;
        SetPreparingCommand = new AsyncRelayCommand(SetPreparing);
        SetReadyCommand = new AsyncRelayCommand(SetReady);
        SetCompletedCommand = new AsyncRelayCommand(SetCompleted);
    }

    public ObservableCollection<OrderCardVm> Orders { get; } = [];

    private string _clock = "00:00:00";
    public string Clock
    {
        get => _clock;
        set => SetField(ref _clock, value);
    }

    private int _paidCount;
    public int PaidCount { get => _paidCount; set => SetField(ref _paidCount, value); }

    private int _prepCount;
    public int PrepCount { get => _prepCount; set => SetField(ref _prepCount, value); }

    private int _readyCount;
    public int ReadyCount { get => _readyCount; set => SetField(ref _readyCount, value); }

    private bool _isEmpty = true;
    public bool IsEmpty { get => _isEmpty; set => SetField(ref _isEmpty, value); }

    public ICommand SetPreparingCommand { get; }
    public ICommand SetReadyCommand { get; }
    public ICommand SetCompletedCommand { get; }

    public void Start()
    {
        _clockTimer?.Stop();
        _clockTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _clockTimer.Tick += (_, _) => Clock = DateTime.Now.ToString("HH:mm:ss");
        _clockTimer.Start();

        _pollTimer?.Stop();
        _pollTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
        _pollTimer.Tick += async (_, _) => await LoadOrders();
        _pollTimer.Start();

        _ = LoadOrders();
    }

    public void Stop()
    {
        _pollTimer?.Stop();
        _clockTimer?.Stop();
    }

    private async Task LoadOrders()
    {
        try
        {
            var all = await _api.GetOrdersAsync();
            var active = all
                .Where(o => NormalizeStatus(o.Status) is "paid" or "preparing" or "ready")
                .OrderBy(o => NormalizeStatus(o.Status) switch { "paid" => 0, "preparing" => 1, "ready" => 2, _ => 9 })
                .ThenBy(o => o.CreatedAt)
                .ToList();

            int paid = 0, prep = 0, ready = 0;
            Orders.Clear();
            foreach (var o in active)
            {
                var st = NormalizeStatus(o.Status);
                switch (st) { case "paid": paid++; break; case "preparing": prep++; break; case "ready": ready++; break; }

                var card = new OrderCardVm
                {
                    OrderId = o.Id,
                    Status = st,
                    TableLabel = o.TableNumber.HasValue ? $"Tavolo {o.TableNumber}" : "Asporto",
                    Elapsed = FormatElapsed(o.PaidAt != default ? o.PaidAt : o.CreatedAt),
                    TotalPrice = o.TotalPrice,
                    StatusLabel = st switch
                    {
                        "paid" => "NUOVO ORDINE",
                        "preparing" => "IN PREPARAZIONE",
                        "ready" => "PRONTO",
                        _ => st.ToUpper()
                    }
                };
                foreach (var item in o.OrderItems)
                    card.Items.Add(new OrderItemVm
                    {
                        Name = item.Product?.Name ?? $"Prodotto #{item.ProductId}",
                        Quantity = item.Quantity
                    });
                Orders.Add(card);
            }

            PaidCount = paid;
            PrepCount = prep;
            ReadyCount = ready;
            IsEmpty = Orders.Count == 0;
        }
        catch { }
    }

    private async Task SetPreparing(object? param)
    {
        if (param is not int id) return;
        try { await _api.SetPreparingAsync(id); await LoadOrders(); }
        catch (Exception ex) { MessageBox.Show($"Errore: {ex.Message}"); }
    }

    private async Task SetReady(object? param)
    {
        if (param is not int id) return;
        try { await _api.SetReadyAsync(id); await LoadOrders(); }
        catch (Exception ex) { MessageBox.Show($"Errore: {ex.Message}"); }
    }

    private async Task SetCompleted(object? param)
    {
        if (param is not int id) return;
        try { await _api.SetCompletedAsync(id); await LoadOrders(); }
        catch (Exception ex) { MessageBox.Show($"Errore: {ex.Message}"); }
    }

    internal static string NormalizeStatus(string s)
    {
        var lower = (s ?? "").ToLowerInvariant();
        return lower switch
        {
            "paid" or "1" => "paid",
            "preparing" or "2" => "preparing",
            "ready" or "3" => "ready",
            "completed" or "4" => "completed",
            "cancelled" or "5" => "cancelled",
            "pending" or "0" => "pending",
            _ => lower
        };
    }

    private static string FormatElapsed(DateTime ts)
    {
        if (ts == default) return "-";
        var span = DateTime.UtcNow - ts;
        if (span.TotalSeconds < 0) span = TimeSpan.Zero;
        return $"{(int)span.TotalMinutes}:{span.Seconds:D2}";
    }
}

public class OrderCardVm : ObservableBase
{
    public int OrderId { get; set; }
    public string Status { get; set; } = "";
    public string StatusLabel { get; set; } = "";
    public string TableLabel { get; set; } = "";
    public string Elapsed { get; set; } = "";
    public decimal TotalPrice { get; set; }
    public ObservableCollection<OrderItemVm> Items { get; } = [];
}

public class OrderItemVm
{
    public string Name { get; set; } = "";
    public int Quantity { get; set; }
}
