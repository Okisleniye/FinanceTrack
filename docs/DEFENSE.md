# FinanceTrack — Project Defense Guide & Presentation Notes

This document provides structured talking points, technical rationales, demonstration workflows, and anticipated questions and answers for defending the **FinanceTrack** university final project.

---

## 1. Project Description & Goal

### Short Description
**FinanceTrack** is a modern, single-user personal finance management web application built with **.NET 10**, **ASP.NET Core Blazor Web App (Interactive Server)**, **Entity Framework Core**, **SQLite**, and **Bootstrap 5**.

### Problem Solved
Managing personal finances across disparate spreadsheets or manual notes often leads to untracked spending, budget overruns, and an inability to understand historical spending patterns. FinanceTrack solves this by providing an intuitive, centralized system to record income and expenses, enforce monthly category budgets, and analyze spending habits through real-time charts and reports.

### Primary Objectives
1. Implement a clean, maintainable software architecture (**Clean Architecture**) separating enterprise rules, use cases, persistence, and UI.
2. Provide full transaction, category, and budget management with robust domain validation.
3. Deliver visual analytics (category breakdown charts, 6-month trends, budget utilization progress bars) using **Chart.js**.
4. Guarantee financial integrity through non-cascading delete constraints (`DeleteBehavior.Restrict`) and invariant decimal calculations in Kazakhstani Tenge (₸).
5. Ensure software reliability with a 100% passing automated test suite (183 tests).

---

## 2. Technology Choices & Justifications

| Technology | Reason for Selection |
| :--- | :--- |
| **.NET 10 & C#** | Latest LTS-ready framework delivering high performance, modern C# language features, and native support for Clean Architecture. |
| **Blazor Web App (Interactive Server)** | Enables building a dynamic single-page web UI in pure C# with SignalR real-time state synchronization, eliminating JavaScript UI framework complexity while maintaining high responsiveness. |
| **Entity Framework Core 10** | Industry-standard ORM providing type-safe LINQ queries, automatic migrations, Fluent API constraints, and database seeding. |
| **SQLite** | Zero-configuration, serverless, self-contained relational database ideal for desktop/local deployments and university evaluation without external database server setup. |
| **Bootstrap 5 & Bootstrap Icons** | Responsive, accessible CSS framework providing mobile-friendly layouts, modals, badges, and progress bars without third-party component libraries. |
| **Chart.js** | Lightweight, high-performance HTML5 canvas charting library integrated cleanly via minimal JavaScript interop for doughnut and bar charts. |
| **xUnit** | Modern, extensible unit and integration testing framework fully supported by the .NET CLI. |

---

## 3. Architecture & Key Technical Decisions

### 3.1. Clean Architecture Separation
- **`Domain`**: Pure enterprise rules with zero external dependencies. Contains entities (`Transaction`, `Category`, `Budget`), enums (`TransactionType`), and invariants.
- **`Application`**: Contains service interfaces, DTOs, use cases, and pagination models. Protects Domain entities from being directly exposed to the UI.
- **`Infrastructure`**: Implements EF Core persistence, database migrations, seed data, and system services (e.g. `DateTimeProvider`).
- **`Web`**: Blazor presentation layer containing Razor components, view models, and JavaScript interop for charts.

### 3.2. Critical Design Decisions
1. **Financial Precision (`decimal`)**: All financial numbers use the C# `decimal` type with database precision `(18, 2)`. Binary floating-point types (`double`/`float`) are strictly forbidden to prevent floating-point rounding errors.
2. **Referential Integrity & No Cascade Delete**: Foreign keys are configured with `DeleteBehavior.Restrict`. Deleting a Category that contains transactions or budgets is prevented at the database and application levels.
3. **Culture Invariance**: `CultureInfo.InvariantCulture` is enforced application-wide at startup and in tests. This prevents SQLite from throwing decimal parsing format exceptions on systems using comma decimal separators.
4. **Startup Migration & Seeding**: `Program.cs` automatically invokes `Database.Migrate()` on launch, creating the database and seeding 13 default categories with zero manual setup.
5. **Budget Uniqueness**: A composite unique index on `(CategoryId, Month, Year)` guarantees that a category cannot have duplicate budgets in the same calendar month.

---

## 4. Live Demonstration Plan

1. **Dashboard (`/`)**:
   - Show the 4 summary KPI cards (Month Income, Month Expenses, Month Balance, Total Balance).
   - Point out the 6-month trends dual-bar chart and the current month Category Spending doughnut chart.
   - Show the Recent Transactions table and current month Budget progress meters.
2. **Transactions (`/transactions`)**:
   - Add an Expense transaction (e.g., Groceries for 5,000 ₸). Show that Category dropdown only shows Expense categories.
   - Switch type to Income (e.g., Freelance for 50,000 ₸); show that Category dropdown dynamically updates to Income categories.
   - Demonstrate text search, filter by category, and sorting options.
   - Demonstrate the Delete Confirmation modal.
3. **Categories (`/categories`)**:
   - View category usage counters (transactions and budgets).
   - Create a new custom category with color picker and icon grid.
   - Attempt to delete a category that has linked transactions; show that the system detects usage and prevents deletion.
4. **Budgets (`/budgets`)**:
   - Navigate across months (Next/Previous month buttons).
   - Set a budget for an Expense category (e.g., Food). Show that Income categories are excluded.
   - Demonstrate progress bar color changes (Green under 75%, Yellow 75–100%, Red over 100%).
   - Attempt to create a duplicate budget for the same category and month; show the validation rejection.
