<div align="center">

<br/>

# ✈️ Online Travel Booking API

### *An enterprise-grade, full-featured travel platform backend built on .NET 10*

<br/>

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com)
[![C#](https://img.shields.io/badge/C%23-12.0-239120?style=for-the-badge&logo=csharp&logoColor=white)](https://learn.microsoft.com/en-us/dotnet/csharp/)
[![EF Core](https://img.shields.io/badge/EF_Core-10.0-388E3C?style=for-the-badge&logo=databricks&logoColor=white)](https://learn.microsoft.com/en-us/ef/core/)
[![JWT](https://img.shields.io/badge/JWT-Bearer-000000?style=for-the-badge&logo=jsonwebtokens&logoColor=white)](https://jwt.io)
[![Stripe](https://img.shields.io/badge/Stripe-Payments-635BFF?style=for-the-badge&logo=stripe&logoColor=white)](https://stripe.com)
[![Swagger](https://img.shields.io/badge/Swagger-OpenAPI-85EA2D?style=for-the-badge&logo=swagger&logoColor=black)](https://swagger.io)
[![MediatR](https://img.shields.io/badge/MediatR-CQRS-FF6B35?style=for-the-badge)](https://github.com/jbogard/MediatR)
[![FluentValidation](https://img.shields.io/badge/FluentValidation-Active-C8102E?style=for-the-badge)](https://fluentvalidation.net)

<br/>

[![GitHub Stars](https://img.shields.io/github/stars/BolesGamel123/OnlineTravelBookingAPP?style=social)](https://github.com/BolesGamel123/OnlineTravelBookingAPP)
[![GitHub Forks](https://img.shields.io/github/forks/BolesGamel123/OnlineTravelBookingAPP?style=social)](https://github.com/BolesGamel123/OnlineTravelBookingAPP/forks)
[![GitHub Issues](https://img.shields.io/github/issues/BolesGamel123/OnlineTravelBookingAPP?style=social)](https://github.com/BolesGamel123/OnlineTravelBookingAPP/issues)

<br/>

**[Overview](#-overview) • [Architecture](#-architecture--design-patterns) • [ERD](#-database-entity-relationship-diagram) • [Features](#-feature-modules) • [Security](#-security--authentication) • [Getting Started](#-getting-started) • [API Reference](#-api-reference) • [Contributing](#-contribution-guidelines)**

</div>

---

## 📖 Overview

The **Online Travel Booking API** is the backbone of a comprehensive, production-ready travel agency platform. It provides secure, high-performance RESTful endpoints that cover the complete lifecycle of a traveler's experience — from user authentication and admin catalog management, to booking complex tiered tours, hotel reservations, car rentals, flight bookings, and Stripe-powered payment processing.

Built on a **Modular Monolith** paradigm, the system strictly adheres to **Clean Architecture** and **Vertical Slice** principles. Every feature module is self-contained, highly testable, and designed to be independently extractable into a microservice as the platform scales.

---

## 🏛️ Architecture & Design Patterns

### Clean Architecture Layer Overview

```mermaid
%%{init: {'theme': 'default', 'themeVariables': {'fontSize': '17px'}}}%%
graph LR
    %% Left: Presentation Layer %%
    subgraph WEB ["🌐 Web API (Presentation)"]
        direction TB
        CTRL(["     🎮 Controllers     "])
        MIDW(["     🛡️ Middleware     "])
        DOCS(["     📖 Swagger OpenAPI     "])
        CTRL ~~~ MIDW ~~~ DOCS
    end

    %% Center: Application Core %%
    subgraph CORE ["💎 Application Core (Zero External Dependencies)"]
        direction TB
        
        subgraph APP ["⚙️ Application Layer"]
            direction LR
            CQRS{{"     📨 CQRS Handlers     "}}
            VAL{{"     ✅ Validators     "}}
            MAP{{"     🗺️ DTO Mappers     "}}
            CQRS ~~~ VAL ~~~ MAP
        end
        
        subgraph DOM ["🛡️ Domain Layer"]
            direction LR
            ENT[("     📦 Entities     ")]
            ENM>"     🔖 Enums     "]
            BASE(["     🧱 Base Classes     "])
            ENT ~~~ ENM ~~~ BASE
        end
        
        APP -->|Uses| DOM
    end

    %% Right: Infrastructure Layer %%
    subgraph INF ["🔌 Infrastructure (Data & Services)"]
        direction TB
        DB[("     🗄️ AppDbContext     ")]
        UOW(["     🔗 IUnitOfWork     "])
        SVC[["     💳 External Services     "]]
        DB ~~~ UOW ~~~ SVC
    end

    %% Dependency Arrows (Clean Architecture Rule: Point Inward) %%
    WEB ====>|Depends On| APP
    INF ====>|Implements| APP
    INF ====>|Persists| DOM
```

### Request Pipeline

```mermaid
sequenceDiagram
    participant Client
    participant Controller
    participant MediatR
    participant Validator as FluentValidation Behavior
    participant Handler as Command or Query Handler
    participant UoW as IUnitOfWork
    participant DB as SQL Server

    Client->>Controller: HTTP Request
    Controller->>MediatR: Send(Command/Query)
    MediatR->>Validator: Validate Request
    alt Validation Fails
        Validator-->>Client: 400 Bad Request + Errors
    else Validation Passes
        Validator->>Handler: Handle(Request)
        Handler->>UoW: Repository Operations
        UoW->>DB: EF Core Query / SaveChanges
        DB-->>UoW: Result
        UoW-->>Handler: Entities
        Handler-->>Controller: ApiResponse wrapped result
        Controller-->>Client: HTTP Response
    end
```

### Folder Structure (Vertical Slice)

```
OnlineTravelBookingAPP/
├── 🛡️ Domain/
│   ├── Common/          ← BaseEntity, AuditableEntity
│   ├── Entities/        ← 33 domain entities
│   └── Enums/           ← BookingStatus, TourStatus, FavoriteCategory ...
│
├── ⚙️ Application/
│   ├── Common/
│   │   ├── Behaviors/   ← MediatR Pipeline Behaviors (e.g., ValidationBehavior)
│   │   ├── Exceptions/  ← Custom Domain and Application Exceptions
│   │   ├── Interfaces/  ← IUnitOfWork, IRepository<T>, IJwtTokenGenerator ...
│   │   ├── Mappings/    ← AutoMapper profiles
│   │   ├── Models/      ← ApiResponse<T>, GenericResult<T>
│   │   ├── Pagination/  ← PaginatedList<T> and pagination helpers
│   │   ├── Patterns/    ← Common design pattern implementations
│   │   ├── Services/    ← Application-level shared services
│   │   └── Settings/    ← Strongly-typed configuration objects
│   └── Features/        ← One folder per vertical slice
│       ├── Auth/            Commands: Login | Register | RefreshToken | Logout
│       ├── Tours/           Commands: Create | Update | Delete | Queries: GetAll | GetById
│       ├── TourSchedules/   Commands: Create | AdminCancel
│       ├── TourBookings/    Commands: Create | Cancel | Update | Queries: GetAll | GetById
│       ├── Favorites/       Commands: Add | Remove | Queries: GetMy | Check
│       ├── Hotels/          Handlers: Search | Details | Create | Update | Delete
│       ├── HotelBooking/    Full booking lifecycle
│       ├── Rooms/           Room management + availability
│       ├── Flights/         Flight catalog
│       ├── Passengers/      Profile management
│       └── Payments/        Stripe payment intents + webhooks
│
├── 🔌 Infrastructure/
│   ├── Persistence/     ← AppDbContext, Repository, UnitOfWork
│   ├── Services/        ← CurrentUserService, StripeService, SlugService ...
│   └── Migrations/      ← EF Core migration history
│
└── 🌐 OnlineTravelBooking/
    ├── Controllers/     ← 12 API controllers
    └── Middleware/      ← Global exception handling
```

---

## 🗃️ Database Entity Relationship Diagram

### 👤 Users & Identity

```mermaid
erDiagram
    role {
        int id PK
        string name
    }
    passenger {
        long id PK
        int role_id FK
        string name
        string email
        string password_hash
        string phone
        int location_id FK
        bool is_email_verified
        string refreshToken
        datetime refresh_token_expiry
        string status
    }
    location {
        int id PK
        string city
        string country
    }
    role ||--o{ passenger : "has role"
    location ||--o{ passenger : "lives in"
```

### 🗺️ Tours Domain

```mermaid
erDiagram
    tour {
        long id PK
        string title
        string slug
        string summary
        int duration_days
        int location_id FK
        string difficulty
        string status
        bool is_deleted
        datetime cancelled_at
    }
    tour_price_tier {
        long id PK
        long tour_id FK
        string name
        decimal adult_price
        decimal child_price
    }
    tour_schedule {
        long id PK
        long tour_id FK
        long price_tier_id FK
        date start_date
        date end_date
        int capacity
        int available_slots
        bool is_cancelled
        datetime cancelled_at
        string cancellation_reason
    }
    tour_booking {
        long id PK
        long booking_id FK
        long tour_schedule_id FK
        int adults_count
        int children_count
        int infants_count
    }
    tour_image {
        long id PK
        long tour_id FK
        string url
    }
    tour_inclusion {
        long id PK
        long tour_id FK
        string description
    }
    booking {
        long id PK
        long passenger_id FK
        string type
        string status
        decimal total_price
        string booking_reference
        datetime cancelled_at
        string cancellation_reason
    }

    tour ||--o{ tour_price_tier : "has tiers"
    tour ||--o{ tour_schedule : "has schedules"
    tour ||--o{ tour_image : "has images"
    tour ||--o{ tour_inclusion : "has inclusions"
    tour_price_tier ||--o{ tour_schedule : "prices"
    tour_schedule ||--o{ tour_booking : "booked via"
    booking ||--|| tour_booking : "is a"
```

### 🏨 Hotels Domain

```mermaid
erDiagram
    hotel {
        long id PK
        string name
        string slug
        int location_id FK
        string status
    }
    room {
        long id PK
        long hotel_id FK
        string name
        string status
        decimal price
    }
    room_availability {
        long id PK
        long room_id FK
        date date
        int available_units
        decimal price_override
    }
    room_extra {
        long id PK
        long room_id FK
        string name
        decimal price
    }
    hotel_booking {
        long id PK
        long booking_id FK
        long hotel_id FK
        long room_id FK
        date check_in
        date check_out
    }
    hotel_image {
        long id PK
        long hotel_id FK
        string url
    }

    hotel ||--o{ room : "has rooms"
    hotel ||--o{ hotel_image : "has images"
    hotel ||--o{ hotel_booking : "booked via"
    room ||--o{ room_availability : "has availability"
    room ||--o{ room_extra : "has extras"
    booking ||--|| hotel_booking : "is a"
```

### ✈️ Flights & 🚗 Cars Domain

```mermaid
erDiagram
    flight {
        long id PK
        string airline
        string origin
        string destination
        datetime departure
        datetime arrival
        decimal price
    }
    flight_booking {
        long id PK
        long booking_id FK
        long flight_id FK
    }
    flight_booking_passenger {
        long id PK
        long flight_booking_id FK
        string name
        string passport_number
    }
    car {
        long id PK
        long brand_id FK
        long category_id FK
        string name
    }
    car_pricing_tier {
        long id PK
        long car_id FK
        int min_days
        int max_days
        decimal price_per_day
    }
    car_booking {
        long id PK
        long booking_id FK
        long car_id FK
        date start_date
        date end_date
        decimal total_price
    }

    flight ||--o{ flight_booking : "booked via"
    flight_booking ||--o{ flight_booking_passenger : "has passengers"
    booking ||--|| flight_booking : "is a"
    car ||--o{ car_pricing_tier : "has tiers"
    car ||--o{ car_booking : "booked via"
    booking ||--|| car_booking : "is a"
```

### 🌟 Cross-Cutting Concerns

```mermaid
erDiagram
    passenger {
        long id PK
        string name
        string email
    }
    booking {
        long id PK
        long passenger_id FK
        string type
        string status
        decimal total_price
        string booking_reference
    }
    favorite {
        long id PK
        long passenger_id FK
        string category
        long entity_id
    }
    payment {
        long id PK
        long passenger_id FK
        decimal amount
        string status
        string stripe_payment_intent_id
    }
    review {
        long id PK
        long passenger_id FK
        long entity_id
        int rating
        string comment
    }

    passenger ||--o{ booking : "makes"
    passenger ||--o{ favorite : "has"
    passenger ||--o{ payment : "makes"
    passenger ||--o{ review : "writes"
```

---

## 🚀 Feature Modules

| # | Module | Routes | Access |
|---|---|---|---|
| 1 | 🔐 **Auth** | `/api/auth` | Public |
| 2 | 👤 **Passengers** | `/api/passengers` | 🔒 Bearer |
| 3 | 🌟 **Favorites** | `/api/favorites` | 🔒 Bearer |
| 4 | 🗺️ **Tours** (Read) | `/api/tours` | Public |
| 5 | 🗺️ **Tours** (Write) | `/api/admin/tours` | 🔴 Admin |
| 6 | 🗓️ **Tour Schedules** | `/api/admin/tour-schedules` | 🔴 Admin |
| 7 | 🎫 **Tour Bookings** | `/api/tour-bookings` | 🔒 Bearer |
| 8 | 🏨 **Hotels** | `/api/hotels` | 🔒 Bearer |
| 9 | 🛏️ **Rooms** | `/api/rooms` | 🔒 Bearer |
| 10 | 🏨 **Hotel Bookings** | `/api/hotel-bookings` | 🔒 Bearer |
| 11 | ✈️ **Flights & Bookings** | `/api/flights` + `/api/flight-bookings` | 🔒 Bearer |
| 12 | 🚗 **Car Bookings** | `/api/car-bookings` | 🔒 Bearer |
| 13 | 💳 **Payments** | `/api/payments` | 🔒 Bearer |

### Key Highlights

- **🗓️ Tour Schedules** — Unique constraint `(tour_id, start_date, end_date)` prevents duplicates with `409 Conflict`. Admin cancel cascades to all active passenger bookings atomically.
- **🎫 Tour Bookings** — Tiered pricing engine: `(adults × adult_price) + (children × child_price)`. Inventory tracked via `available_slots`.
- **🌟 Favorites** — Supports Tours, Hotels, Flights, and Cars via `FavoriteCategory` enum. N+1-free projections.
- **💳 Payments** — Stripe payment intent creation + async webhook event handling.

---

## 🔐 Security & Authentication

```mermaid
flowchart LR
    A[Client] -->|"POST /auth/login"| B[AuthController]
    B --> C{Credentials Valid?}
    C -->|No| D[401 Unauthorized]
    C -->|Yes| E[Generate JWT + RefreshToken]
    E --> F[Store RefreshToken in DB]
    F --> G[Return AccessToken + RefreshToken]
    G --> A

    A -->|"POST /auth/refresh-token"| H[RefreshTokenCommand]
    H --> I{Token Valid and Not Expired?}
    I -->|No| D
    I -->|Yes| J[Rotate: Issue New Pair]
    J --> F
```

| Feature | Detail |
| :--- | :--- |
| **Algorithm** | JWT Bearer with HS256 signing |
| **Identity Resolution** | `ICurrentIUserService` reads from `ClaimTypes.NameIdentifier` — never from the request body |
| **Token Rotation** | Every `/refresh-token` call issues a brand new `accessToken` + `refreshToken`, invalidating the old pair |
| **Access Token** | Long JWT string containing encoded claims (user ID, email, role) |
| **Refresh Token** | Short 32-character secure random string stored in the database |
| **Roles** | `Passenger` (default) and `Admin` — seeded via EF Core migration |

---

## 💻 Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- SQL Server LocalDB *(bundled with Visual Studio)* **or** SQL Server Developer Edition

### 1. Clone the Repository

```bash
git clone https://github.com/BolesGamel123/OnlineTravelBookingAPP.git
cd OnlineTravelBookingAPP
```

### 2. Configure the Application

Review `appsettings.Development.json` and update for your environment:

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

> ⚠️ The migration `20260712092244_SeedDefaultRoles` seeds the `Admin` and `Passenger` roles. Always apply migrations **before** registering users.

### 4. Build & Launch

```bash
# Build
dotnet build OnlineTravelBooking/OnlineTravelBooking.csproj

# Run
dotnet run --project OnlineTravelBooking
```

Navigate to **[http://localhost:5183/swagger](http://localhost:5183/swagger)** to explore the interactive OpenAPI documentation.

---

## 📋 API Reference

### Unified Response Envelope

Every endpoint returns a typed `ApiResponse<T>`:

```json
{
  "success": true,
  "statusCode": 200,
  "message": "Tour booking created successfully.",
  "data": {
    "bookingReference": "TOUR-A3B7C9D1",
    "status": "Confirmed",
    "totalPrice": 1250.00
  },
  "errors": null
}
```

### Auth Endpoints

| Method | Route | Description |
|---|---|---|
| `POST` | `/api/auth/register` | Register a new passenger |
| `POST` | `/api/auth/login` | Login and receive JWT + refresh token |
| `POST` | `/api/auth/refresh-token` | Rotate tokens using a valid refresh token |
| `POST` | `/api/auth/logout` | Invalidate refresh token |

### Tour Endpoints

| Method | Route | Auth | Description |
|---|---|---|---|
| `GET` | `/api/tours` | Public | Paginated tour catalog |
| `GET` | `/api/tours/{id}` | Public | Tour details with schedules & pricing |
| `POST` | `/api/admin/tours` | Admin | Create a new tour |
| `PUT` | `/api/admin/tours/{id}` | Admin | Update tour details |
| `DELETE` | `/api/admin/tours/{id}` | Admin | Soft-delete a tour (cascades) |
| `POST` | `/api/admin/tours/{tourId}/schedules` | Admin | Add a schedule to a tour |
| `POST` | `/api/admin/tour-schedules/{id}/cancel` | Admin | Cancel schedule + cascade bookings |

---

## 🤝 Contribution Guidelines

When contributing, adhere strictly to the **Vertical Slice Architecture**:

```
Application/Features/{FeatureName}/
├── Commands/
│   └── {ActionName}/
│       └── {ActionName}Command.cs   ← Record + Validator + Handler (one file)
├── Queries/
│   └── {ActionName}/
│       └── {ActionName}Query.cs     ← Record + Validator + Handler (one file)
├── DTOs/                            ← Response/Read models
└── Requests/                        ← Controller input models
```

### Golden Rules

1. ✅ **Always use `IUnitOfWork`** — Never inject `AppDbContext` directly into handlers.
2. ✅ **Always validate** — Every command/query must have an `AbstractValidator<T>`.
3. ✅ **Always wrap responses** — Return `ApiResponse<T>` from every controller action.
4. ✅ **Enums as strings** — All enums use `JsonStringEnumConverter`.
5. ✅ **No ID spoofing** — Resolve the current user from `ICurrentIUserService`, never from request body.

---

<div align="center">
  <br/>

  ```
  Built with ❤️ using .NET 10 · Clean Architecture · CQRS · MediatR · EF Core
  ```

  <sub>© 2026 Online Travel Booking API — All Rights Reserved</sub>

  <br/>
</div>
