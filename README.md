# PuestosApi — Backend

ASP.NET Core 8 Minimal API for managing **Puestos de Trabajo** (job positions) with separated **Modalidad** (work location) and **Jornada** (work schedule) fields.

---

## Stack

| Technology | Version |
|---|---|
| ASP.NET Core (Minimal API) | .NET 8 |
| Entity Framework Core | 8.0.11 |
| SQLite | via EF Core provider |
| FluentValidation | 11.3.0 |
| Swagger (Swashbuckle) | 6.5.0 |

---

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

---

## Run locally

```bash
cd backend/PuestosApi
dotnet restore
dotnet run
```

- **API**: http://localhost:5217
- **Swagger UI**: http://localhost:5217/swagger

The SQLite database (`puestos.db`) is created automatically on first run. Five seed records are inserted if the database is empty.

---

## Project structure

```
PuestosApi/
├── Program.cs              # App config + all endpoints (Minimal API)
├── Models/
│   ├── Enums.cs            # Nivel, Modalidad, Jornada enums
│   └── Puesto.cs           # Main entity
├── DTOs/
│   └── PuestoDto.cs        # Read, Create, Update DTOs
├── Data/
│   ├── AppDbContext.cs      # EF Core DbContext + model config
│   └── DbSeeder.cs         # Seed data + legacy migration logic
├── Validators/
│   └── PuestoCreateValidator.cs  # FluentValidation rules
├── Properties/
│   └── launchSettings.json
├── Dockerfile              # For Render/Docker deployment
├── appsettings.json
└── PuestosApi.csproj
```

---

## API Endpoints

| Method | Route | Description |
|---|---|---|
| `GET` | `/api/puestos` | List all (filters: `?nombre=`, `?activo=`) |
| `GET` | `/api/puestos/{id}` | Get by ID |
| `POST` | `/api/puestos` | Create new |
| `PUT` | `/api/puestos/{id}` | Update existing |
| `DELETE` | `/api/puestos/{id}` | Soft delete (sets `Activo = false`) |

---

## Entity: Puesto

| Field | Type | Required | Notes |
|---|---|---|---|
| `Id` | `int` | yes | PK, auto-increment |
| `Area` | `string` | yes | e.g. "Gerencia de Operaciones" |
| `Nombre` | `string` | yes | Position name |
| `Nivel` | `enum` | yes | Jr, Sr, Lider, Gerente, SinNivel |
| `Modalidad` | `enum` | yes | Presencial, Remoto, Hibrido |
| `Jornada` | `enum` | yes | TiempoCompleto, MedioTiempo |
| `SalarioReferencia` | `decimal?` | no | MXN/month, must be ≥ 0 |
| `Activo` | `bool` | yes | Soft delete flag |
| `FechaCreacion` | `DateTime` | yes | UTC |

---

## Business rules

- **Unique constraint**: No two active puestos with the same `Nombre` + `Area` (case-insensitive).
- **Modalidad + Jornada are independent**: any combination is valid (e.g. Remoto + Medio tiempo).
- **SalarioReferencia** must be ≥ 0 if provided.
- **Soft delete**: `DELETE` sets `Activo = false`, does not remove the record.

---

## Legacy migration rule (seed)

Records that had `"MedioTiempo"` as the old single Modalidad field are migrated to:
- `Modalidad = Presencial`
- `Jornada = MedioTiempo`

All other records keep their original Modalidad and default to `Jornada = TiempoCompleto`.

This logic is in `Data/DbSeeder.cs`.

---

## CORS configuration

Allowed origins are configurable via `appsettings.json` or environment variables:

```json
{
  "AllowedOrigins": ["http://localhost:5173", "https://your-frontend.netlify.app"]
}
```

Or via env vars: `AllowedOrigins__0=https://your-frontend.netlify.app`

Defaults to `http://localhost:5173` and `http://127.0.0.1:5173` if not configured.

---

## Docker

```bash
cd backend/PuestosApi
docker build -t puestos-api .
docker run -p 10000:10000 puestos-api
```

API will be at http://localhost:10000.
