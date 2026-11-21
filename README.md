# TaskTracker API

A RESTful task management API built with **C#**, **ASP.NET Core**, and **PostgreSQL**.

TaskTracker allows authenticated users to manage their tasks securely, with pagination, filtering, and user-specific data access.

## Features

- User registration and login
- JWT authentication
- Secure password hashing
- Create, retrieve, complete, and delete tasks
- Pagination and filtering
- User-specific task access
- Login rate limiting
- Database indexing
- Centralized exception handling
- CORS configuration
- Database health check

## Tech Stack

| Technology | Purpose |
|---|---|
| C# | Programming language |
| ASP.NET Core | REST API |
| Entity Framework Core | ORM |
| PostgreSQL | Database |
| JWT | Authentication |
| xUnit | Testing |

## Project Structure

```text
TaskTracker/
├── TaskApi/
│   ├── DTOs/
│   ├── ExceptionHandlers/
│   └── Program.cs
├── TaskTracker.Core/
│   ├── Data/
│   ├── Models/
│   ├── Repositories/
│   └── Services/
├── TaskApi.Tests/
├── TaskManagement.slnx
└── README.md
```

## Getting Started

### Prerequisites

- .NET SDK 11
- PostgreSQL
- Entity Framework Core CLI

Install the EF Core CLI if needed:

```bash
dotnet tool install --global dotnet-ef
```

### 1. Clone the repository

```bash
git clone https://github.com/alikassem877/TaskTracker.git
cd TaskTracker
```

### 2. Configure secrets

Initialize User Secrets:

```bash
dotnet user-secrets init --project ./TaskApi
```

Configure your PostgreSQL connection string:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "YOUR_CONNECTION_STRING" --project ./TaskApi
```

Configure JWT settings:

```bash
dotnet user-secrets set "Jwt:Key" "YOUR_BASE64_KEY" --project ./TaskApi
dotnet user-secrets set "Jwt:Issuer" "TaskTracker" --project ./TaskApi
dotnet user-secrets set "Jwt:Audience" "TaskTracker" --project ./TaskApi
```

The JWT key must decode to at least 32 bytes.

### 3. Create the database and apply migrations

Make sure your PostgreSQL database exists, then run:

```bash
dotnet ef database update --project ./TaskTracker.Core --startup-project ./TaskApi
```

### 4. Run the API

```bash
dotnet run --project ./TaskApi --urls http://localhost:5205
```

API base URL:

`http://localhost:5205`

### 5. Run tests

```bash
dotnet test ./TaskManagement.slnx
```

## API Endpoints

| Method | Endpoint | Description |
|---|---|---|
| POST | `/api/auth/register` | Register a user |
| POST | `/api/auth/login` | Login and receive a JWT |
| GET | `/api/tasks` | Get tasks |
| GET | `/api/tasks/{id}` | Get a task |
| POST | `/api/tasks` | Create a task |
| PUT | `/api/tasks/{id}/complete` | Complete a task |
| DELETE | `/api/tasks/{id}` | Delete a task |
| GET | `/health` | Check API and database health |

Task endpoints require JWT authentication.

## Pagination and Filtering

The tasks endpoint supports:

- `page` — Page number
- `pageSize` — Results per page (maximum 50)
- `isDone` — Filter by completion status
- `priority` — Filter by priority
- `search` — Search task titles

Example:

```http
GET /api/tasks?page=1&pageSize=10&isDone=false
```

## Security

- JWT token validation
- Password hashing
- User-specific data access
- Login rate limiting
- Unique email constraint
- Restricted CORS origin
- Secure configuration using User Secrets

---

**Built with C# and ASP.NET Core.**