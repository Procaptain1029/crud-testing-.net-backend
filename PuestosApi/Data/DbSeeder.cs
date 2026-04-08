using PuestosApi.Models;

namespace PuestosApi.Data;

/// <summary>
/// Seeds initial data including legacy migration simulation.
/// Legacy rule: records that had "MedioTiempo" as Modalidad are migrated
/// to Modalidad=Presencial + Jornada=MedioTiempo. All others keep their
/// Modalidad and default to Jornada=TiempoCompleto.
/// </summary>
public static class DbSeeder
{
    public static void Seed(AppDbContext context)
    {
        if (context.Puestos.Any()) return;

        // Simulated legacy records — before the fix, these had a single "Modalidad" field
        var legacyRecords = new[]
        {
            new { Nombre = "Técnico de Impresión", Area = "Gerencia de Operaciones", Nivel = Nivel.Sr, ModalidadLegacy = "Presencial", Salario = (decimal?)12000m },
            new { Nombre = "Ejecutiva Corporativa", Area = "Gerencia de Ventas", Nivel = Nivel.Sr, ModalidadLegacy = "Remoto", Salario = (decimal?)16000m },
            new { Nombre = "Ingeniero en Sistemas", Area = "Desarrollo de Sistemas", Nivel = Nivel.Sr, ModalidadLegacy = "Hibrido", Salario = (decimal?)20000m },
            // Legacy record with "MedioTiempo" as modalidad — must be migrated
            new { Nombre = "Asistente de Administración", Area = "Gerencia de Administración", Nivel = Nivel.Jr, ModalidadLegacy = "MedioTiempo", Salario = (decimal?)9000m },
            new { Nombre = "Coordinador de RRHH", Area = "Gerencia de Administración", Nivel = Nivel.Sr, ModalidadLegacy = "Presencial", Salario = (decimal?)12000m },
        };

        foreach (var legacy in legacyRecords)
        {
            // Migration rule: "MedioTiempo" as Modalidad → Presencial + MedioTiempo
            Modalidad modalidad;
            Jornada jornada;

            if (legacy.ModalidadLegacy == "MedioTiempo")
            {
                modalidad = Modalidad.Presencial;
                jornada = Jornada.MedioTiempo;
            }
            else
            {
                modalidad = Enum.Parse<Modalidad>(legacy.ModalidadLegacy);
                jornada = Jornada.TiempoCompleto;
            }

            context.Puestos.Add(new Puesto
            {
                Nombre = legacy.Nombre,
                Area = legacy.Area,
                Nivel = legacy.Nivel,
                Modalidad = modalidad,
                Jornada = jornada,
                SalarioReferencia = legacy.Salario,
                Activo = true,
                FechaCreacion = DateTime.UtcNow
            });
        }

        context.SaveChanges();
    }
}
