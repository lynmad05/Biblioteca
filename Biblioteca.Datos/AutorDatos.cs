using Biblioteca.Entidades;

namespace Biblioteca.Datos;

public class AutorDatos
{
    public Task<List<Autor>> ListarAsync() =>
        ConexionBD.ConsultarAsync(
            "SELECT AutorId, Nombre, Nacionalidad, Activo FROM Autores WHERE Activo = 1 ORDER BY Nombre",
            dr => new Autor
            {
                AutorId = (int)dr["AutorId"],
                Nombre = (string)dr["Nombre"],
                Nacionalidad = dr["Nacionalidad"] as string,
                Activo = (bool)dr["Activo"]
            });
}