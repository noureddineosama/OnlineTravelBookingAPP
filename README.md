<div align="center">
  <h1>🌍 Online Travel Booking API</h1>
  <p>
    <strong>A robust, scalable backend for an Online Travel Booking platform built with .NET 10, Clean Architecture, and CQRS.</strong>
  </p>

  ![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=for-the-badge&logo=dotnet)
  ![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
  ![EF Core](https://img.shields.io/badge/EF_Core-10.0-388E3C?style=for-the-badge&logo=nuget&logoColor=white)
  ![Clean Architecture](https://img.shields.io/badge/Clean_Architecture-Solid-FF9800?style=for-the-badge)
</div>

<br />

## 📖 Overview

The Online Travel Booking API is a comprehensive backend system designed to handle the core operations of a modern travel agency. It provides secure, high-performance endpoints for managing users (passengers), booking tours, and curating personal favorites across various travel categories (Tours, Hotels, Flights, and Cars).

Built on top of a **Modular Monolith** design, the application strictly adheres to **Clean Architecture** and **Vertical Slice** principles, ensuring that the codebase is highly testable, maintainable, and ready to scale into microservices if needed.

---

## 🏗️ Architecture & Core Patterns

This project relies on industry-standard enterprise patterns to ensure long-term stability and separation of concerns.

### 1. Clean Architecture Layers

| Layer | Responsibility |
|---|---|
| **Domain** | Core business entities (`BaseEntity`, `AuditableEntity`), Enums, and domain exceptions. Zero external dependencies. |
| **Application** | Use cases implemented via **CQRS**, MediatR pipelines, DTOs, FluentValidation, and AutoMapper. |
| **Infrastructure** | Database access via `AppDbContext`, Generic Repository pattern, Unit of Work, and JWT/Security implementations. |
| **API** | ASP.NET Core Web API, Controllers, Global Exception Middleware, and Swagger documentation. |

### 2. CQRS with MediatR
Every single API request follows a strict pipeline:
`Controller → MediatR → ValidationBehavior (FluentValidation) → Command/Query Handler → DbContext`

### 3. Unified Response Wrapper
To ensure frontend clients always receive a predictable response, every endpoint is wrapped in an `ApiResponse<T>`:
```json
{
  "success": true,
  "message": "Tour booking created successfully.",
  "data": { "bookingId": "TOUR-A3B7C9D1", "status": "Confirmed" },
  "errors": null
}
```

---

## 🔐 Security & Authentication

The API uses **Stateless JWT Bearer Authentication**. 

- **Secure by Default:** Endpoints are protected via the `[Authorize]` attribute.
- **Identity Resolution:** The API never trusts client-provided User IDs. Instead, the `ICurrentIUserService` safely extracts the `UserId` directly from the validated JWT token claims, completely eliminating ID spoofing vulnerabilities.
- **Role-Based Access:** Built-in support for `Passenger` and `Admin` roles.

---

## ✨ Key Features & Endpoints

### 🌟 Universal Favorites (`/api/favorites`)
A highly optimized, multi-category favorites system. Users can favorite Tours, Hotels, Flights, and Cars.
*   **Batch Fetching:** Eliminates N+1 queries by instantly projecting relational data into read-ready DTOs.
*   **Validation:** Automatically verifies that the requested item exists and is in an `active` state before saving.

### 🗓️ Tour Bookings (`/api/tour-bookings`)
Complete lifecycle management for booking travel tours.
*   **Inventory Management:** Safely decrements `available_slots` upon booking and restores them upon cancellation.
*   **Dynamic Pricing:** Automatically calculates total prices based on tiered schedules `(adults × price) + (children × price)`.
*   **Secure Tracking:** Retrieve a paginated list of bookings tied exclusively to the logged-in user via `/api/tour-bookings/my-bookings`.

---

## 🚀 Getting Started

### Prerequisites
*   [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
*   SQL Server LocalDB (comes pre-installed with Visual Studio) or SQL Server Developer Edition.

### 1. Clone & Setup
```bash
git clone https://github.com/BolesGamel123/OnlineTravelBookingAPP.git
cd OnlineTravelBookingAPP
```

### 2. Database Migration
The application uses `(localdb)\MSSQLLocalDB` for seamless local development. Apply the EF Core migrations to build the schema:
```bash
dotnet ef database update -p Infrastructure -s OnlineTravelBooking
```

### 3. Run the Application
```bash
dotnet run --project OnlineTravelBooking
```
Once running, navigate to `http://localhost:5183/swagger` to explore the interactive OpenAPI documentation.

---

## 🧪 Testing with Postman

We have included a fully configured Postman collection to instantly test the API without writing any client code.

1. **Import:** Open Postman and import the `FavouriteTourBooking.postman_collection.json` file located in the root directory.
2. **Register & Login:** Open the `🔐 Authentication` folder. Run **Register User** first, then **Login**. 
3. **Automated Tokens:** The Login request has a built-in Postman script that automatically captures your JWT token and injects it into all other requests.
4. **Test:** You can now instantly run `Add Tour to Favourites` or `Create Tour Booking` and receive `201 Created` responses!

---

## 🤝 Contributing
When adding new features, please adhere to the **Vertical Slice Architecture** within the `Application` layer:
```text
Application/Features/{FeatureName}/
  ├── DTOs/           ← Data Transfer Objects
  ├── Commands/       ← Command, Validator, and Handler per write operation
  └── Queries/        ← Query and Handler per read operation
```

<div align="center">
  <sub>Built with modern .NET 10 best practices.</sub>
</div>
