# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

ASP.NET Core Web API (.NET 10) for a hardware store ("ferretería"): products, categories, clients, sales, users and roles. Code, identifiers, comments and API messages are in Spanish — keep new code consistent with that. The solution file is `FerreteriaAPI.slnx` with a single project in `FerreteriaAPI/`. There is no test project.

## Commands

Run from the repo root:

```bash
dotnet build FerreteriaAPI.slnx
dotnet run --project FerreteriaAPI            # http://localhost:5226 (profile "http")
dotnet run --project FerreteriaAPI --launch-profile https   # https://localhost:7137
```

In Development, `/` redirects to Swagger UI (`/swagger`). `FerreteriaAPI/FerreteriaAPI.http` can be used for manual requests.

## Database

- SQL Server LocalDB, database `Ferreteria` (`ConnectionStrings:DefaultConnection` in `appsettings.json`).
- **Database-first**: `Models/FerreteriaContext.cs` and the entity classes were scaffolded from an existing database (`Scaffold-DbContext`); there is no `Migrations/` folder. Schema changes are made in the DB and then reflected by re-scaffolding or hand-editing the models/`OnModelCreating`. Some models were hand-edited afterward (e.g. `[Table]`/`[Key]` attributes on `Ventas`), so re-scaffolding with `--force` will overwrite those edits.
- `FerreteriaContext.OnConfiguring` still contains a hardcoded connection string from scaffolding; the DI registration in `Program.cs` (reading `DefaultConnection`) is what's actually used at runtime.
- `Data/AppDbContext.cs` is an unused leftover (not registered in DI). All controllers use `FerreteriaContext`, whose DbSet for categories is named `Categoria` (singular) while the entity class is `Categorias`.

## Architecture

- No service/repository layer: controllers inject `FerreteriaContext` directly and contain the business logic.
- Navigation properties follow scaffolded naming (`IdClienteNavigation`, `IdProductoNavigation`, `IdCategoriaNavigation`); endpoints eager-load them with `Include`/`ThenInclude`. JSON serialization uses `ReferenceHandler.IgnoreCycles` (set in `Program.cs`) to tolerate the bidirectional navigations.
- Entities are often bound directly as request bodies (e.g. `POST api/Ventas` takes a `Ventas` with nested `DetalleVenta`). Where navigation properties break model binding, a DTO is used instead (`UsuarioRegistroDto` for `POST api/Usuarios/Registro`).
- **Sale creation (`VentasController.PostVenta`)** is the core business flow: inside an explicit DB transaction it validates each product exists and has stock, sets `PrecioUnitario` from the current product price (client-sent prices/totals are ignored), decrements stock, computes `ValorTotal`, and sets `Fecha` server-side. `EnviarSmsAlAdmin` (low-stock alert at ≤10) is a stub that does nothing yet.
- Controller namespaces are inconsistent: some are in `FerreteriaAPI.Controllers`, others (e.g. `ProductosController`, `VentasController`) are in the global namespace. Route is always `api/[controller]`, so the class name determines the URL (`ProductoController.cs` defines `ProductosController` → `api/Productos`).

## Gotchas

- CORS policy `AngularPolicy` (in `Program.cs`) only allows `http://localhost:4200`; add new frontend origins there.
- There is no `.gitignore`; `bin/`, `obj/` and `.vs/` are tracked/show up in `git status`. Avoid committing build output.
- `WeatherForecastController` / `WeatherForecast.cs` are template leftovers.
- Passwords are hashed with ASP.NET Core Identity's `PasswordHasher<Usuarios>` (registered as `IPasswordHasher<Usuarios>` in DI). DTOs receive plaintext in `Contrasena`; only the hash goes in `Usuarios.ContrasenaHash`, which is `[JsonIgnore]`d. `POST api/Usuarios/Login` verifies credentials but issues no token — there is no authentication/JWT yet.
