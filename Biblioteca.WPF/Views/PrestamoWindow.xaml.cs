using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Biblioteca.Entidades;
using Biblioteca.Negocio;
using Biblioteca.WPF.Helpers;

namespace Biblioteca.WPF.Views;

public partial class PrestamoWindow : Window
{
    private readonly SocioNegocio _socios = new();
    private readonly LibroNegocio _libros = new();
    private readonly PrestamoNegocio _prestamos = new();
    private readonly ObservableCollection<Libro> _seleccion = new();

    public PrestamoWindow()
    {
        InitializeComponent();
        dgSeleccion.ItemsSource = _seleccion;
        txtReglas.Text = $"Máximo {PrestamoNegocio.MaxLibrosPendientes} libros pendientes por socio. " +
                         $"Plazo de devolución: {PrestamoNegocio.DiasPrestamo} días.";
        ActualizarResumen();
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        await UI.EjecutarAsync(async () =>
        {
            cmbSocio.ItemsSource = await _socios.ListarAsync();
            cmbLibro.ItemsSource = await _libros.ListarAsync();
        });
    }

    private void ActualizarResumen()
    {
        txtCantidad.Text = $"Libros agregados: {_seleccion.Count}";
        txtLimite.Text = $"Fecha límite de devolución: {DateTime.Today.AddDays(PrestamoNegocio.DiasPrestamo):dd/MM/yyyy}";
    }

    private async Task ActualizarPendientesAsync()
    {
        if (cmbSocio.SelectedValue is not int socioId)
        {
            txtPendientes.Text = "Seleccione un socio para ver sus libros pendientes.";
            return;
        }

        var pendientes = await _prestamos.ContarLibrosPendientesAsync(socioId);
        txtPendientes.Text = $"Libros pendientes de devolución: {pendientes} de {PrestamoNegocio.MaxLibrosPendientes} permitidos.";
    }

    private async void CmbSocio_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        await UI.EjecutarAsync(ActualizarPendientesAsync);
    }

    private void BtnAgregar_Click(object sender, RoutedEventArgs e)
    {
        if (cmbLibro.SelectedItem is not Libro libro)
        {
            UI.Advertencia("Seleccione un libro para agregar.");
            return;
        }

        if (_seleccion.Any(l => l.LibroId == libro.LibroId))
        {
            UI.Advertencia("Ese libro ya fue agregado al préstamo.");
            return;
        }

        _seleccion.Add(libro);
        ActualizarResumen();
    }

    private void BtnQuitar_Click(object sender, RoutedEventArgs e)
    {
        if (dgSeleccion.SelectedItem is not Libro libro)
        {
            UI.Advertencia("Seleccione en la tabla el libro que desea quitar.");
            return;
        }

        _seleccion.Remove(libro);
        ActualizarResumen();
    }

    private async void BtnRegistrar_Click(object sender, RoutedEventArgs e)
    {
        await UI.EjecutarAsync(async () =>
        {
            if (cmbSocio.SelectedValue is not int socioId)
            {
                UI.Advertencia("Seleccione un socio.");
                return;
            }

            var ids = _seleccion.Select(l => l.LibroId).ToList();
            var prestamoId = await _prestamos.RegistrarPrestamoAsync(socioId, ids);

            UI.Info($"Préstamo N° {prestamoId} registrado correctamente.\n" +
                    $"Fecha límite de devolución: {DateTime.Today.AddDays(PrestamoNegocio.DiasPrestamo):dd/MM/yyyy}");

            _seleccion.Clear();
            ActualizarResumen();
            cmbLibro.ItemsSource = await _libros.ListarAsync();   // el stock cambió
            await ActualizarPendientesAsync();
        });
    }
}