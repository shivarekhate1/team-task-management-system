# Team Task Management System

A production-ready, full-stack **Role-Based Task Management System** built with **ASP.NET Core .NET 8/10**, **Entity Framework Core**, **SQL Server**, **JWT Authentication**, and a modern responsive **React (Vite + Axios)** frontend.

---

## 🌟 Features Overview

- **Authentication & Security**:
  - Secure Password Hashing (PBKDF2 SHA-256 with cryptographic salt).
  - JWT Bearer Authentication with custom claims (`NameIdentifier`, `Email`, `Name`, `Role`).
  - Session/token expiration handling & automatic 401 interceptor redirection.

- **Role-Based Access Control (RBAC)**:
  - **Admin**: Full system oversight, team management, user role promotion, cross-team task assignment.
  - **Manager**: Create teams, assign team members, create and assign tasks, track progress.
  - **User**: View assigned tasks & team tasks, update task status (`To Do` -> `In Progress` -> `Done`), collaborate via comments.
  - Fine-grained API-level authorization attributes (`[Authorize(Roles = "...")]`) + business ownership authorization logic.

- **Task Management**:
  - CRUD operations with strict domain validation (non-empty title, valid deadline, team membership verification).
  - Supported task statuses: `To Do`, `In Progress`, `Done`.
  - Supported task priorities: `Low`, `Medium`, `High`, `Urgent`.
  - Interactive Kanban Board View & Filterable Data Table View.

- **Team Management**:
  - Create and manage organizational teams.
  - Assign/reassign Team Managers.
  - Add & remove team members with role permission checks.

- **Task Comments & Notifications**:
  - Threaded comment collaboration per task.
  - Automatic event-driven notification triggers for:
    1. **Task Assignment** ("You have been assigned a new task: {Title}").
    2. **Task Status Update** ("Task '{Title}' status changed to {Status}.").
  - Interactive top navigation notification bell with unread badge counter, popover drawer, and read toggles.

- **Dynamic Analytics Dashboard**:
  - Real-time aggregate metric widgets (Total Tasks, To Do, In Progress, Done, Overdue Alert).
  - Priority distribution bars.
  - Per-user task progress breakdown for Admin & Manager oversight.
  - Dynamic backend API data source (zero hardcoded stats).

---

## 🏗️ Technology Stack

| Layer | Technology |
| :--- | :--- |
| **Backend Framework** | C#, ASP.NET Core Web API (.NET 8/10) |
| **Database & ORM** | SQL Server / SQL Server LocalDB, Entity Framework Core Code-First |
| **Authentication** | JWT (JSON Web Tokens), ASP.NET Core Authorizations |
| **API Style & Docs** | RESTful APIs, Swagger / OpenAPI UI with Bearer Token Authorize button |
| **Frontend Framework** | React 18, Vite, React Router DOM v6, Axios |
| **Styling & UI** | Custom CSS Design System (Glassmorphic dark UI, badges, modals, responsive layout) |
| **Testing** | xUnit, FluentAssertions, EF Core InMemory Database |
| **DevOps / Containers** | Docker multi-stage builds, Docker Compose, GitHub Actions CI Pipeline |

---

## 🔑 Sample Credentials (Development & Demo)

Use these seeded accounts to log in directly from the React interface or Swagger UI:

| Role | Email | Password | Permissions |
| :--- | :--- | :--- | :--- |
| **Admin** | `admin@taskmanagement.com` | `Admin@123` | Full system access, User role management, Team/Task administration |
| **Manager** | `manager@taskmanagement.com` | `Manager@123` | Create/edit tasks, Manage teams & team members |
| **User (Shiva)** | `shiva@taskmanagement.com` | `User@123` | View assigned tasks, update status, add comments |
| **User (Jane)** | `jane@taskmanagement.com` | `User@123` | View assigned tasks, update status, add comments |

---

## 🚀 Quick Setup & Execution

### Prerequisites
- [.NET 8 SDK or .NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js (v18+ or v20+)](https://nodejs.org/)
- SQL Server or SQL Server LocalDB (`(localdb)\mssqllocaldb`)

### 1. Database Setup & Migration
The backend is configured to use **SQL Server LocalDB** out of the box and auto-applies migrations & seed data on startup.

To manually run database updates via EF Core CLI:
```bash
dotnet restore
dotnet ef database update --project Backend/TaskManagement.Infrastructure/TaskManagement.Infrastructure.csproj --startup-project Backend/TaskManagement.API/TaskManagement.API.csproj
```

### 2. Run Backend Web API
```bash
cd Backend/TaskManagement.API
dotnet run
```
- API Base URL: `http://localhost:5198` (or `https://localhost:7198`)
- Swagger OpenAPI Documentation: `http://localhost:5198/swagger`

### 3. Run React Frontend
```bash
cd Frontend/task-management-ui
npm install
npm run dev
```
- App URL: `http://localhost:5173`

---

## 🧪 Running Automated Unit Tests

The test project `TaskManagement.Tests` includes xUnit unit tests verifying Auth logic, Task creation rules, Status transition event notifications, and Role scoping.

Run all tests:
```bash
dotnet test
```

---

## 🐳 Running with Docker Compose

To launch the multi-container environment (SQL Server 2022 + Backend API + Nginx React Frontend):

```bash
docker-compose up --build
```

- **Frontend App**: `http://localhost:8080`
- **Backend API**: `http://localhost:5198/swagger`

---

## 📖 Key REST API Endpoints Summary

| Method | Endpoint | Description | Auth Required |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/auth/register` | Register a new user | Public |
| `POST` | `/api/auth/login` | Authenticate and obtain JWT token | Public |
| `GET` | `/api/auth/me` | Get current user details | `Bearer` |
| `GET` | `/api/users` | List all users | `Bearer` |
| `PUT` | `/api/users/{id}/role` | Update user role | `Admin` |
| `GET` | `/api/teams` | List accessible teams | `Bearer` |
| `POST` | `/api/teams` | Create a new team | `Admin`, `Manager` |
| `POST` | `/api/teams/{id}/members` | Add user to team | `Admin`, `Manager` |
| `GET` | `/api/tasks` | List tasks (supports search & filter params) | `Bearer` |
| `POST` | `/api/tasks` | Create task | `Admin`, `Manager` |
| `PUT` | `/api/tasks/{id}/status` | Update task status (`ToDo`, `InProgress`, `Done`) | `Bearer` |
| `POST` | `/api/comments` | Add comment to a task | `Bearer` |
| `GET` | `/api/notifications` | Get user notifications | `Bearer` |
| `GET` | `/api/dashboard/overview` | Dynamic aggregate metrics for dashboard | `Bearer` |

---

## Checklist of PDF Requirements

- [x] Role-Based System (Admin, Manager, User)
- [x] JWT Authentication & Secure Password Hashing
- [x] Role & Business Level Authorization
- [x] EF Core Code-First Migrations & Database Seeding
- [x] Task Management (CRUD, Status tracking: To Do, In Progress, Done)
- [x] Team Management & Member Allocation
- [x] Threaded Comments Section
- [x] In-App Notifications for Task Assignment & Status Update
- [x] Dynamic Dashboard with Status, Priority, Overdue Metrics
- [x] Clean Architecture Backend (.NET 8/10)
- [x] Modern Responsive React Frontend (Axios, Kanban + Table views)
- [x] Swagger OpenAPI Documentation with Bearer Auth
- [x] Unit Tests Suite (xUnit + FluentAssertions + InMemory DB)
- [x] Multi-container Docker Compose & GitHub Actions CI Workflow
