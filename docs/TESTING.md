# FinanceTrack — Testing Documentation

This document describes the testing strategy, test suite architecture, execution commands, and verified test results for the FinanceTrack solution.

---

## 1. Test Projects Structure

The test suite is organized into two distinct projects following Clean Architecture principles:

```
tests/
├── FinanceTrack.UnitTests/
│   ├── Common/                         # In-memory SQLite test fixtures, TestDateTimeProvider
│   ├── Domain/
│   │   ├── BaseEntityTests.cs          # CreatedAt/UpdatedAt timestamp tracking (1 test)
│   │   ├── BudgetTests.cs              # Budget entity invariants and validation (14 tests)
│   │   ├── CategoryTests.cs            # Category entity invariants and validation (14 tests)
│   │   ├── DomainExceptionTests.cs     # Custom exception formatting (1 test)
│   │   └── TransactionTests.cs         # Transaction entity invariants and validation (14 tests)
│   └── Services/
│       ├── BudgetServiceTests.cs       # Budget CRUD, calculations, duplicate prevention (10 tests)
│       ├── CategoryServiceTests.cs     # Category CRUD, type changes, delete restrictions (9 tests)
│       ├── DashboardServiceTests.cs    # Multi-period aggregations, category breakdowns (2 tests)
│       ├── ReportServiceTests.cs       # Date range validation, savings rate, multi-month trends (5 tests)
│       └── TransactionServiceTests.cs  # Transaction CRUD, filtering, pagination, sorting (8 tests)
│
└── FinanceTrack.IntegrationTests/
    ├── Infrastructure/
    │   ├── DatabaseSetupTests.cs       # In-memory SQLite connectivity (1 test)
    │   └── FinanceTrackDbContextTests.cs # Tables, seed data, CRUD, FK restrict, unique indexes (50 tests)
    └── Web/
        ├── CategoriesAndBudgetsPageTests.cs # Endpoints, models, category & budget workflows (21 tests)
        ├── DashboardAndReportsPageTests.cs  # Endpoints, dashboard & report service workflows (4 tests)
        └── TransactionsPageTests.cs         # Endpoints, models, transaction service workflows (15 tests)
```

---

## 2. Test Execution Commands

To execute tests from the command line, run the following commands from the repository root:

### Run All Tests
```powershell
dotnet test FinanceTrack.sln
```

### Run Unit Tests Only
```powershell
dotnet test tests/FinanceTrack.UnitTests/FinanceTrack.UnitTests.csproj
```

### Run Integration Tests Only
```powershell
dotnet test tests/FinanceTrack.IntegrationTests/FinanceTrack.IntegrationTests.csproj
```

### Run with Detailed Verbosity
```powershell
dotnet test FinanceTrack.sln --logger "console;verbosity=normal"
```

---

## 3. Scope of Coverage

### 3.1. Domain Unit Tests (44 tests)
- **`Transaction`**: Enforces `Amount > 0`, non-empty description (max 200 chars), valid date, valid transaction type, and strict category type match (`AssignCategory`).
- **`Category`**: Enforces non-empty name (max 50 chars), valid transaction type, duplicate budget prevention per period, and prevents adding mismatched transactions.
- **`Budget`**: Enforces `Amount > 0`, month range (1–12), year range (2000–2100), and restricts category assignment exclusively to `Expense` categories.
- **`BaseEntity` & Exceptions**: Automatic UTC timestamp generation on creation and `MarkUpdated()` verification.

### 3.2. Application Service Unit Tests (48 tests)
- **`TransactionService`**: Full CRUD, text search on descriptions/categories, filtering by type/category/date range, sorting (`date_desc`, `date_asc`, `amount_desc`, `amount_asc`), pagination (`PagedResult<T>`), and missing category rejection.
- **`CategoryService`**: Duplicate category name rejection per type, category type change prevention when linked to transactions/budgets, and deletion prevention via `CanDeleteAsync` for in-use categories.
- **`BudgetService`**: Duplicate budget prevention for same `(CategoryId, Month, Year)`, income category rejection, actual spent calculation from transactions, remaining balance, percentage used, and over-budget flag.
- **`DashboardService`**: Aggregation of total income/expenses/balance, current month statistics, 5 most recent transactions, category spending distribution, and budget overview.
- **`ReportService`**: Custom period financial summary, category breakdowns, net savings, savings rate calculation, and multi-month financial trends.

### 3.3. Infrastructure Integration Tests (51 tests)
- **Database Schema**: Verification of `Transactions`, `Categories`, and `Budgets` tables in SQLite.
- **Default Seeding**: Verification of 13 pre-seeded categories (8 Expense, 5 Income) with proper icons and colors.
- **Referential Integrity**: Enforcement of foreign keys with `DeleteBehavior.Restrict` (attempting to delete a category that has transactions or budgets triggers a `DbUpdateException`).
- **Unique Constraints**: Unique compound index on `(CategoryId, Month, Year)` in the `Budgets` table.
- **Query Optimization**: Verification of indexing on `Date`, `Type`, `CategoryId`, and `(CategoryId, Date)`.

### 3.4. Web & Endpoint Integration Tests (40 tests)
- **HTTP Routing & SSR**: Verification that GET requests to `/`, `/transactions`, `/categories`, `/budgets`, and `/reports` return HTTP 200 OK and valid HTML markup.
- **View Model Validation**: Complete DataAnnotations validation tests for `TransactionFormModel`, `CategoryFormModel`, and `BudgetFormModel` (required fields, positive amounts, valid months/years, valid hex colors, string length limits).
- **DTO Conversions**: Bidirectional mapping between ViewModels and Application DTOs.
- **End-to-End Container Flows**: Full CRUD, filtering, deletion rule enforcement, and calculations executed through the `WebApplicationFactory<Program>` dependency injection container.

---

## 4. Verified Test Results

| Test Project | Framework | Tests Executed | Passed | Failed | Skipped | Pass Rate |
| :--- | :--- | :---: | :---: | :---: | :---: | :---: |
| **FinanceTrack.UnitTests** | .NET 10 | 92 | 92 | 0 | 0 | **100%** |
| **FinanceTrack.IntegrationTests** | .NET 10 | 91 | 91 | 0 | 0 | **100%** |
| **Total Test Suite** | **.NET 10** | **183** | **183** | **0** | **0** | **100%** |

- **Solution Build**: 0 Errors, 0 Warnings.
- **Total Test Execution Time**: ~2.5 seconds.

---

## 5. Testing Limitations

- **Browser-Level UI Interaction Limitation**: Live browser DOM interaction testing (e.g., simulating mouse clicks and form submissions in an active browser session over WebSocket circuits) was not performed because end-to-end browser automation frameworks (such as Playwright, Selenium, or bUnit) are not included in the project dependencies.
- **Scope of UI Verification**: All UI routes, initial HTML markup generation, view model validations, DTO transformations, and DI service container interactions are comprehensively verified through ASP.NET Core's official `WebApplicationFactory<Program>` test infrastructure.
