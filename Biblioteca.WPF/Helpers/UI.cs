using System.Windows;
using Biblioteca.Negocio;

namespace Biblioteca.WPF.Helpers;

/// <summary>
/// Utilidades de la capa de presentación. Centraliza el manejo de errores:
/// solo se capturan ReglaNegocioException (mensaje claro) y errores inesperados.
/// Esta capa no conoce ningún tipo de acceso a datos.
/// </summary>
public static class UI
{
    public static async Task EjecutarAsync(Func<Task> accion)
    {
        try
        {
            await accion();
        }
        catch (ReglaNegocioException ex)
        {
            Advertencia(ex.Message);
        }
        catch (Exception ex)
        {
            Error($"Ocurrió un error inesperado: {ex.Message}");
        }
    }

    public static void Info(string mensaje) =>
        MessageBox.Show(mensaje, "Biblioteca", MessageBoxButton.OK, MessageBoxImage.Information);

    public static void Advertencia(string mensaje) =>
        MessageBox.Show(mensaje, "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);

    public static void Error(string mensaje) =>
        MessageBox.Show(mensaje, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

    public static bool Confirmar(string mensaje) =>
        MessageBox.Show(mensaje, "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
}