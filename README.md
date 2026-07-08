# Online Travel Booking App API

A robust, scalable backend for an Online Travel Booking application built with **.NET 10** using a **Modular Monolith** and **Clean Architecture**.

---

## 🏗️ Architecture & Technologies

This project strictly adheres to Clean Architecture and Vertical Slice Architecture within the Application layer to ensure separation of concerns, testability, and long-term maintainability.

| Layer | Responsibility |
|---|---|
| **Domain** | Core business entities, base models, no external dependencies |
| **Application** | Use cases (CQRS), DTOs, FluentValidation, AutoMapper, MediatR pipeline |
| **Infrastructure** | EF Core DbContext, Generic Repository, Unit of Work |
| **API** | ASP.NET Core Controllers, Middleware, Swagger |

- **Framework**: .NET 10.0 (ASP.NET Core Web API)
- **Database**: SQL Server (LocalDB for development) & Entity Framework Core 10
- **Patterns**: Clean Architecture · Vertical Slice Architecture · CQRS with MediatR · Generic Repository & Unit of Work
- **Validation**: FluentValidation (auto-pipeline via `ValidationBehavior`)
- **Mapping**: AutoMapper
- **API Documentation**: Swagger / OpenAPI

---

## 📁 Project Structure

```text
OnlineTravelBookingAPP.slnx
├── Domain/                        # 32 Entities, BaseEntity hierarchy, AuditableEntity
├── Application/
│   ├── Common/                    # ApiResponse<T>, PagedResult<T>, Exceptions, Interfaces
│   └── Features/
│       ├── Passengers/            # Full CRUD — Create, Update, Delete, GetById, GetAll
│       ├── FavouriteTours/        # Add, Remove, List, Check
│       └── TourBookings/          # Create, Cancel, GetById, GetUserBookings
├── Infrastructure/                # AppDbContext, EfRepository, UnitOfWork
└── OnlineTravelBooking/           # Controllers, Middleware, Program.cs
```

---

## ✨ Implemented Features

### 👤 Passengers
Full CRUD management for passengers (users).

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/passengers` | List all passengers (paginated, filterable) |
| `GET` | `/api/passengers/{id}` | Get passenger by ID |
| `POST` | `/api/passengers` | Create a new passenger |
| `PUT` | `/api/passengers/{id}` | Update passenger details |
| `DELETE` | `/api/passengers/{id}` | Delete a passenger |

---

### 🌟 Favourite Tours
Allow passengers to save and manage their favourite tours.

| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/api/favourite-tours` | Add a tour to favourites |
| `DELETE` | `/api/favourite-tours` | Remove a tour from favourites |
| `GET` | `/api/favourite-tours/{userId}` | List user's favourite tours (paginated) |
| `GET` | `/api/favourite-tours/{userId}/check/{tourId}` | Check if a tour is favourited |

**Business Rules:**
- Tour must be `active` status to be favourited
- Duplicate favourites are rejected
- Returns enriched tour details (title, image, location, starting price)

---

### 🗓️ Tour Bookings
Full tour booking lifecycle — create, view, and cancel.

| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/api/tour-bookings` | Create a new tour booking |
| `PUT` | `/api/tour-bookings/{bookingId}/cancel` | Cancel a booking |
| `GET` | `/api/tour-bookings/{bookingId}` | Get booking details |
| `GET` | `/api/tour-bookings/user/{userId}` | List user's bookings (paginated, filterable by status) |

**Business Rules:**
- Schedule must be in the future
- `available_slots` checked before booking — decremented on create, restored on cancel
- Tiered pricing: `(adults × adult_price) + (children × child_price) + (infants × infant_price)`
- Booking number format: `TOUR-{8-char-GUID}` (e.g. `TOUR-A3B7C9D1`)
- Initial status: `confirmed`, payment status: `pending`

---

## 🏛️ Core Architectural Patterns

### Standardized Response Envelope
Every endpoint returns the same `ApiResponse<T>` structure:
```json
{
  "success": true,
  "message": "Success",
  "data": { },
  "errors": null
}
```

### Pagination
All list endpoints use `PagedResult<T>`:
```json
{
  "items": [],
  "totalCount": 100,
  "page": 1,
  "pageSize": 20,
  "totalPages": 5,
  "hasNextPage": true,
  "hasPreviousPage": false
}
```

### Entity Hierarchy
```
BaseEntity (long id)
  └── CreatedAtEntity (+ created_at)
        └── AuditableEntity (+ updated_at)   ← booking, passenger, tour
BaseIntEntity (int id)
  └── AuditableIntEntity (+ created_at)      ← role
```

### CQRS Pipeline (per request)
```
Controller → MediatR → ValidationBehavior (FluentValidation) → Handler → DbContext
```

---

## 🚀 Getting Started

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- SQL Server or SQL Server Express LocalDB

### Setup & Run

1. **Clone the repository:**
   ```bash
   git clone https://github.com/BolesGamel123/OnlineTravelBookingAPP.git
   cd OnlineTravelBookingAPP
   ```

2. **Verify the connection string** in `OnlineTravelBooking/appsettings.json`:
   ```json
   "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=TravelDB;Trusted_Connection=True;"
   ```

3. **Apply migrations:**
   ```bash
   dotnet ef database update -p Infrastructure -s OnlineTravelBooking
   ```

4. **Run the API:**
   ```bash
   dotnet run --project OnlineTravelBooking
   ```

5. **Explore the API:**
   Open `http://localhost:5183/swagger` for interactive Swagger documentation.

---

## 🌿 Branch Strategy

| Branch | Purpose |
|---|---|
| `initial-setup` | Base architecture — Clean Architecture scaffold, Passengers CRUD |
| `feature/favourite-tour-booking` | Favourite Tours + Tour Bookings features |

---

## 🤝 Contribution Guidelines

All new features must follow the **Vertical Slice** pattern:
```
Application/Features/{FeatureName}/
  ├── DTOs/           ← Output DTOs only
  ├── Commands/       ← Command + Validator + Handler per operation
  └── Queries/        ← Query + Handler per operation
```

---

*Built with modern .NET 10 best practices — Clean Architecture · CQRS · MediatR · FluentValidation.*
