using PuestosApi.Models;

namespace PuestosApi.DTOs;

public record PuestoReadDto(
    int Id,
    string Area,
    string Nombre,
    string Nivel,
    string Modalidad,
    string Jornada,
    decimal? SalarioReferencia,
    bool Activo,
    DateTime FechaCreacion
);

public record PuestoCreateDto(
    string Area,
    string Nombre,
    Nivel Nivel,
    Modalidad Modalidad,
    Jornada Jornada,
    decimal? SalarioReferencia
);

public record PuestoUpdateDto(
    string Area,
    string Nombre,
    Nivel Nivel,
    Modalidad Modalidad,
    Jornada Jornada,
    decimal? SalarioReferencia
);
