# Task Management System API

A robust, enterprise-grade Task Management System built using .NET 10 and Clean Architecture (Onion Architecture) principles.

## Features Included
- **Clean Architecture**: Separation of concerns across Core, Infrastructure, Services, and API layers.
- **Authentication & Security**: JWT Bearer authentication, Role-Based Access Control (Admin/User), and JWT Refresh Token rotation.
- **Standardized Responses**: Every API returns a consistent `ApiResponse<T>` wrapper.
- **AutoMapper**: Automated mapping between domain entities and DTOs.
- **Entity Framework Core**: Code-first migrations with the UnitOfWork and Repository patterns.
- **Redis Caching**: Caching of individual tasks with targeted invalidation on updates.
- **Background Processing**: A hosted `BackgroundService` that processes newly created tasks asynchronously (simulating work by moving tasks from `InProgress` to `Done`).
- **Global Error Handling**: Custom exception middleware returning standardized ProblemDetails.
- **API Versioning & Rate Limiting**: Built-in .NET Fixed Window rate limiting and URL-segment versioning (`/api/v1/`).
- **Logging**: Serilog integration writing to Console, File, and MSSQL Server sinks.

---

## Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- SQL Server (LocalDB or Docker instance)
- Redis Server (Running locally on default port `localhost:6379`)

---

## Setup Instructions

1. **Clone the Repository**
2. **Ensure Redis is running**: 
   If using Docker, you can start a local Redis instance:
   ```bash
   docker run -d -p 6379:6379 --name redis redis
   ```
3. **Database Configuration**:
   The `appsettings.json` is currently configured to connect to your local MS SQL Server instance:
   `Server=DESKTOP-QIQ65J3\\SQLEXPRESS;Database=TaskManagementDB;Trusted_Connection=True;MultipleActiveResultSets=true;Encrypt=False`
   
   *Note: If your SQL Server has a different name, please update the `DefaultConnection` string in `TaskManagementSystem.API/appsettings.json`.*

4. **Apply Migrations**:
   Open a terminal in the root directory and run the Entity Framework Core database update command:
   ```bash
   dotnet ef database update --project TaskManagementSystem.Infrastructure --startup-project TaskManagementSystem.API
   ```

---

## How to Run the Project

You can run the project using the .NET CLI:
```bash
cd TaskManagementSystem.API
dotnet run
```
Once running, navigate to `https://localhost:7054/swagger` in your web browser to view the interactive API documentation and test the endpoints.

---

## Seeded Admin Credentials

Upon the very first startup, the system will automatically create the database (if it doesn't exist) and seed a default Admin user to help you get started.

- **Email**: `admin@example.com`
- **Password**: `Admin@123`

You can use these credentials in the `POST /api/v1/auth/login` endpoint to receive your JWT Access Token and Refresh Token.

---

## Business Logic Assumptions

1. **Duplicate Tasks**: 
   - *Assumption*: A user should not be able to create the exact same task twice in one day by accident. 
   - *Logic*: The `TaskService.CreateTaskAsync` method throws a 409 Conflict if a task with the identical Title is created on the same UTC calendar day by the same user.
2. **Task Sorting**:
   - *Assumption*: Users want to see their most important tasks first.
   - *Logic*: `GetAllTasksAsync` sorts tasks by Priority (High > Medium > Low) and then by Creation Date (Oldest first).
3. **Soft Deletion**:
   - *Assumption*: Administrative deletions should be auditable.
   - *Logic*: Deleting a user via the Admin API does not drop the row. Instead, `IsDeleted = true` is set, and the `DeletedBy` column is populated with the Admin's email.
4. **Rate Limiting**:
   - *Assumption*: The API needs baseline protection from abuse.
   - *Logic*: A global fixed-window rate limiter restricts clients to 100 requests per minute per IP address.
5. **Token Lifespans**:
   - *Assumption*: Access tokens should be short-lived for security, but the user shouldn't be logged out constantly.
   - *Logic*: The JWT expires in 15 minutes, but the Refresh Token is valid for 7 days.
6. **CORS**:
   - *Assumption*: The API will be consumed by various front-end clients during development with no specific origin restrictions.
   - *Logic*: A global `AllowAll` CORS policy is configured, permitting any origin, any HTTP method, and any header. This is applied before authentication in the middleware pipeline.

---

## CORS Configuration

Cross-Origin Resource Sharing (CORS) is enabled globally for all origins.

**Policy Name**: `AllowAll`

| Setting | Value |
|---|---|
| Allowed Origins | Any (`*`) |
| Allowed Methods | Any (GET, POST, PUT, PATCH, DELETE, etc.) |
| Allowed Headers | Any |

> **Note**: The `AllowAll` policy is suitable for development. For production, restrict origins to your actual client domains (e.g., `https://your-frontend.com`).

## New Feature: Task Filtering & Pagination

- Implemented flexible task retrieval using **Specification** pattern.
- Added endpoint `GET /api/v1/tasks/filtered` supporting:
  - Full‑text search on title/description.
  - Filtering by status, priority, and creation date range.
  - Sorting by any field (`title`, `status`, `priority`, `createdAt`) in ascending or descending order.
  - Server‑side pagination with configurable `page` and `pageSize`.
- Updated `TaskService` to map DTO filters to domain filters and return a `PagedResultDto`.
- Added `TasksWithFiltersSpecification` and generic repository methods `GetPagedWithSpecAsync`.
- New DTO `PagedResultDto<T>` and `TaskFilterDto` expose pagination metadata.

This enhances API usability for large task lists and improves performance by retrieving only required data.