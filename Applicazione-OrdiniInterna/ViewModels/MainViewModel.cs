using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Applicazione_OrdiniInterna.Helpers;
using Applicazione_OrdiniInterna.Models;
using Applicazione_OrdiniInterna.Services;

namespace Applicazione_OrdiniInterna.ViewModels;

public class MainViewModel : ObservableBase
{
    private readonly ApiService _api;

    public MainViewModel()
    {
        _api = new ApiService();
        ConnectCommand = new AsyncRelayCommand(ConnectAsync);
        NavigateCommand = new RelayCommand(Navigate);

        CassaVm = new CassaViewModel(_api);
        KitchenVm = new KitchenViewModel(_api);
        CustomerDisplayVm = new CustomerDisplayViewModel(_api);
        AdminVm = new AdminViewModel(_api);

        CurrentView = KitchenVm;
        ActiveNav = "Cucina";
    }

    public ApiService Api => _api;

    // ── Connection ──
    private string _serverUrl = "http://localhost:5225";
    public string ServerUrl
    {
        get => _serverUrl;
        set => SetField(ref _serverUrl, value);
    }

    private string _apiKey = "1234";
    public string ApiKey
    {
        get => _apiKey;
        set => SetField(ref _apiKey, value);
    }

    private bool _isConnected;
    public bool IsConnected
    {
        get => _isConnected;
        set { SetField(ref _isConnected, value); OnPropertyChanged(nameof(StatusText)); OnPropertyChanged(nameof(StatusColor)); }
    }

    public string StatusText => IsConnected ? "Connesso" : "Non connesso";
    public SolidColorBrush StatusColor => IsConnected
        ? new SolidColorBrush(Color.FromRgb(34, 197, 94))
        : new SolidColorBrush(Color.FromRgb(239, 68, 68));

    // ── Navigation ──
    private object? _currentView;
    public object? CurrentView
    {
        get => _currentView;
        set => SetField(ref _currentView, value);
    }

    private string _activeNav = "Cucina";
    public string ActiveNav
    {
        get => _activeNav;
        set => SetField(ref _activeNav, value);
    }

    // ── Sub-ViewModels ──
    public CassaViewModel CassaVm { get; }
    public KitchenViewModel KitchenVm { get; }
    public CustomerDisplayViewModel CustomerDisplayVm { get; }
    public AdminViewModel AdminVm { get; }

    // ── Commands ──
    public ICommand ConnectCommand { get; }
    public ICommand NavigateCommand { get; }

    public async Task ConnectAsync()
    {
        try
        {
            _api.SetBaseUrl(ServerUrl.Trim());
            _api.ApiKey = ApiKey.Trim();
            await _api.GetProductsAsync();
            IsConnected = true;

            CassaVm.Start();
            KitchenVm.Start();
            CustomerDisplayVm.Start();
            AdminVm.Start();
        }
        catch (Exception ex)
        {
            IsConnected = false;
            MessageBox.Show($"Impossibile connettersi al server:\n{ex.Message}", "Errore", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void Navigate(object? parameter)
    {
        var target = parameter?.ToString() ?? "";
        ActiveNav = target;
        CurrentView = target switch
        {
            "Cassa" => CassaVm,
            "Cucina" => KitchenVm,
            "Display" => CustomerDisplayVm,
            "Admin" => AdminVm,
            _ => CurrentView
        };
    }

    public void StopAll()
    {
        KitchenVm.Stop();
        CustomerDisplayVm.Stop();
    }
}
