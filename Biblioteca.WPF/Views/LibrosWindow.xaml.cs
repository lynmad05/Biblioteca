using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Biblioteca.Entidades;
using Biblioteca.Negocio;
using Biblioteca.WPF.Helpers;

namespace Biblioteca.WPF.Views;

public partial class LibrosWindow : Window
{
    private readonly LibroNegocio _negocio = new();
    private int _libroId;   // 0 = libro nuevo

    public LibrosWindow()
    {
        InitializeComponent();
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        await UI.EjecutarAsync(async () =>
        {
            cmbAutor.ItemsSource = await _negocio.ListarAutoresAsync();
            await CargarAsync();
        });
    }

    private async Task CargarAsync()
    {
        dgLibros.ItemsSource = await _negocio.BuscarAsync(txtBuscar.Text);
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

    private void DgLibros_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (dgLibros.SelectedItem is not Libro libro)
            return;

        _libroId = libro.LibroId;
        txtTitulo.Text = libro.Titulo;
        txtIsbn.Text = libro.ISBN;
        cmbAutor.SelectedValue = libro.AutorId;
        txtEjemplares.Text = libro.Ejemplares.ToString();
        txtModo.Text = "Editar libro";
    }

    private void LimpiarFormulario()
    {
        _libroId = 0;
        txtTitulo.Clear();
        txtIsbn.Clear();
        txtEjemplares.Clear();
        cmbAutor.SelectedIndex = -1;
        dgLibros.SelectedItem = null;
        txtModo.Text = "Nuevo libro";
    }

    private void BtnNuevo_Click(object sender, RoutedEventArgs e)
    {
        LimpiarFormulario();
        txtTitulo.Focus();
    }

    private async void BtnGuardar_Click(object sender, RoutedEventArgs e)
    {
        await UI.EjecutarAsync(async () =>
        {
            if (!int.TryParse(txtEjemplares.Text, out var ejemplares))
            {
                UI.Advertencia("Ingrese un número entero válido en ejemplares.");
                return;
            }

            var libro = new Libro
            {
                LibroId = _libroId,
                Titulo = txtTitulo.Text,
                ISBN = txtIsbn.Text,
                AutorId = cmbAutor.SelectedValue is int autorId ? autorId : 0,
                Ejemplares = ejemplares
            };

            if (_libroId == 0)
                await _negocio.InsertarAsync(libro);
            else
                await _negocio.ActualizarAsync(libro);

            UI.Info("Libro guardado correctamente.");
            LimpiarFormulario();
            await CargarAsync();
        });
    }

    private async void BtnEliminar_Click(object sender, RoutedEventArgs e)
    {
        await UI.EjecutarAsync(async () =>
        {
            if (_libroId == 0)
            {
                UI.Advertencia("Seleccione un libro de la lista.");
                return;
            }

            if (!UI.Confirmar("¿Desea dar de baja el libro seleccionado?"))
                return;

            await _negocio.EliminarAsync(_libroId);

            UI.Info("Libro dado de baja correctamente.");
            LimpiarFormulario();
            await CargarAsync();
        });
    }
}