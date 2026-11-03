# Travel Agency System

![CI](https://github.com/dolganiuc-nicoleta/TravelAgencySystem/actions/workflows/ci.yml/badge.svg)

A desktop travel-agency management system built with **.NET 8 / WPF (MVVM)**, **Entity Framework Core + PostgreSQL**, and a companion **ASP.NET Core Minimal API**. It supports three roles: clients, travel agents, and administrators. Each role has its own workspace.

The project was also built as a practical showcase of **classic GoF design patterns** applied to a realistic domain (see [Design patterns](#design-patterns)).

## Features

**Client**
- Browse and filter travel packages, with destination photos and hotel details
- Customize a trip with optional extras and submit booking requests
- Track bookings, receive status notifications, keep a favorites list
- Message the agency; manage profile and contact details

**Agent**
- Dashboard with bookings, clients and activity overview
- Create packages through a multi-step wizard or a quick-create form, with live destination lookup (GeoDB), hotel search (SerpApi) and destination imagery (Wikipedia/Wikimedia)
- Approve or reject booking requests; generate PDF reports (QuestPDF)

**Admin**
- User management: change roles, block/unblock accounts
- Package moderation and bookings control
- Analytics overview with cancellation rate, plus snapshot save/restore to compare against live data
- Financial settings (discount / VAT) and report export

**Account security**
- Passwords hashed with PBKDF2-SHA256 (600k iterations, per-user salt); legacy hashes are upgraded transparently on login
- Password reset via SMS (Twilio) or e-mail link, handled server-side by the API so secrets never ship with the desktop client

## Architecture

```
TravelAgency.WPF     Desktop client: Views, ViewModels, commands, converters, DI container, mediator
       │
       ▼
TravelAgency.Core    Domain models, design patterns, services, validators (FluentValidation),
                     EF Core DbContext + repositories + migrations, external API adapters
       ▲
       │
TravelAgency.Api     Minimal API: password reset flow, destination images & hotel search
                     (with disk cache), SSRF-safe image proxy for the WPF client

TravelAgency.Tests   xUnit tests for the domain logic and security-sensitive code
```

## Design patterns

| Category | Pattern | Where |
|---|---|---|
| Creational | Builder + Director | `Patterns/Builders`: assembling trip packages |
| | Abstract Factory | `Patterns/Factories/AbstractFactory`: budget vs. premium trip components |
| | Factory Method | `Patterns/Factories/FactoryMethod`: bus / plane / train transport |
| | Prototype | `Patterns/Prototypes`: cloning trip packages |
| | Singleton | `Patterns/Singletons/AppConfig`, `SessionManager` |
| Structural | Adapter | `Patterns/Adapters`: GeoDB and SerpApi behind domain interfaces |
| | Bridge | `Patterns/Bridge`: itinerary abstraction vs. standard / premium implementation |
| | Composite | `Patterns/Composite`: grouped extra services |
| | Decorator | `Patterns/Decorator`: insurance, guided tour, airport transfer, free cancellation |
| | Facade | `Patterns/Facades/TravelPackageFacade` |
| | Flyweight | `Patterns/Flyweight`: shared package info |
| | Proxy | `Services/BookingAccessProxy`: role-based booking access |
| Behavioral | Chain of Responsibility | `Patterns/ChainOfResponsibility`: booking approval checks |
| | Iterator | `Patterns/Iterator`: filtered booking collections |
| | Mediator | `WPF/Messaging`: decoupled window navigation |
| | Memento | `Patterns/Memento`: admin analytics snapshots |
| | Observer | `Patterns/Observer`: booking status notifications |
| | State | `Patterns/State`: booking lifecycle (pending → confirmed / rejected / cancelled) |
| | Strategy | `Patterns/Strategy`: pricing (standard / discount / full with VAT) |
| | Visitor | `Visitors/ExtraServices`: extra-service summaries |
| | Command | `WPF/Commands`: relay / async commands, admin actions |

## Getting started

### Prerequisites
- .NET 8 SDK (Windows, for WPF)
- A PostgreSQL database (the project uses Supabase, but any PostgreSQL instance works)

### Configuration
Connection strings live in `TravelAgency.WPF/App.config` and `TravelAgency.Api/appsettings.json` **without the password**. All secrets come from environment variables or [user secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets):

| Variable | Used for | Required |
|---|---|---|
| `TRAVEL_AGENCY_DB_PASSWORD` | PostgreSQL password (WPF and API) | yes |
| `RAPIDAPI_KEY` | GeoDB city/country lookup | for package creation |
| `SERPAPI_API_KEY` | Hotel search and images | for package creation |
| `TWILIO_ACCOUNT_SID`, `TWILIO_AUTH_TOKEN`, `TWILIO_FROM_PHONE` | SMS password reset | optional |
| `SMTP_USER`, `SMTP_PASS` | E-mail password reset | optional |

### Run
```bash
dotnet build TravelAgencySystem.sln
dotnet run --project TravelAgency.WPF
```
In Debug builds the WPF app starts the API automatically on `http://localhost:5280`, applies pending EF Core migrations, and seeds two demo accounts. Demo accounts are never created in Release builds.

| Role | E-mail | Password |
|---|---|---|
| Agent | `agent1@travelagency.demo` | `Agent123!` |
| Client | `client1@travelagency.demo` | `Client123!` |

### Tests
```bash
dotnet test TravelAgency.Tests
```

## Tech stack
C# 12 · .NET 8 · WPF · MVVM · Microsoft.Extensions.DependencyInjection · EF Core 8 · PostgreSQL (Npgsql) · ASP.NET Core Minimal API · FluentValidation · QuestPDF · Twilio · xUnit · GitHub Actions
