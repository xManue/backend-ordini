using System.Globalization;
using System.Windows;
using Applicazione_OrdiniInterna.Models;

namespace Applicazione_OrdiniInterna.Dialogs;

public partial class ProductDialog : Window
{
    public ProductDto Result { get; private set; } = new();

    public ProductDialog(ProductDto? existing = null)
    {
        InitializeComponent();
        Title = existing != null ? "Modifica Piatto" : "Nuovo Piatto";
        if (existing != null)
        {
            NameBox.Text = existing.Name;
            DescBox.Text = existing.Description ?? "";
            PriceBox.Text = existing.Price.ToString("F2", CultureInfo.InvariantCulture);
            CategoryBox.Text = existing.Category ?? "";
        }
    }

    private void SaveClick(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NameBox.Text))
        {
            MessageBox.Show("Inserisci il nome.", "Errore", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (!decimal.TryParse(PriceBox.Text.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var price) || price <= 0)
        {
            MessageBox.Show("Prezzo non valido.", "Errore", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        Result = new ProductDto
        {
            Name = NameBox.Text.Trim(),
            Description = string.IsNullOrWhiteSpace(DescBox.Text) ? null : DescBox.Text.Trim(),
            Price = price,
            Category = CategoryBox.Text.Trim()
        };
        DialogResult = true;
    }

    private void CancelClick(object sender, RoutedEventArgs e) => DialogResult = false;
}