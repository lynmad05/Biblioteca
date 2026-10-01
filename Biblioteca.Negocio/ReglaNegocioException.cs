namespace Biblioteca.Negocio;

/// <summary>
/// Excepción propia de la capa de Negocio. Se lanza cuando se incumple una regla
/// y su mensaje está pensado para mostrarse tal cual al usuario.
/// </summary>
public class ReglaNegocioException : Exception
{
    public ReglaNegocioException(string mensaje) : base(mensaje) { }

    public ReglaNegocioException(string mensaje, Exception inner) : base(mensaje, inner) { }
}