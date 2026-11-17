# FinanceTrack — Architectural Documentation

## 1. Architectural Style: Clean Architecture

FinanceTrack is built using a **Simplified Clean Architecture** (also known as Onion or Hexagonal Architecture). The core tenet is the **Dependency Inversion Principle**: high-level business rules do not depend on low-level implementation details (such as UI frameworks or database drivers); instead, both depend on abstractions.

```
+-------------------------------------------------------+
|                      Presentation                     |
|                 (FinanceTrack.Web)                    |
|       Blazor Interactive Server, Razor Pages, JS      |
+---------------------------+---------------------------+
                            |
                            v
+-------------------------------------------------------+
|                      Application                      |
|             (FinanceTrack.Application)                |
|       DTOs, Interfaces, Service Logic, Use Cases      |
+---------------------------+---------------------------+
                            |
                            v
+-------------------------------------------------------+
|                        Domain                         |
|                (FinanceTrack.Domain)                  |
|    Entities, Value Invariants, Enums, Exceptions      |
+-------------------------------------------------------+
                            ^
                            |
+---------------------------+---------------------------+
|                     Infrastructure                    |
|            (FinanceTrack.Infrastructure)              |
|        EF Core, SQLite DbContext, Migrations          |
+-------------------------------------------------------+
```

---

## 2. Layer Responsibilities & Dependencies

### 2.1. Domain Layer (`FinanceTrack.Domain`)
- **Dependencies**: None (0 external NuGet packages or project dependencies).
- **Responsibilities**:
  - Encapsulates enterprise business entities: `Transaction`, `Category`, `Budget`.
  - Defines the core business enumeration: `TransactionType` (`Income = 1`, `Expense = 2`).
  - Enforces entity invariants through encapsulated constructors, setters, and mutation methods (`AssignCategory`, `Update`, `AddTransaction`, `AddBudget`, `HasBudgetFor`, `HasConflictWith`).
  - Defines domain exceptions (`DomainException`, `DomainValidationException`).
  - Base abstractions: `BaseEntity` (Id, CreatedAt, UpdatedAt, `MarkUpdated()`) and `IAggregateRoot`.

### 2.2. Application Layer (`FinanceTrack.Application`)
- **Dependencies**: Depends strictly on `FinanceTrack.Domain` and `Microsoft.EntityFrameworkCore` (for `DbSet<T>` and asynchronous LINQ query abstractions).
- **Responsibilities**:
  - Implements orchestrating application services: `TransactionService`, `CategoryService`, `BudgetService`, `DashboardService`, `ReportService`.
  - Defines application interfaces in `Common/Interfaces`: `ITransactionService`, `ICategoryService`, `IBudgetService`, `IDashboardService`, `IReportService`, `IApplicationDbContext`, `IDateTimeProvider`.
  - Defines strongly typed Data Transfer Objects (DTOs) in `DTOs`: Transaction DTOs, Category DTOs, Budget DTOs, Dashboard DTOs, Report DTOs.
  - Implements pagination models: `PagedResult<T>`.
  - Defines application exceptions: `NotFoundException`, `ApplicationValidationException`.
  - Service registration: `AddApplicationServices()` registering all application services as Scoped.

### 2.3. Infrastructure Layer (`FinanceTrack.Infrastructure`)
- **Dependencies**: Depends on `FinanceTrack.Application` (and transitively `FinanceTrack.Domain`), `Microsoft.EntityFrameworkCore.Sqlite`, and `Microsoft.EntityFrameworkCore.Design`.
- **Responsibilities**:
  - Implements `IApplicationDbContext` via `FinanceTrackDbContext` (and legacy-compatible `ApplicationDbContext`).
  - Fluent API Entity Type Configurations in `Data/Configurations/`:
    - `CategoryConfiguration`: Table name, property constraints, backing field access mode, restrict deletion behavior, and default seed data (13 categories).
    - `TransactionConfiguration`: Table name, decimal precision `(18, 2)`, max length, query indexes (`Date`, `Type`, `CategoryId`, compound index `CategoryId, Date`), foreign key to Category with `DeleteBehavior.Restrict`.
    - `BudgetConfiguration`: Table name, decimal precision `(18, 2)`, compound unique index on `(CategoryId, Month, Year)`, period index `(Year, Month)`, foreign key to Category with `DeleteBehavior.Restrict`.
  - EF Core database migrations: `20261117095658_InitialCreate`.
  - Concrete infrastructure services: `DateTimeProvider` implementing `IDateTimeProvider`.
  - Service registration: `AddInfrastructureServices()` configuring SQLite, database directory creation, and DbContext scopes.

