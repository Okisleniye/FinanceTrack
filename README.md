# FinanceTrack — Personal Finance Management

FinanceTrack is a modern personal finance management web application designed and built with **C#**, **.NET 10**, **ASP.NET Core Blazor Web App (Interactive Server)**, **Entity Framework Core**, **SQLite**, and **Bootstrap 5**.

The application helps users record transactions, categorize income and expenses, set and monitor monthly budgets with visual spending meters, and analyze financial health through dynamic dashboards and reports.

---

## 🌟 Key Features

- **Financial Dashboard**: Real-time summary of current-month income, expenses, and balance, all-time net position, recent transactions, category spending distribution (Chart.js doughnut chart), and 6-month financial trends.
- **Transactions Management**: Full CRUD operations for income and expenses with descriptions, dates, category tags, amounts in KZT (₸), real-time search, multi-field filtering (type, category, date range), sorting, and responsive pagination.
- **Categories Management**: Organize transactions by custom or default categories with colors and icons. Enforces strict deletion protection preventing removal of categories currently linked to transactions or budgets.
- **Monthly Budgets**: Set spending limits per expense category by month and year. Tracks real-time actual spending, remaining funds, percentage utilized, over-budget indicators, and prevents duplicate budgets for the same category and period.
- **Reports & Analytics**: Comprehensive historical analysis with period presets ("This Month", "Last Month", "Last 6 Months", "This Year") and custom date pickers. Includes income vs. expense comparison charts, category breakdown tables, and net savings rate metrics.
- **Data Integrity & Consistency**: Enforces Clean Architecture principles, non-cascading delete constraints (`DeleteBehavior.Restrict`), locale-invariant monetary calculations, and robust domain validation.

---

## 🛠️ Technology Stack

- **Platform / Framework**: .NET 10 (`net10.0`, SDK 10.0.401)
- **Web UI**: ASP.NET Core Blazor Web App (Interactive Server mode)
- **Database & ORM**: SQLite with Entity Framework Core 10.0.12
- **Styling & UI Components**: Bootstrap 5.3.3, Bootstrap Icons 1.11.3
- **Data Visualization**: Chart.js (via lightweight JavaScript interop)
- **Architecture Pattern**: Simplified Clean Architecture
- **Testing**: xUnit 2.9.2, Microsoft.NET.Test.Sdk, Microsoft.AspNetCore.Mvc.Testing
- **Containerization & Deployment**: Docker (Multi-stage build, .NET 10 SDK & ASP.NET Core Runtime), Render Web Services

---

## 🏛️ Architecture Overview

The solution adheres strictly to Clean Architecture dependency rules:

```
FinanceTrack/
├── FinanceTrack.sln
├── src/
│   ├── FinanceTrack.Domain/           # Enterprise entities (Transaction, Category, Budget), Enums, Invariants
│   ├── FinanceTrack.Application/      # DTOs, interfaces, service implementations, pagination models
│   ├── FinanceTrack.Infrastructure/   # EF Core DbContext, entity configurations, SQLite migrations, seeding
│   └── FinanceTrack.Web/              # Blazor Interactive Server UI, layouts, pages, models, JS interop
├── tests/
│   ├── FinanceTrack.UnitTests/        # 92 unit tests covering Domain invariants and Application services
│   └── FinanceTrack.IntegrationTests/ # 91 integration tests covering DbContext, migrations, and Web endpoints
└── docs/
    ├── ARCHITECTURE.md                # In-depth architectural documentation and data flow
    ├── FEATURES.md                    # Detailed feature specifications and workflows
    ├── TESTING.md                     # Test suite structure, execution, and verification report
    ├── DEFENSE.md                     # University defense presentation guide with Q&A
    └── DEVELOPMENT_PROGRESS.md        # Comprehensive 10-phase development audit log
```

- **Domain** has zero external package dependencies.
- **Application** depends solely on Domain and EF Core query abstractions (`DbSet<T>`).
- **Infrastructure** implements persistence interfaces defined in Application.
- **Web** depends on Application and Infrastructure, registering dependencies through Clean Architecture service extensions.

---

## 📋 Prerequisites

- **.NET 10 SDK** (Version `10.0.401` or later). Verify using:
  ```powershell
  dotnet --version
  ```
- Any modern web browser (Chrome, Edge, Firefox, Safari).
- Visual Studio 2026, Visual Studio Code (with C# Dev Kit), or JetBrains Rider (optional).

---

## 🚀 Getting Started

### 1. Clone the Repository
```powershell
git clone <repository-url>
cd FinanceTrack
```

### 2. Restore Dependencies
```powershell
dotnet restore FinanceTrack.sln
```

### 3. Build the Solution
```powershell
dotnet build FinanceTrack.sln --no-restore
```

### 4. Run the Test Suite
Verify that all 183 automated unit and integration tests pass:
```powershell
dotnet test FinanceTrack.sln --no-build
```

### 5. Launch the Application
```powershell
dotnet run --project src/FinanceTrack.Web/FinanceTrack.Web.csproj
```

Upon startup, the console displays the active local listening URL, typically:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000 (or https://localhost:5001 / dynamically assigned Kestrel port)
```
Open the reported URL in your web browser to access the FinanceTrack Dashboard.

---

## 🐳 Docker and Cloud Deployment (Render)

The project includes a multi-stage **Dockerfile** and **render.yaml** optimized for deploying to [Render](https://render.com) or any container platform:

```bash
# Build Docker image
docker build -t financetrack:latest .

# Run container (listening on port 10000)
docker run -d -p 5000:10000 -v financetrack-data:/app/data financetrack:latest
```

For complete step-by-step instructions on deploying to Render (Free or Persistent Disk tier), see [docs/DEPLOYMENT_RENDER.md](docs/DEPLOYMENT_RENDER.md).

---

## 🗄️ Database and Migrations

- **Database Engine**: SQLite.
- **Database File**: `FinanceTrack.db` (created automatically in the project execution directory or configured connection path).
- **Automatic Migration**: `src/FinanceTrack.Web/Program.cs` automatically invokes `Database.Migrate()` on startup. There is no need to manually run `dotnet ef database update`.
- **Default Seed Data**: On first launch, 13 default categories (8 Expense and 5 Income) are automatically seeded:
  - **Expense**: Food, Transport, Shopping, Housing, Entertainment, Health, Education, Other.
  - **Income**: Salary, Freelance, Business, Investments, Other Income.
- **Data Protection**: All foreign key relationships are configured with `DeleteBehavior.Restrict` to prevent accidental cascading deletion of financial history.

---

## 📖 Additional Documentation

For detailed technical and defense preparation materials, refer to the `docs/` folder:
- [ARCHITECTURE.md](docs/ARCHITECTURE.md) — Layered architectural design, component responsibilities, and data flow.
- [FEATURES.md](docs/FEATURES.md) — Detailed feature descriptions, validation rules, and user interaction design.
- [TESTING.md](docs/TESTING.md) — Testing methodology, test case distribution, and verification commands.
- [DEFENSE.md](docs/DEFENSE.md) — Student presentation guide, key architectural decisions, and 15 defense Q&A.
- [DEPLOYMENT_RENDER.md](docs/DEPLOYMENT_RENDER.md) — Comprehensive Render cloud deployment guide with Docker and SQLite.
- [DEVELOPMENT_PROGRESS.md](docs/DEVELOPMENT_PROGRESS.md) — Complete 10-phase verification logs.
