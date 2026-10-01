using System.Windows;
using System.Windows.Controls;
using Biblioteca.Entidades;
using Biblioteca.Negocio;
using Biblioteca.WPF.Helpers;

namespace Biblioteca.WPF.Views;

public partial class DevolucionWindow : Window
{
    private readonly SocioNegocio _socios = new();
    private readonly PrestamoNegocio _prestamos = new();

    public DevolucionWindow()
    {
        InitializeComponent();
        txtReglas.Text = $"Multa por retraso: S/ {PrestamoNegocio.MultaPorDia:N2} por día después de la fecha límite.";
        txtFecha.Text = DateTime.Today.ToString("dd/MM/yyyy");
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        await UI.EjecutarAsync(async () =>
        {
            cmbSocio.ItemsSource = await _socios.ListarAsync();
        });
    }

    private async Task CargarPendientesAsync()
    {
        if (cmbSocio.SelectedValue is not int socioId)
        {
            dgPendientes.ItemsSource = null;
            return;
        }

        dgPendientes.ItemsSource = await _prestamos.ListarPendientesPorSocioAsync(socioId);
        ActualizarMulta();
    }

    private async void CmbSocio_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        await UI.EjecutarAsync(CargarPendientesAsync);
    }

    private void DgPendientes_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ActualizarMulta();
    }

    // Vista previa de la multa con la fecha de hoy
    private void ActualizarMulta()
    {
        if (dgPendientes.SelectedItem is not PrestamoReporte item)
        {
            txtDias.Text = "-";
            txtMulta.Text = "S/ 0.00";
            return;
        }

        var dias = PrestamoNegocio.CalcularDiasRetraso(item.FechaLimite, DateTime.Today);
        var multa = PrestamoNegocio.CalcularMulta(item.FechaLimite, DateTime.Today);

        txtDias.Text = dias.ToString();
        txtMulta.Text = $"S/ {multa:N2}";
    }

    private async void BtnDevolver_Click(object sender, RoutedEventArgs e)
    {
        await UI.EjecutarAsync(async () =>
        {
            if (dgPendientes.SelectedItem is not PrestamoReporte item)
            {
                UI.Advertencia("Seleccione el libro que se está devolviendo.");
                return;
            }

            var resultado = await _prestamos.RegistrarDevolucionAsync(item.PrestamoId, item.LibroId);

            var mensaje = $"Devolución registrada: {item.Libro}\n" +
                          $"Días de retraso: {resultado.DiasRetraso}\n" +
                          $"Multa: S/ {resultado.Multa:N2}";

            if (resultado.PrestamoCerrado)
                mensaje += $"\n\nNo quedan libros pendientes: el préstamo N° {item.PrestamoId} pasó a Devuelto.";

            UI.Info(mensaje);
            await CargarPendientesAsync();
        });
    }
}