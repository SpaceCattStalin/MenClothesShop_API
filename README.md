# MenClothesShop_API — e-commerce backend

**Context.** Backend API for a men’s clothing shop: authentication, catalog/categories, product details, cart, orders, payments, chat, and supporting services.

## What this repo is

- **ASP.NET Core** solution (`MenClothesShop_API.sln`) with projects for API + services + repositories + common code.
- Uses a custom `AppDbContext` plus seeding on startup.
- Integrations:
  - **SignalR** hub (`/hub`) for chat / realtime updates.
  - **MinIO / S3** compatible storage (via `IAmazonS3`) for images/media.
  - **Geocoding** client (VietMap) for address/map features.

## How it works (high level)

- API registers services like cart, inventory, size, orders, payments, and image handling.
- Endpoints are mapped using a mix of controllers + feature endpoint mappers (see `API/Program.cs`).
- Static files are served (for assets hosted by the API), and the SignalR hub is exposed at `/hub`.
- Configuration is loaded from environment (`DotNetEnv.Env.Load()`), so secrets/connection strings are expected in an `.env` / environment variables.

## Run locally (typical)

```bash
dotnet restore
dotnet run --project API/API.csproj
```

Make sure required env vars are set (DB connection, MinIO keys, etc.) before running.
