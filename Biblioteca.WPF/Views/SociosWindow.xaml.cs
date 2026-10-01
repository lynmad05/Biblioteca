using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Biblioteca.Entidades;
using Biblioteca.Negocio;
using Biblioteca.WPF.Helpers;

namespace Biblioteca.WPF.Views;

public partial class SociosWindow : Window
{
    private readonly SocioNegocio _negocio = new();
    private int _socioId;   // 0 = socio nuevo

    public SociosWindow()
    {
        InitializeComponent();
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        await UI.EjecutarAsync(CargarAsync);
    }

    private async Task CargarAsync()
    {
        dgSocios.ItemsSource = await _negocio.BuscarAsync(txtBuscar.Text);
    }

    private async void BtnBuscar_Click(object sender, RoutedEventArgs e)
    {
        await UI.EjecutarAsync(CargarAsync);
    }

    private async void TxtBuscar_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            await UI.EjecutarAsync(CargarAsync);
    }

    private async void BtnLimpiarBusqueda_Click(object sender, RoutedEventArgs e)
    {
        txtBuscar.Clear();
        await UI.EjecutarAsync(CargarAsync);
    }

    private void DgSocios_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (dgSocios.SelectedItem is not Socio socio)
            return;

        _socioId = socio.SocioId;
        txtDni.Text = socio.DNI;
        txtNombre.Text = socio.Nombre;
        txtEmail.Text = socio.Email;
        txtModo.Text = "Editar socio";
    }

    private void LimpiarFormulario()
    {
        _socioId = 0;
        txtDni.Clear();
        txtNombre.Clear();
        txtEmail.Clear();
        dgSocios.SelectedItem = null;
        txtModo.Text = "Nuevo socio";
    }

    private void BtnNuevo_Click(object sender, RoutedEventArgs e)
    {
        LimpiarFormulario();
        txtDni.Focus();
    }

    private async void BtnGuardar_Click(object sender, RoutedEventArgs e)
    {
        await UI.EjecutarAsync(async () =>
        {
            var socio = new Socio
            {
                SocioId = _socioId,
                DNI = txtDni.Text,
                Nombre = txtNombre.Text,
                Email = txtEmail.Text
            };

            if (_socioId == 0)
                await _negocio.InsertarAsync(socio);
            else
                await _negocio.ActualizarAsync(socio);

            UI.Info("Socio guardado correctamente.");
            LimpiarFormulario();
            await CargarAsync();
        });
    }

    private async void BtnEliminar_Click(object sender, RoutedEventArgs e)
    {
        await UI.EjecutarAsync(async () =>
        {
            if (_socioId == 0)
            {
                UI.Advertencia("Seleccione un socio de la lista.");
                return;
            }

            if (!UI.Confirmar("¿Desea dar de baja al socio seleccionado?"))
                return;

            await _negocio.EliminarAsync(_socioId);

            UI.Info("Socio dado de baja correctamente.");
            LimpiarFormulario();
            await CargarAsync();
        });
    }
}