### 2.4. Presentation Layer (`FinanceTrack.Web`)
- **Dependencies**: Depends on `FinanceTrack.Application` and `FinanceTrack.Infrastructure`.
- **Responsibilities**:
  - Hosts the ASP.NET Core Blazor Web App with `@rendermode InteractiveServer`.
  - Presentation components and pages:
    - `Home.razor` (`/`): Real-time financial dashboard and charts.
    - `Transactions.razor` (`/transactions`): Filterable, searchable, paged transaction management with create/edit/delete modals.
    - `Categories.razor` (`/categories`): Category management with usage inspection and deletion protection.
    - `Budgets.razor` (`/budgets`): Period-based budget management with progress meters and expense category enforcement.
    - `Reports.razor` (`/reports`): Historical reporting with presets and custom date range analysis.
  - View Models: `TransactionFormModel`, `CategoryFormModel`, `BudgetFormModel` with DataAnnotations validation.
  - JavaScript Interop: `wwwroot/charts.js` for Chart.js doughnut and bar chart rendering.
  - Responsive layout: `MainLayout.razor` (with mobile hamburger drawer and header) and `NavMenu.razor`.

---

## 3. Domain Entity Model & Relationships

```
+-------------------------------------------------------+
|                       Category                        |
+-------------------------------------------------------+
| PK Id: int                                            |
|    Name: string (max 50)                              |
|    Color: string (hex)                                |
|    Icon: string (bi-*)                                |
|    Type: TransactionType (Income/Expense)             |
|    CreatedAt: DateTime                                |
|    UpdatedAt: DateTime?                               |
+---------------------------+---------------------------+
             | 1            | 1
             |              |
             | 0..*         | 0..*
             v              v
+--------------------------+  +--------------------------+
|       Transaction        |  |          Budget          |
+--------------------------+  +--------------------------+
| PK Id: int               |  | PK Id: int               |
|    Amount: decimal(18,2) |  |    Amount: decimal(18,2) |
|    Description: string   |  |    Month: int (1..12)    |
|    Date: DateTime        |  |    Year: int (2000..2100)|
|    Type: TransactionType |  | FK CategoryId: int       |
| FK CategoryId: int       |  +--------------------------+
+--------------------------+  * Unique (CategoryId,Month,Year)
```

### Relationship Constraints
1. **Category -> Transactions (One-to-Many)**:
   - Foreign key: `Transaction.CategoryId -> Category.Id`.
   - Delete Behavior: `DeleteBehavior.Restrict` (a category cannot be deleted if referenced by any transaction).
2. **Category -> Budgets (One-to-Many)**:
   - Foreign key: `Budget.CategoryId -> Category.Id`.
   - Delete Behavior: `DeleteBehavior.Restrict` (a category cannot be deleted if referenced by any budget).
   - Category constraint: Budgets can **only** be associated with categories where `Type == TransactionType.Expense`.
   - Uniqueness: A composite unique constraint ensures `(CategoryId, Month, Year)` is unique across the entire database.

---

## 4. End-to-End Data Flow

```
[ User Interaction ]
         |
         v
[ Blazor Component (e.g. Transactions.razor) ]
         | (Binds input to TransactionFormModel & runs DataAnnotations validation)
         v
[ Application Service Interface (ITransactionService) ]
         | (Converts FormModel to CreateTransactionDto / UpdateTransactionDto)
         v
[ Service Implementation (TransactionService) ]
         | (Validates business invariants: category existence, category type compatibility)
         v
[ Domain Entity (Transaction) ]
         | (Encapsulates business rules: amount > 0, description valid, date valid)
         v
[ Application DbContext (IApplicationDbContext / FinanceTrackDbContext) ]
         | (Executes LINQ query / entity tracking via EF Core)
         v
[ SQLite Database (FinanceTrack.db) ]
         | (Enforces unique indexes, foreign keys, and decimal precision)
         v
[ Response & ViewModel Mapping (TransactionDto) ]
         |
         v
[ Blazor UI Re-render over SignalR Circuit ]
```

---

## 5. Core Business Rules & Invariants

1. **Financial Values**:
   - Stored and calculated exclusively using C# `decimal` (SQL precision `18, 2`). Floats and doubles are never used for currency.
   - All monetary amounts must be strictly greater than zero (`> 0`).
2. **Type Compatibility Rule**:
   - A Transaction cannot reference a Category of a mismatched `TransactionType`. An Expense transaction cannot be assigned to an Income category.
3. **Budget Period Invariant**:
   - Budgets are strictly restricted to `Expense` categories.
   - A Category can have at most one Budget per calendar month and year.
4. **Referential Integrity & Safe Deletion**:
   - Cascade delete is disabled for all financial records (`DeleteBehavior.Restrict`).
   - `CategoryService.CanDeleteAsync(id)` verifies no transactions or budgets reference the category before deletion is allowed.
5. **Locale Independence**:
   - Invariant culture (`CultureInfo.InvariantCulture`) is explicitly enforced on the application thread and DbContext to prevent SQLite decimal point interpretation errors on European/Russian locales where commas are used as decimal separators.
