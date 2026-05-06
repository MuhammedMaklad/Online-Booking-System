# Online Booking System

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