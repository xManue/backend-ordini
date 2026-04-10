using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Applicazione_OrdiniInterna.Models;
using Applicazione_OrdiniInterna.Services;

namespace Applicazione_OrdiniInterna.Views;

public partial class DashboardView : UserControl
{
    private ApiService? _api;
    private List<ProductDto> _products = [];

    public DashboardView()
    {
        InitializeComponent();
        CategoryFilter.Items.Add("Tutte le categorie");
        CategoryFilter.SelectedIndex = 0;
    }

    public async void Initialize(ApiService api)
    {
        _api = api;
        await LoadProducts();
    }

    private async Task LoadProducts()
    {
        if (_api == null) return;
        try
        {
            _products = await _api.GetProductsAsync();
            BuildCategoryFilter();
            RenderFiltered();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore caricamento prodotti:\n{ex.Message}", "Errore", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void BuildCategoryFilter()
    {
        var selected = CategoryFilter.SelectedItem?.ToString();
        CategoryFilter.Items.Clear();
        CategoryFilter.Items.Add("Tutte le categorie");
        var cats = _products.Select(p => (p.Category ?? "").Trim()).Where(c => c != "").Distinct().OrderBy(c => c);
        foreach (var c in cats)
            CategoryFilter.Items.Add(char.ToUpper(c[0]) + c[1..]);
        CategoryFilter.SelectedItem = selected ?? "Tutte le categorie";
        if (CategoryFilter.SelectedIndex < 0) CategoryFilter.SelectedIndex = 0;
    }

    private void RenderFiltered()
    {
        var search = (SearchBox.Text ?? "").ToLowerInvariant();
        var catFilter = CategoryFilter.SelectedIndex > 0 ? CategoryFilter.SelectedItem?.ToString()?.ToLowerInvariant() : null;

        var filtered = _products.Where(p =>
        {
            var matchSearch = (p.Name ?? "").Contains(search, StringComparison.CurrentCultureIgnoreCase)
                || (p.Description ?? "").Contains(search, StringComparison.CurrentCultureIgnoreCase);
            var matchCat = catFilter == null || (p.Category ?? "").Equals(catFilter, StringComparison.CurrentCultureIgnoreCase);
            return matchSearch && matchCat;
        }).ToList();

        ProductList.Items.Clear();

        foreach (var item in filtered)
        {
            var row = new Border
            {
                Background = Brushes.White,
                CornerRadius = new CornerRadius(10),
                BorderBrush = (SolidColorBrush)FindResource("BorderBrush"),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(16, 12, 16, 12),
                Margin = new Thickness(0, 0, 0, 8)
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // Info
            var info = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            info.Children.Add(new TextBlock { Text = item.Name, FontWeight = FontWeights.SemiBold, FontSize = 14 });
            if (!string.IsNullOrWhiteSpace(item.Description))
                info.Children.Add(new TextBlock { Text = item.Description, FontSize = 12, Foreground = (SolidColorBrush)FindResource("TextSecondary"), TextTrimming = TextTrimming.CharacterEllipsis, MaxWidth = 350 });

            var metaPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 4, 0, 0) };
            var catBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(243, 244, 246)),
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(8, 2, 8, 2),
                Margin = new Thickness(0, 0, 8, 0)
            };
            catBorder.Child = new TextBlock { Text = item.Category ?? "Altro", FontSize = 11, FontWeight = FontWeights.Medium };
            metaPanel.Children.Add(catBorder);
            metaPanel.Children.Add(new TextBlock { Text = $"€{item.Price:F2}", FontSize = 14, FontWeight = FontWeights.Bold, Foreground = (SolidColorBrush)FindResource("TextPrimary"), VerticalAlignment = VerticalAlignment.Center });
            info.Children.Add(metaPanel);
            Grid.SetColumn(info, 0);
            grid.Children.Add(info);

            // Availability toggle
            var availBtn = new Button
            {
                Content = item.IsAvailable ? "✓ Disponibile" : "✗ Non disponibile",
                Background = item.IsAvailable ? new SolidColorBrush(Color.FromRgb(220, 252, 231)) : new SolidColorBrush(Color.FromRgb(254, 226, 226)),
                Foreground = item.IsAvailable ? new SolidColorBrush(Color.FromRgb(22, 101, 52)) : new SolidColorBrush(Color.FromRgb(153, 27, 27)),
                FontSize = 11, FontWeight = FontWeights.SemiBold,
                Padding = new Thickness(12, 6, 12, 6),
                BorderThickness = new Thickness(0),
                Cursor = System.Windows.Input.Cursors.Hand,
                Margin = new Thickness(12, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            var productId = item.Id;
            availBtn.Click += async (_, _) => { try { await _api!.ToggleProductAvailabilityAsync(productId); await LoadProducts(); } catch (Exception ex) { MessageBox.Show(ex.Message); } };
            Grid.SetColumn(availBtn, 1);
            grid.Children.Add(availBtn);

            // Edit
            var editBtn = new Button
            {
                Content = "✏️", FontSize = 16, Background = Brushes.Transparent, BorderThickness = new Thickness(0),
                Cursor = System.Windows.Input.Cursors.Hand, Padding = new Thickness(8, 4, 8, 4), Margin = new Thickness(4, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center
            };
            editBtn.Click += (_, _) => OpenEditDialog(item);
            Grid.SetColumn(editBtn, 2);
            grid.Children.Add(editBtn);

            // Delete
            var delBtn = new Button
            {
                Content = "🗑️", FontSize = 16, Background = Brushes.Transparent, BorderThickness = new Thickness(0),
                Cursor = System.Windows.Input.Cursors.Hand, Padding = new Thickness(8, 4, 8, 4), Margin = new Thickness(0), VerticalAlignment = VerticalAlignment.Center
            };
            delBtn.Click += async (_, _) =>
            {
                if (MessageBox.Show($"Eliminare \"{item.Name}\"?", "Conferma", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    try { await _api!.DeleteProductAsync(productId); await LoadProducts(); }
                    catch (Exception ex) { MessageBox.Show(ex.Message); }
                }
            };
            Grid.SetColumn(delBtn, 3);
            grid.Children.Add(delBtn);

            row.Child = grid;
            ProductList.Items.Add(row);
        }

        if (!filtered.Any())
        {
            ProductList.Items.Add(new TextBlock
            {
                Text = "Nessun piatto trovato",
                FontSize = 14, Foreground = (SolidColorBrush)FindResource("TextSecondary"),
                HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 40, 0, 0)
            });
        }
    }

    private void OpenEditDialog(ProductDto? existing = null)
    {
        var dlg = new Dialogs.ProductDialog(existing) { Owner = Window.GetWindow(this) };
        if (dlg.ShowDialog() == true)
        {
            _ = SaveProduct(existing?.Id, dlg.Result);
        }
    }

    private async Task SaveProduct(int? id, ProductDto dto)
    {
        try
        {
            if (id.HasValue)
                await _api!.UpdateProductAsync(id.Value, dto);
            else
                await _api!.CreateProductAsync(dto);
            await LoadProducts();
        }
        catch (Exception ex) { MessageBox.Show($"Errore salvataggio:\n{ex.Message}"); }
    }

    private void AddProductClick(object sender, RoutedEventArgs e) => OpenEditDialog();
    private void SearchChanged(object sender, TextChangedEventArgs e) => RenderFiltered();
    private void CategoryChanged(object sender, SelectionChangedEventArgs e) => RenderFiltered();
}