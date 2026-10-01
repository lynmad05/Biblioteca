using System.Windows;
using Biblioteca.Negocio;
using Biblioteca.WPF.Helpers;

namespace Biblioteca.WPF.Views;

public partial class ReporteWindow : Window
{
    private readonly PrestamoNegocio _prestamos = new();

    public ReporteWindow()
    {
        InitializeComponent();
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        dpDesde.SelectedDate = DateTime.Today.AddMonths(-1);
        dpHasta.SelectedDate = DateTime.Today;
        await UI.EjecutarAsync(GenerarAsync);
    }

    private async void BtnGenerar_Click(object sender, RoutedEventArgs e)
    {
        await UI.EjecutarAsync(GenerarAsync);
    }

    private async Task GenerarAsync()
    {
        if (dpDesde.SelectedDate is not DateTime desde || dpHasta.SelectedDate is not DateTime hasta)
        {
            UI.Advertencia("Seleccione la fecha inicial y la fecha final.");
            return;
        }

        var lista = await _prestamos.ReporteAsync(desde, hasta);

        dgReporte.ItemsSource = lista;
        txtTotal.Text = lista.Count == 0
            ? "No se encontraron préstamos en el intervalo seleccionado."
            : $"{lista.Count} registro(s) encontrado(s).";
    }
}