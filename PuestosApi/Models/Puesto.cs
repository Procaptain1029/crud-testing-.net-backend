namespace PuestosApi.Models;

public class Puesto
{
    public int Id { get; set; }
    public string Area { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public Nivel Nivel { get; set; }
    public Modalidad Modalidad { get; set; }
    public Jornada Jornada { get; set; }
    public decimal? SalarioReferencia { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}
