A production-ready ASP.NET Core MVC application for listing properties, managing bookings, and handling payments. Designed with role-based access (User, Owner, Admin), a service-layer architecture, and EF Core for data persistence.

---

## Short Description

This project implements a full-featured online booking platform where Owners publish properties, Users make reservations, and Admins moderate content and manage platform operations. The system includes a booking lifecycle (Pending → Approved → Paid), flexible cancellation and refund workflows, and integration points for payment providers.

---

## Features

### User
- Register / login using ASP.NET Identity
- Browse approved property listings
- Reserve properties (create bookings)
- View personal bookings dashboard
- Pay for approved bookings via pluggable payment providers
- Request cancellations (rules vary by booking status)

### Owner
- Create, edit and delete property listings (CRUD)
- View bookings for owned properties
- Approve or reject booking requests
- Cancel bookings for owned properties (with refund logic for paid bookings)
- Owner dashboard with recent bookings and revenue

### Advertisements
- Owners can create advertisements for properties or promoted listings
- Manage advertisement content and images
- Admin approval workflow for advertisements (approve/reject)
- Admin can view and moderate advertisements site-wide

### Admin
- Approve or reject property listings and advertisements
- View site-wide dashboards and metrics
- Cancel bookings in any state (with refund initiation support)
- Issue refunds and view payment history

---

## Technologies
- ASP.NET Core MVC (.NET 10)
- Entity Framework Core
- SQL Server
- ASP.NET Identity (roles: User, Owner, Admin)
- Dependency Injection, Service Layer pattern
- Razor Views, Bootstrap 5
- Payment provider abstraction (Stripe, PayPal, PayMob as examples)

---

## System Workflow

Booking flow (high level):

1. User reserves property → Booking status = `Pending`
2. Owner approves reservation → Booking status = `Approved`
3. User pays → Booking status = `Paid`

Cancellation & refund flow:
- `Pending` or `Approved` bookings can be cancelled by User/Owner/Admin. Status becomes `CancelledByUser`, `CancelledByOwner`, or `CancelledByAdmin` accordingly.
- If a paid booking is cancelled (by owner/admin/user with conditions), the booking becomes `RefundPending` and an admin may issue the refund. After successful refund the booking becomes `Refunded`.

---

## Status Flow Diagram (text)

Pending (User reserved)
  ├─ Owner approves → Approved
  │    └─ User pays → Paid
  │         └─ If cancelled after payment → RefundPending → (on refund) Refunded
  ├─ Owner cancels → CancelledByOwner
  └─ User cancels → CancelledByUser

Admin may cancel from any state:
  └─ CancelledByAdmin (if not paid) or RefundPending (if paid)

---

## Installation Guide

