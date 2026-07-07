# Online Travel Booking App API

A robust, scalable backend for an Online Travel Booking application built with **.NET 10** using a **Modular Monolith** and **Clean Architecture**.

## 🏗️ Architecture & Technologies

This project strictly adheres to the principles of Clean Architecture and Vertical Slice Architecture within the Application layer to ensure separation of concerns, testability, and long-term maintainability.

- **Framework**: .NET 10.0 (ASP.NET Core Web API)
- **Database**: SQL Server (LocalDB for development) & Entity Framework Core 10
- **Patterns**:
  - Clean Architecture (Domain, Application, Infrastructure, API)
  - CQRS (Command Query Responsibility Segregation) with MediatR
  - Repository & Unit of Work (Generic implementation)
- **Validation**: FluentValidation
- **Mapping**: AutoMapper
- **API Documentation**: Swagger / OpenAPI

## 📁 Project Structure

```text
OnlineTravelBookingAPP.slnx
├── Domain/                   # Core business rules, Entities (32 total), and Base models
├── Application/              # Use cases (CQRS), DTOs, Validation, and Abstractions
├── Infrastructure/           # EF Core Data Access, DbContext, Repository, UnitOfWork
└── OnlineTravelBooking/      # ASP.NET Core API, Controllers, Middleware, Swagger
```

## ✨ Key Features

- **Decoupled Architecture**: No dependencies on external frameworks in the Domain layer.
- **Pagination System**: Built-in, reusable generic pagination wrapper (`PagedResult<T>`) applied across all list endpoints.
- **Base Entity Hierarchy**: Structured inheritance (`AuditableEntity`, `CreatedAtEntity`, `BaseIntEntity`) to handle audit trails automatically.
- **Global Error Handling**: Standardized `ApiResponse<T>` envelope for all responses, with central exception handling middleware (returns 400 for Validation, 404 for Not Found).

## 🚀 Getting Started

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- SQL Server (or SQL Server Express LocalDB)

### Setup & Run

1. **Clone the repository:**
   ```bash
   git clone https://github.com/BolesGamel123/OnlineTravelBookingAPP.git
   cd OnlineTravelBookingAPP
   ```

2. **Update Database Connection:**
   Verify the `DefaultConnection` in `OnlineTravelBooking/appsettings.json` points to your active SQL Server instance.

3. **Apply Entity Framework Migrations:**
   ```bash
   dotnet ef database update -p Infrastructure -s OnlineTravelBooking
   ```

4. **Run the API:**
   ```bash
   dotnet run --project OnlineTravelBooking
   ```

5. **Explore Endpoints:**
   Open a browser and navigate to `http://localhost:5183/swagger` to view and test the interactive API documentation.

## 🤝 Contribution Guidelines
This project is currently under active development. Ensure all new features are built using the Vertical Slice pattern within the `Application/Features` directory.

---
*Developed with modern .NET best practices.*
