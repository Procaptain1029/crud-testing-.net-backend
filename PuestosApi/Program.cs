using System.Text.Json.Serialization;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PuestosApi.Data;
using PuestosApi.DTOs;
using PuestosApi.Models;
using PuestosApi.Validators;

var builder = WebApplication.CreateBuilder(args);

// ── JSON: accept enums as strings ──
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// ── EF Core + SQLite ──
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=puestos.db"));

// ── FluentValidation ──
builder.Services.AddScoped<IValidator<PuestoCreateDto>, PuestoCreateValidator>();
builder.Services.AddScoped<IValidator<PuestoUpdateDto>, PuestoUpdateValidator>();

// ── CORS ──
var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()
    ?? new[] { "http://localhost:5173", "http://127.0.0.1:5173" };

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// ── Swagger ──
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ── Middleware ──
app.UseCors();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ── DB init + seed ──
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    DbSeeder.Seed(db);
}

// ── Mapping helper ──
static PuestoReadDto ToReadDto(Puesto p) => new(
    p.Id,
    p.Area,
    p.Nombre,
    p.Nivel.ToString(),
    p.Modalidad.ToString(),
    p.Jornada.ToString(),
    p.SalarioReferencia,
    p.Activo,
    p.FechaCreacion
);

// ═══════════════════════════════════════════
// ENDPOINTS
// ═══════════════════════════════════════════

// GET /api/puestos — list with optional filters
app.MapGet("/api/puestos", async (AppDbContext db, string? nombre, bool? activo) =>
{
    var query = db.Puestos.AsQueryable();

    if (!string.IsNullOrWhiteSpace(nombre))
        query = query.Where(p => p.Nombre.ToLower().Contains(nombre.ToLower()));

    if (activo.HasValue)
        query = query.Where(p => p.Activo == activo.Value);

    var puestos = await query.OrderByDescending(p => p.FechaCreacion).ToListAsync();
    return Results.Ok(puestos.Select(ToReadDto));
});

// GET /api/puestos/{id}
app.MapGet("/api/puestos/{id:int}", async (int id, AppDbContext db) =>
{
    var puesto = await db.Puestos.FindAsync(id);
    return puesto is null
        ? Results.Problem("Puesto no encontrado.", statusCode: 404)
        : Results.Ok(ToReadDto(puesto));
});

// POST /api/puestos
app.MapPost("/api/puestos", async (PuestoCreateDto dto, IValidator<PuestoCreateDto> validator, AppDbContext db) =>
{
    var validation = await validator.ValidateAsync(dto);
    if (!validation.IsValid)
    {
        return Results.ValidationProblem(validation.ToDictionary());
    }

    // Check unique constraint: no duplicate active Nombre+Area (case-insensitive)
    var exists = await db.Puestos.AnyAsync(p =>
        p.Activo &&
        p.Nombre.ToLower() == dto.Nombre.ToLower() &&
        p.Area.ToLower() == dto.Area.ToLower());

    if (exists)
    {
        return Results.Problem(
            detail: $"Ya existe un puesto activo con el nombre '{dto.Nombre}' en el área '{dto.Area}'.",
            statusCode: 409,
            title: "Conflicto de duplicado"
        );
    }

    var puesto = new Puesto
    {
        Area = dto.Area,
        Nombre = dto.Nombre,
        Nivel = dto.Nivel,
        Modalidad = dto.Modalidad,
        Jornada = dto.Jornada,
        SalarioReferencia = dto.SalarioReferencia,
        Activo = true,
        FechaCreacion = DateTime.UtcNow
    };

    db.Puestos.Add(puesto);
    await db.SaveChangesAsync();

    return Results.Created($"/api/puestos/{puesto.Id}", ToReadDto(puesto));
});

// PUT /api/puestos/{id}
app.MapPut("/api/puestos/{id:int}", async (int id, PuestoUpdateDto dto, IValidator<PuestoUpdateDto> validator, AppDbContext db) =>
{
    var validation = await validator.ValidateAsync(dto);
    if (!validation.IsValid)
    {
        return Results.ValidationProblem(validation.ToDictionary());
    }

    var puesto = await db.Puestos.FindAsync(id);
    if (puesto is null)
        return Results.Problem("Puesto no encontrado.", statusCode: 404);

    // Check unique constraint (excluding current record)
    var exists = await db.Puestos.AnyAsync(p =>
        p.Id != id &&
        p.Activo &&
        p.Nombre.ToLower() == dto.Nombre.ToLower() &&
        p.Area.ToLower() == dto.Area.ToLower());

    if (exists)
    {
        return Results.Problem(
            detail: $"Ya existe un puesto activo con el nombre '{dto.Nombre}' en el área '{dto.Area}'.",
            statusCode: 409,
            title: "Conflicto de duplicado"
        );
    }

    puesto.Area = dto.Area;
    puesto.Nombre = dto.Nombre;
    puesto.Nivel = dto.Nivel;
    puesto.Modalidad = dto.Modalidad;
    puesto.Jornada = dto.Jornada;
    puesto.SalarioReferencia = dto.SalarioReferencia;

    await db.SaveChangesAsync();
    return Results.Ok(ToReadDto(puesto));
});

// DELETE /api/puestos/{id} — soft delete
app.MapDelete("/api/puestos/{id:int}", async (int id, AppDbContext db) =>
{
    var puesto = await db.Puestos.FindAsync(id);
    if (puesto is null)
        return Results.Problem("Puesto no encontrado.", statusCode: 404);

    puesto.Activo = false;
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.Run();