Prerequisites
- .NET SDK 10 (install from https://dotnet.microsoft.com)
- SQL Server (LocalDB or full SQL Server)
- Optional: Visual Studio 2022/2026 or VS Code

Local setup
1. Clone repository

   git clone https://github.com/MuhammedMaklad/Online-Booking-System.git

2. Configure connection string
- Open `appsettings.json` and update `DefaultConnection` to point to your SQL Server instance.
- For development, `appsettings.Development.json` may contain overrides.

3. Apply database migrations

   dotnet ef database update

If you prefer Visual Studio Package Manager Console:

   Update-Database

4. Seed data
- The application seeds initial roles and sample owner/admin users on startup (see `Program.cs`). Ensure the database is accessible.

5. Run the application

   dotnet run

Open the site at `https://localhost:5001` (or the URL shown in the console).

---

## Project Structure Overview

- `Controllers/` — MVC controllers
- `Views/` — Razor views (User, Owner, Admin, Payment flows)
- `Models/` — Domain entities and enums (Bookings, Properties, Payments)
- `Models/` — Domain entities and enums (Bookings, Properties, Payments, Advertisements)
- `Data/` — `AppDbContext` (EF Core) and migrations
- `Services/` — Business logic (BookingService, OwnerService, PaymentService, etc.)
- `Contracts/` — Service interfaces
- `ViewModels/` — DTOs used between Controllers and Views
- `Migrations/` — EF Core migrations
- `wwwroot/` — Static assets (CSS, JS, images)

---

## How It Enforces Rules
- Role-based access: `[Authorize(Roles = "Owner,Admin")]` on Owner areas, Admin-only actions are restricted using `[Authorize(Roles = "Admin")]`.
- Booking status transitions are managed in services (not controller code) to keep business rules centralized and testable.
- Availability: Only bookings with `Approved` or `Paid` statuses block dates (overlap logic is centralized in the BookingService).
- Refunds: Refund execution is guarded by transaction state (only completed transactions can be refunded) to avoid duplicate refunds.

---

## Future Improvements
- Add audit fields to `Booking` (CancelledBy, CancelledAt, CancelReason, RefundRequestedAt).
- Expose owner/admin refund workflow as background jobs with retry and reconciliation.
- Add unit and integration tests for booking and refund flows.
- Implement a tenant-safe UI for managing multiple currencies and locales.
- Add API endpoints and OpenAPI/Swagger for external integrations.

---

## Contributing
- Fork the repository, create a feature branch, and submit pull requests. Follow existing code style and patterns.
- Run `dotnet ef migrations add <Name>` for schema changes and include migrations in PR.

---

## License
This repository does not include a license file. Add one if you intend to open-source the code.

---

*Generated for a production-grade ASP.NET Core MVC sample project.*

An ASP.NET Core MVC application for online booking management with user authentication, role-based access control, and email notifications.

## Features

- **User Management**
  - User registration with email confirmation
  - Login with local credentials
  - Login with Google OAuth
  - Password reset functionality
  - Profile management (edit name, bio, phone, avatar)
  - Admin user management panel

- **Role Management**
  - Three roles: Admin, User, Guest
  - Static role seeding on startup
  - Role assignment/revocation for admins

- **Email Service**
  - Gmail SMTP integration
  - HTML email templates
  - Confirmation emails
  - Password reset emails
  - Development mode (logs to console)

- **Authentication**
  - ASP.NET Core Identity
  - Google OAuth integration
  - Account linking (local + Google)
  - Email confirmation with auto-verify

## Technology Stack

- **.NET 10.0** - Web framework
- **ASP.NET Core Identity** - Authentication
- **Entity Framework Core** - Database ORM
- **SQL Server** - Database
- **MailKit** - SMTP email client
- **Google OAuth** - External authentication

## Project Structure

```
Online Booking System/
├── Controllers/
│   ├── AccountController.cs    # User authentication & management
│   └── HomeController.cs       # Home page
├── Data/
│   └── AppDbContext.cs        # EF Core database context
├── Models/
│   ├── ApplicationUser.cs    # Custom user model
│   └── ApplicationRole.cs   # Custom role model
├── Services/
│   ├── IUserService.cs       # User service interface
│   ├── UserService.cs        # User service implementation
│   ├── IEmailService.cs      # Email service interface
│   └── EmailService.cs      # Email service implementation
├── Settings/
│   ├── SmtpSettings.cs      # SMTP configuration
│   └── EmailSettings.cs   # Email settings
├── ViewModels/
│   └── UserViewModels.cs    # MVC view models
├── Middlewares/
│   └── GlobalExceptionHandling.cs
├── Templates/
│   └── Email/            # Email HTML templates
│       ├── ConfirmEmail.cshtml
│       └── PasswordReset.cshtml
└── Views/
    ├── Account/           # Account controller views
    ├── Home/             # Home controller views
    └── Shared/           # Shared layouts
```

## Getting Started

### Prerequisites

- .NET 10.0 SDK
- SQL Server (local or remote)
- Gmail account (for SMTP)
- Google Cloud Console project (for OAuth)

### Installation

1. Clone the repository
2. Update connection string in `appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER;Database=OnlineBooking;Trusted_Connection=True;MultipleActiveResultSets=true"
}
```

3. Configure SMTP in `appsettings.json`:
```json
"Smtp": {
  "Host": "smtp.gmail.com",
  "Port": 587,
  "UseSsl": true,
  "Username": "your-email@gmail.com",
  "Password": "YOUR_APP_PASSWORD",
  "FromEmail": "your-email@gmail.com",
  "FromName": "Online Booking System"
}
```

4. Configure Google OAuth (optional):
```json
"Authentication": {
  "Google": {
    "ClientId": "YOUR_GOOGLE_CLIENT_ID",
    "ClientSecret": "YOUR_GOOGLE_CLIENT_SECRET"
  }
}
```

5. Apply database migrations:
```bash
dotnet ef database update
```

6. Run the application:
```bash
dotnet run
```

### Gmail App Password Setup

To use Gmail SMTP:

1. Go to Google Account → Security
2. Enable 2-Step Verification
3. Search "App Passwords" → Create new
4. Use the 16-character password in config

### Google OAuth Setup

1. Go to Google Cloud Console
2. APIs & Services → Credentials
3. Create OAuth 2.0 Client ID
4. Add authorized redirect URI: `https://localhost:7001/signin-google`
5. Copy Client ID and Secret to config

## Default Credentials

The application seeds an admin user on first run:

- **Email**: admin@booking.com
- **Password**: Admin123@

## Routes

| Route | Description |
|------|-------------|
| `/Account/Register` | User registration |
| `/Account/Login` | User login |
| `/Account/ConfirmEmail` | Email confirmation (auto-verify) |
| `/Account/ForgotPassword` | Password reset request |
| `/Account/ResetPassword` | Password reset |
| `/Account/Profile` | User profile (authenticated) |
| `/Account/EditProfile` | Edit profile (authenticated) |
| `/Account/ChangePassword` | Change password (authenticated) |
| `/Account/Users` | User management (Admin only) |

## User Roles

| Role | Description |
|------|-------------|
| Admin | Full system access, user management |
| User | Standard user access |
| Guest | Newly registered, unconfirmed users |

## Development

Run in development mode:
```bash
dotnet run
```

In development mode:
- Emails are logged to console instead of sending
- Detailed error pages are shown

## Build

```bash
dotnet build
```

## License

This project is for demonstration purposes.