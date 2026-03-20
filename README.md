# Payments API

A .NET 10 Web API for processing payments with idempotency support, built with ASP.NET Core, Entity Framework Core, and SQL Server.

## Features

- **Create payments** via `POST /api/payments` with amount, currency, and optional narration
- **Idempotency** — duplicate requests with the same `Idempotency-Key` header return the original payment instead of creating a new one
- **Retrieve payments** by ID (`GET /api/payments/{id}`) or by reference (`GET /api/payments?reference=...`)
- **Structured logging** with Serilog
- **OpenAPI** document available in development at `/openapi/v1.json`

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server (local or remote)

## Getting Started

1. **Clone the repository**

   ```bash
   git clone <repository-url>
   cd payments-api
   ```

2. **Update the connection string**

   Edit `PaymentsApi.Api/appsettings.json` to point to your SQL Server instance:

   ```json
   {
     "ConnectionStrings": {
       "PaymentsDb": "Server=localhost;Database=PaymentsDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True"
     }
   }
   ```

3. **Apply database migrations**

   ```bash
   dotnet ef database update --project PaymentsApi.Api
   ```

4. **Run the application**

   ```bash
   dotnet run --project PaymentsApi.Api
   ```

## API Endpoints

### Create a Payment

```http
POST /api/payments
Content-Type: application/json
Idempotency-Key: <unique-key>

{
  "amount": 5000.00,
  "currency": "NGN",
  "narration": "Invoice #1234"
}
```

### Get Payment by ID

```http
GET /api/payments/{id}
```

### Get Payment by Reference

```http
GET /api/payments?reference=PAY-...
```

## Project Structure

```
PaymentsApi.Api/
├── Contracts/          # Request and response DTOs
├── Controllers/        # API controllers
├── Data/               # EF Core DbContext and configuration
├── Domain/             # Domain entities
└── Program.cs          # Application entry point
```

## Tech Stack

- .NET 10 / ASP.NET Core
- Entity Framework Core 10 (SQL Server)
- Serilog (structured logging)
- Swagger / OpenAPI
