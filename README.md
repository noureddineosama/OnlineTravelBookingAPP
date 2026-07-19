<div align="center">

<br/>

# ✈️ Online Travel Booking API

### *An enterprise-grade, full-featured travel platform backend built on .NET 10*

<br/>

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-12.0-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
![EF Core](https://img.shields.io/badge/EF_Core-10.0-388E3C?style=for-the-badge&logo=nuget&logoColor=white)
![JWT](https://img.shields.io/badge/JWT-Bearer-000000?style=for-the-badge&logo=jsonwebtokens&logoColor=white)
![Stripe](https://img.shields.io/badge/Stripe-Payments-635BFF?style=for-the-badge&logo=stripe&logoColor=white)
![Swagger](https://img.shields.io/badge/Swagger-OpenAPI-85EA2D?style=for-the-badge&logo=swagger&logoColor=black)

<br/>

[![GitHub Stars](https://img.shields.io/github/stars/BolesGamel123/OnlineTravelBookingAPP?style=social)](https://github.com/BolesGamel123/OnlineTravelBookingAPP)
[![GitHub Forks](https://img.shields.io/github/forks/BolesGamel123/OnlineTravelBookingAPP?style=social)](https://github.com/BolesGamel123/OnlineTravelBookingAPP/forks)

<br/>

**[Overview](#-overview) • [Architecture](#-architecture--design-patterns) • [Features](#-feature-modules) • [Security](#-security--authentication) • [Getting Started](#-getting-started) • [API Endpoints](#-api-endpoints) • [Contributing](#-contribution-guidelines)**

</div>

---

## 📖 Overview

The **Online Travel Booking API** is the comprehensive, production-ready backend of a full-scale travel agency platform. It provides secure, high-performance RESTful endpoints covering the complete lifecycle of a traveler's experience — from registering and authenticating users, managing admin catalogs, and booking complex multi-schedule tours, to hotel reservations, car bookings, and flight management.

Built on a **Modular Monolith** architecture, the system strictly adheres to **Clean Architecture** and **Vertical Slice** principles. Every feature module is self-contained, highly testable, and designed to be independently extractable into a microservice as the platform scales.

---

## 🏛️ Architecture & Design Patterns

This application is engineered around industry-standard enterprise patterns to guarantee long-term maintainability, testability, and strict separation of concerns.

### Layer Diagram

```
┌───────────────────────────────────────────┐
│              🌐 Web API Layer              │
│   Controllers • Middleware • Swagger UI    │
├───────────────────────────────────────────┤
│           ⚙️  Application Layer           │
│  CQRS • MediatR • FluentValidation •      │
│  AutoMapper • DTOs • Validators           │
├───────────────────────────────────────────┤
│          🔌 Infrastructure Layer          │
│  AppDbContext • IUnitOfWork •             │
│  IRepository<T> • JWT • Stripe •          │
│  EF Core Migrations                       │
├───────────────────────────────────────────┤
│             🛡️ Domain Layer              │
│   Entities • Enums • Domain Exceptions    │
│        (Zero external dependencies)       │
└───────────────────────────────────────────┘
```

### Key Patterns

| Pattern | Implementation |
| :--- | :--- |
| 🏗️ **Clean Architecture** | Strict layer dependency rules enforced via project references |
| ✂️ **Vertical Slice** | Each feature lives in its own `Features/{Name}/` slice |
| 📬 **CQRS** | Commands (write) and Queries (read) are fully separated via MediatR |
| 🗂️ **Repository + Unit of Work** | `IRepository<T>` and `IUnitOfWork` abstract all database access |
| ✅ **Validation Pipeline** | `FluentValidation` wired via `ValidationBehavior<TRequest, TResponse>` |
| 🗺️ **Object Mapping** | `AutoMapper` profiles for clean DTO ↔ Entity transformations |
| 📦 **Response Envelope** | Every endpoint returns a typed `ApiResponse<T>` wrapper |
| 🔒 **Zero-Trust Auth** | User identity extracted from JWT claims, never from request body |

### Request Pipeline

```
Client Request
    ↓
[Controller]
    ↓
[MediatR Send]
    ↓
[ValidationBehavior — FluentValidation]
    ↓
[Command / Query Handler]
    ↓
[IUnitOfWork → IRepository<T>]
    ↓
[Entity Framework Core → SQL Server]
    ↓
[ApiResponse<T>] → Client
```

### Unified API Response Format

Every single endpoint — success or failure — returns a consistent, predictable `ApiResponse<T>` envelope:

```json
{
  "success": true,
  "statusCode": 200,
  "message": "Tour booking created successfully.",
  "data": {
    "bookingId": "TOUR-A3B7C9D1",
    "status": "Confirmed",
    "totalPrice": 1250.00
  },
  "errors": null
}
```

---

## 🚀 Feature Modules

The platform is composed of **12 fully-implemented feature modules**, each following the Vertical Slice pattern.

---

### 🔐 Auth
> `POST /api/auth/register` · `POST /api/auth/login` · `POST /api/auth/refresh-token` · `POST /api/auth/logout`

- Secure **JWT Bearer** stateless authentication.
- Full **token rotation**: each refresh generates a new `accessToken` + `refreshToken` pair, invalidating the old one.
- Passwords are stored with a **custom `IPasswordHasher`** implementation.
- Role-based seeding: `Passenger` and `Admin` roles are pre-seeded via EF Core migration.

---

### 👤 Passengers
> `GET /api/passengers` · `GET /api/passengers/{id}` · `PUT /api/passengers/{id}`

- Allows registered users to view and update their own passenger profile.
- Admin-accessible list of all registered passengers.

---

### 🌟 Favorites
> `POST /api/favorites` · `DELETE /api/favorites/{id}` · `GET /api/favorites/my-favorites` · `GET /api/favorites/check`

- Universal favorites system supporting **Tours, Hotels, Flights, and Cars** via the `FavoriteCategory` enum.
- **N+1-free:** Single repository query with projections across all categories.
- Entity existence and status validated before saving.

---

### 🗺️ Tours  *(Admin-Managed)*
> `POST /api/admin/tours` · `PUT /api/admin/tours/{id}` · `DELETE /api/admin/tours/{id}` · `GET /api/tours` · `GET /api/tours/{id}`

- Full **CRUD** for tour catalog management, restricted to `Admin` role for write operations.
- Tours include rich metadata: images, inclusions, price tiers, and schedules.
- Cascading soft-delete: deleting a tour gracefully removes all associated schedules, bookings, price tiers, and inclusions.
- Paginated listing with filters via `GetAllToursQuery`.

---

### 🗓️ Tour Schedules *(Admin-Managed)*
> `POST /api/admin/tours/{tourId}/schedules` · `POST /api/admin/tour-schedules/{scheduleId}/cancel`

- Admins can attach multiple schedules to a tour, each with its own capacity and pricing.
- **Unique constraint** enforced on `(tour_id, start_date, end_date)` to prevent duplicate schedules (returns `409 Conflict`).
- **Admin Cancel**: Cancelling a schedule cascades to all active bookings for that schedule, marking them as `Cancelled` with a reason and timestamp — ensuring full data integrity.

---

### 🎫 Tour Bookings
> `POST /api/tour-bookings` · `GET /api/tour-bookings/my-bookings` · `GET /api/tour-bookings/{id}` · `PUT /api/tour-bookings/{id}` · `DELETE /api/tour-bookings/{id}/cancel`

- Full booking lifecycle for passengers: **Create → View → Cancel**.
- **Smart Inventory:** `available_slots` are automatically decremented on booking and restored on cancellation.
- **Tiered Pricing Engine:** Total price calculated as `(adults × adult_price) + (children × child_price)` from the schedule's price tier.
- Cancelled bookings are hidden from user-facing queries.

---

### 🏨 Hotels
> `GET /api/hotels` · `GET /api/hotels/{id}` · `POST /api/hotels` · `PUT /api/hotels/{id}` · `DELETE /api/hotels/{id}`

- Full hotel catalog management with images, locations, and amenities.
- Advanced search with filters (location, dates, guest count).
- Slug generation via `IGenerateSlug` for SEO-friendly hotel URLs.

---

### 🛏️ Rooms
> `POST /api/rooms` · `GET /api/hotels/{hotelId}/rooms` · `GET /api/rooms/{id}` · `PUT /api/rooms/{id}` · `DELETE /api/rooms/{id}`

- Detailed room management with status tracking via `RoomStatus` enum.
- Room extras (add-ons) support.
- Room availability tracking with date-level granularity via `room_availability`.
- Dynamic **per-night pricing** via `ICalculateNightPrice` and `ICalculateNumberOfNights`.

---

### 🏨 Hotel Bookings
> `POST /api/hotel-bookings` · `GET /api/hotel-bookings/my-bookings` · `GET /api/hotel-bookings/{id}` · `DELETE /api/hotel-bookings/{id}/cancel`

- Full hotel booking lifecycle with check-in / check-out date validation.
- Automatic availability checks before booking confirmation.
- Room availability status updated upon booking and cancellation.

---

### ✈️ Flights & Flight Bookings
> `GET /api/flights` · `POST /api/flight-bookings` · `GET /api/flight-bookings/my-bookings`

- Flight catalog with origin, destination, airline, and schedule data.
- Multi-passenger flight booking support via `flight_booking_passenger`.

---

### 🚗 Car Bookings
> `POST /api/car-bookings` · `GET /api/car-bookings/my-bookings` · `GET /api/car-bookings/{id}` · `DELETE /api/car-bookings/{id}/cancel`

- Car rental booking with dynamic pricing tiers (`car_pricing_tier`).
- Support for car extras and add-ons.

---

### 💳 Payments
> `POST /api/payments/create-intent` · `POST /api/payments/confirm` · `POST /api/payments/webhook`

- Integrated **Stripe** payment processing via `IStripeService`.
- Create payment intents and confirm payments for any booking type.
- Stripe webhook support for asynchronous payment event handling.

---

## 🔐 Security & Authentication

| Feature | Detail |
| :--- | :--- |
| **Algorithm** | JWT Bearer with HS256 signing |
| **Identity Resolution** | `ICurrentIUserService` reads `UserId` & `Email` from `ClaimTypes.NameIdentifier` / `ClaimTypes.Email` — never from request body |
| **Token Rotation** | Every `/refresh-token` call issues a new `accessToken` + `refreshToken`, invalidating the previous pair |
| **Role Claim** | Extracted from `passenger.role.name`; defaults to `"Passenger"` |
| **Admin Protection** | Admin-only endpoints use `[Authorize(Roles = "Admin")]` |
| **Public Endpoints** | `GET /api/tours`, all auth endpoints are publicly accessible |

---

## 💻 Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- SQL Server LocalDB *(bundled with Visual Studio)* **or** SQL Server Developer Edition.

### 1. Clone the Repository

```bash
git clone https://github.com/BolesGamel123/OnlineTravelBookingAPP.git
cd OnlineTravelBookingAPP
```

### 2. Configure the Application

Review and update `appsettings.Development.json` with your local settings:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=OnlineTravelBooking;"
  },
  "JwtSettings": {
    "SecretKey": "your-super-secret-key-here",
    "Issuer": "OnlineTravelBookingAPI",
    "Audience": "OnlineTravelBookingClient",
    "ExpirationInMinutes": 60
  }
}
```

### 3. Apply Database Migrations

```bash
dotnet ef database update -p Infrastructure -s OnlineTravelBooking
```

> ⚠️ The migration `20260712092244_SeedDefaultRoles` seeds the `Admin` and `Passenger` roles. Always apply migrations before registering users.

### 4. Build & Launch

```bash
# Build the solution
dotnet build OnlineTravelBooking/OnlineTravelBooking.csproj

# Run the API
dotnet run --project OnlineTravelBooking
```

Navigate to **[http://localhost:5183/swagger](http://localhost:5183/swagger)** to explore the fully interactive OpenAPI documentation.

---

## 📋 API Endpoints Summary

| Module | Base Route | Auth Required |
| :--- | :--- | :---: |
| 🔐 Auth | `/api/auth` | ❌ Public |
| 👤 Passengers | `/api/passengers` | ✅ Bearer |
| 🌟 Favorites | `/api/favorites` | ✅ Bearer |
| 🗺️ Tours (Read) | `/api/tours` | ❌ Public |
| 🗺️ Tours (Write) | `/api/admin/tours` | 🔴 Admin Only |
| 🗓️ Tour Schedules | `/api/admin/tour-schedules` | 🔴 Admin Only |
| 🎫 Tour Bookings | `/api/tour-bookings` | ✅ Bearer |
| 🏨 Hotels | `/api/hotels` | ✅ Bearer |
| 🛏️ Rooms | `/api/rooms` | ✅ Bearer |
| 🏨 Hotel Bookings | `/api/hotel-bookings` | ✅ Bearer |
| ✈️ Flights | `/api/flights` | ✅ Bearer |
| ✈️ Flight Bookings | `/api/flight-bookings` | ✅ Bearer |
| 🚗 Car Bookings | `/api/car-bookings` | ✅ Bearer |
| 💳 Payments | `/api/payments` | ✅ Bearer |

---

## 🤝 Contribution Guidelines

When contributing new features, adhere strictly to the **Vertical Slice Architecture**:

```
Application/
 └── Features/
      └── {FeatureName}/
           ├── Commands/
           │    └── {ActionName}/
           │         └── {ActionName}Command.cs   ← Record + Validator + Handler
           ├── Queries/
           │    └── {ActionName}/
           │         └── {ActionName}Query.cs     ← Record + Validator + Handler
           ├── DTOs/                              ← Request/Response models
           └── Requests/                          ← Controller input models
```

### Golden Rules

1. ✅ **Always use `IUnitOfWork`** — Never inject `IApplicationDbContext` or `AppDbContext` directly into handlers.
2. ✅ **Always validate** — Every command and query must have a corresponding `AbstractValidator<T>`.
3. ✅ **Always use `ApiResponse<T>`** — Every controller action must return a consistent envelope.
4. ✅ **Enums as strings** — All enums are serialized as strings via `JsonStringEnumConverter`.
5. ✅ **No ID spoofing** — Always resolve the current user via `ICurrentIUserService` from JWT claims.

---

<div align="center">
  <br/>
  <p>Built with ❤️ using <strong>.NET 10</strong>, <strong>Clean Architecture</strong>, and enterprise-grade patterns.</p>
  <sub>© 2026 Online Travel Booking API — All Rights Reserved</sub>
  <br/><br/>
</div>
