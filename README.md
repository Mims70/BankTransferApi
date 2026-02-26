# Bank Transfer API

A secure payment transaction API built with ASP.NET Core, featuring account management, money transfers, and JWT authentication.

## Features

- Account Management (Create, Read, Update, Delete)
- Money Transfers with Database Transactions
- JWT Authentication & Authorization
- Role-Based Access Control
- SQL Server Database
- External API Integration

## Tech Stack

- **Framework:** ASP.NET Core 9.0
- **Database:** SQL Server 2022
- **ORM:** Entity Framework Core
- **Authentication:** JWT Bearer Tokens
- **Password Hashing:** BCrypt

## Setup

1. Install .NET 9.0 SDK
2. Run SQL Server (Docker):
```bash
   docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=YourPassword" -p 1433:1433 -d mcr.microsoft.com/mssql/server:2022-latest
```
3. Update `appsettings.json` with your connection string
4. Run migrations:
```bash
   dotnet ef database update
```
5. Run the API:
```bash
   dotnet run
```

## API Endpoints

### Authentication
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Login and get JWT token

### Accounts
- `GET /api/accounts` - Get all accounts
- `GET /api/accounts/{accountNumber}` - Get specific account
- `POST /api/accounts` - Create account (Admin only)

### Transactions
- `POST /api/transactions/transfer` - Transfer money
- `GET /api/transactions` - Get all transactions
- `GET /api/transactions/{reference}` - Get transaction by reference

## Author

Built as a learning project for CoralPay integration.