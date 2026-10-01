namespace Biblioteca.Negocio;

/// <summary>Resultado de registrar una devolución (lo que la pantalla necesita mostrar).</summary>
public class ResultadoDevolucion
{
    public DateTime FechaDevolucion { get; init; }
    public int DiasRetraso { get; init; }
    public decimal Multa { get; init; }
    public bool PrestamoCerrado { get; init; }
}