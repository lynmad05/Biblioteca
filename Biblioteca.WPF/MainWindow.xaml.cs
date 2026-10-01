using System.Windows;
using Biblioteca.Datos;
using Biblioteca.WPF.Views;

namespace Biblioteca.WPF;

public partial class MainWindow : Window
{
    private readonly EstadisticasDatos _estadisticas = new();

    public MainWindow()
    {
        InitializeComponent();
        Loaded += async (_, _) => await CargarEstadisticasAsync();
    }

    private async Task CargarEstadisticasAsync()
    {
        try
        {
            var e = await _estadisticas.ObtenerAsync();
            TxtTotalLibros.Text = e.TotalLibros.ToString();
            TxtTotalSocios.Text = e.TotalSocios.ToString();
            TxtPrestamosActivos.Text = e.PrestamosPendientes.ToString();
            TxtDevolucionesHoy.Text = e.DevolucionesHoy.ToString();
        }
        catch (Exception ex)
        {
            MessageBox.Show("No se pudieron cargar las estadísticas:\n" + ex.Message,
                "Biblioteca", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private async void Abrir(Window ventana)
    {
        ventana.Owner = this;
        ventana.ShowDialog();
        await CargarEstadisticasAsync();
    }

    private void BtnLibros_Click(object sender, RoutedEventArgs e) => Abrir(new LibrosWindow());
    private void BtnSocios_Click(object sender, RoutedEventArgs e) => Abrir(new SociosWindow());
    private void BtnPrestamo_Click(object sender, RoutedEventArgs e) => Abrir(new PrestamoWindow());
    private void BtnDevolucion_Click(object sender, RoutedEventArgs e) => Abrir(new DevolucionWindow());
    private void BtnReporte_Click(object sender, RoutedEventArgs e) => Abrir(new ReporteWindow());
}