5. **Reports & Analytics (`/reports`)**:
   - Select presets ("This Month", "Last 6 Months", "This Year") or custom date range.
   - Review Net Savings and Savings Rate percentage.
   - View Expense Breakdown and Income Breakdown charts and tables.

---

## 5. Verified Testing Results

- **Unit Tests**: 92 passed (44 Domain invariants + 48 Application services).
- **Integration Tests**: 91 passed (51 Infrastructure/EF Core + 40 Web endpoints and workflows).
- **Total Tests**: **183 passed**, **0 failed**, **0 skipped** (**100% pass rate**).
- **Compilation**: 0 Errors, 0 Warnings.

---

## 6. Anticipated Defense Questions & Answers

### Q1: Why did you choose Clean Architecture instead of a standard 3-tier architecture?
> **Answer**: Clean Architecture decouples the core business logic from external frameworks, database drivers, and UI libraries. In FinanceTrack, the `Domain` layer has zero dependencies, meaning business rules (such as budget constraints and category compatibility) can be tested thoroughly with unit tests without needing a database connection or web server.

### Q2: Why did you use Blazor Server rather than Blazor WebAssembly or a JavaScript framework like React?
> **Answer**: Blazor Server allows writing the entire full-stack application in C#, sharing DTOs and validation rules directly without writing redundant JavaScript APIs or models. SignalR maintains real-time UI state on the server, which is fast and lightweight for a personal finance system without requiring client-side API serialization.

### Q3: How do you prevent financial rounding errors in the application?
> **Answer**: We use the 128-bit `decimal` data type throughout the Domain, Application, and UI layers, configured in EF Core with precision `(18, 2)`. Unlike `double` or `float`, which use binary base-2 representation causing imprecise decimal fractions (like `0.1 + 0.2 = 0.30000000000000004`), `decimal` uses base-10 representation, guaranteeing exact financial calculations.

### Q4: What happens if a user tries to delete a category that already has transactions?
> **Answer**: The application strictly forbids this. First, the UI checks `CanDeleteAsync()`, which queries whether any transactions or budgets reference that category, displaying a warning and disabling the delete action. Second, even if a request bypassed the UI, EF Core's foreign key relationship is configured with `DeleteBehavior.Restrict`, causing the SQLite database to reject the deletion with a constraint exception.

### Q5: How do you prevent duplicate budgets for the same category in the same month?
> **Answer**: This is enforced at three levels:
> 1. **Domain**: `Category.HasBudgetFor(month, year)` and `Budget.HasConflictWith()`.
> 2. **Application**: `BudgetService.CreateAsync` checks `AnyAsync()` before saving and throws an `ApplicationValidationException`.
> 3. **Database**: A composite unique index on `(CategoryId, Month, Year)` is enforced directly in the SQLite schema by EF Core.

### Q6: Why can budgets only be created for expense categories?
> **Answer**: By definition, a budget is an expenditure ceiling designed to control spending. Setting a budget on an Income category (like Salary) is semantically incorrect and would distort spending calculations and progress bars. The Domain and UI explicitly enforce that budgets can only be assigned to categories with `TransactionType.Expense`.

### Q7: Why did you add `Database.Migrate()` in `Program.cs`?
> **Answer**: For a local, standalone application, requiring manual command-line migrations (`dotnet ef database update`) creates friction for new users or instructors evaluating the project. Placing `Database.Migrate()` in `Program.cs` automatically ensures the SQLite file is created, schema migrations are executed, and default categories are seeded upon startup, without risking data loss on subsequent runs.

### Q8: How did you solve locale-dependent decimal issues with SQLite?
> **Answer**: On operating systems set to regional locales like Russian or French, numbers use commas as decimal separators (e.g., `1000,50`). SQLite natively stores decimal numbers with dots (`1000.50`), which can cause `FormatException` during SQL sorting and parsing. We resolved this by explicitly configuring `CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture` across `Program.cs` and all test fixtures.

### Q9: How are the charts implemented if you used Blazor Server?
> **Answer**: We integrated **Chart.js** using a minimal JavaScript interop script (`charts.js`). When Blazor completes component rendering (`OnAfterRenderAsync`), it invokes JavaScript functions passing chart data, labels, and colors. In addition, every chart has an accompanying HTML table and progress meter so data remains 100% accessible even if JavaScript is unavailable.

### Q10: What are the main limitations of the current implementation?
> **Answer**: The current application is designed as a single-user system without multi-tenant authentication or multi-currency conversions (it is standardized on KZT). In future iterations, ASP.NET Core Identity could be added for multi-user support, and an external exchange rate API could support multi-currency wallets.

### Q11: How do you ensure that transactions cannot be assigned to an incompatible category type?
> **Answer**: In the Domain layer, `Transaction.Update()` and `Transaction.AssignCategory()` verify that `category.Type == this.Type`, throwing a `DomainValidationException` on mismatch. In the Blazor UI, the category dropdown is dynamically filtered based on the selected `TransactionType` radio button, making it impossible for a user to select an incompatible category.

### Q12: What is the purpose of the `IDateTimeProvider` abstraction?
> **Answer**: Calling `DateTime.UtcNow` directly inside business logic makes unit testing time-dependent calculations (like current month income or 6-month trends) difficult and non-deterministic. By injecting `IDateTimeProvider`, unit tests can provide a mock or fixed time provider (`TestDateTimeProvider`) to simulate any point in time accurately.

---

## 7. Conclusion

FinanceTrack exemplifies modern C# and .NET 10 development practices. By adhering to Clean Architecture, enforcing strict domain validation, maintaining 100% automated test pass rates, and delivering a clean, responsive Blazor UI, the project successfully fulfills all academic and professional engineering requirements.
