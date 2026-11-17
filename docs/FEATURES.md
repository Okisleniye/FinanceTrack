# FinanceTrack — Features & Functionality

This document details the functional specifications, workflows, and user interface features implemented in the FinanceTrack web application.

---

## 1. Financial Dashboard (`/`)

The Dashboard provides a comprehensive high-level view of current financial status:

- **Key Metrics Header**:
  - **Month Income**: Total income recorded in the current calendar month with a green `+` prefix.
  - **Month Expenses**: Total expenses recorded in the current calendar month with a red `-` prefix.
  - **Month Net Balance**: Net savings for the current month (`CurrentMonthIncome - CurrentMonthExpenses`).
  - **Total Balance**: Overall all-time net worth (`TotalIncome - TotalExpenses`).
- **6-Month Financial Trends Chart**:
  - Visual dual-bar chart powered by Chart.js comparing monthly income vs. monthly expenses side-by-side for the past 6 months.
- **Spending by Category Breakdown**:
  - Chart.js doughnut chart depicting current month expense distribution by category.
  - Companion category list with color swatches, icons, total amounts in KZT (₸), and exact percentage shares.
- **Recent Transactions Widget**:
  - Table showing the 5 most recent transactions with date, description, category badge, and formatted amount.
  - Quick action buttons to view the full transactions page or add a new transaction.
- **Monthly Budgets Status Widget**:
  - Visual progress bars for active budgets in the current month indicating spent vs. budget amount and alert colors when exceeding limits.

---

## 2. Transactions Management (`/transactions`)

Full-lifecycle income and expense tracking:

- **Transactions Table**:
  - Columns: Date (`dd.MM.yyyy`), Description (with responsive text-break), Category (badge with custom color and Bootstrap icon), Type badge (Income / Expense), Amount (green `+` or red `-` with `₸`), Actions (Edit, Delete).
- **Search & Multi-Filter Toolbar**:
  - **Search Input**: Real-time debounce search matching description or category name.
  - **Type Filter**: Filter by All Types, Expense, or Income.
  - **Category Filter**: Dropdown dynamically showing categories matching the active type filter.
  - **Date Range Filter**: "From Date" and "To Date" pickers with a one-click "Clear dates" button.
  - **Sorting**: Multi-field sorting by `Newest First`, `Oldest First`, `Amount: High to Low`, `Amount: Low to High`.
  - **Reset**: Single-click button restoring all search and filter controls to defaults.
- **Pagination**:
  - Configurable page size (15 items per page).
  - Previous / Next buttons, individual page numbers, and item range counters ("Showing 1 to 15 of 45 transactions").
- **Create Transaction Modal**:
  - Transaction Type toggle (Expense / Income).
  - Category dropdown that automatically updates and filters to only show categories matching the chosen type.
  - Amount input (validates amount > 0).
  - Date picker (defaults to today).
  - Description input (max 200 characters).
  - Dismissible by clicking outside or pressing Cancel.
- **Edit Transaction Modal**:
  - Pre-populates all existing transaction fields with client and server validation.
- **Delete Confirmation Modal**:
  - Displays full transaction summary (description, category, date, amount) before confirming deletion.

---

## 3. Categories Management (`/categories`)

Organization of income and expense classifications:

- **Category Listing**:
  - Displays avatar circle with custom background color and icon.
  - Category Name, unique ID, and Type badge (Expense / Income).
  - Live usage badges: Transaction count and Budget count.
- **Type Filter Tabs**:
  - "All Categories" (with total count badge).
  - "Expenses" (with expense count badge).
  - "Income" (with income count badge).
- **Create Category Modal**:
  - Name input (max 50 characters, enforces uniqueness per type).
  - Type toggle (Expense / Income).
  - Color Picker: Interactive color input with a 12-color preset palette.
  - Icon Picker: Grid of 20 categorized Bootstrap Icons (e.g., `bi-cup-hot`, `bi-car-front`, `bi-cash-stack`, `bi-laptop`, `bi-briefcase`, `bi-bag`, etc.).
- **Edit Category Modal**:
  - Enforces type change restrictions: if the category is used by existing transactions or budgets, type modification is disabled and an explanatory warning is shown.
- **Delete Protection Workflow**:
  - When the user clicks the delete button, the system verifies `CanDeleteAsync()`.
  - If the category has existing transactions or budgets, the modal displays a warning showing the exact number of linked items and disables the delete button.
  - If unreferenced, deletion is permitted.

---

## 4. Monthly Budgets Management (`/budgets`)

Proactive spending limits and allocation tracking:

- **Period Navigation**:
  - Month dropdown (January to December).
  - Year dropdown (current year ± 3 years).
  - Previous Month and Next Month quick navigation buttons.
  - "Today" button resetting to the current calendar month and year.
- **Summary Metrics Banner**:
  - **Total Budgeted**: Sum of all budget allocations for the chosen month.
  - **Total Spent**: Actual expenses recorded in the selected month across budgeted categories.
  - **Remaining Balance**: Total budgeted minus total spent.
  - **Overall Usage Progress Bar**: Percentage of total budget used.
- **Budget Cards Grid**:
  - Category icon, color, and name.
  - Amount spent vs. Budgeted amount in KZT (₸).
  - Remaining amount (or negative exceeded amount).
  - Visual Progress Bar:
    - Green (< 75% utilized).
    - Yellow / Warning (75% to 100% utilized).
    - Red / Danger (> 100% utilized / over-budget).
  - "Over Budget" badge displaying exact excess spending when exceeded.
- **Create & Edit Budget Modal**:
  - **Strict Expense Category Restriction**: Only categories of type `Expense` appear in the category selector. Income categories cannot be budgeted.
  - **Duplicate Prevention**: Prevents creating multiple budgets for the same category in the same month and year.
  - Amount input (strictly positive).
- **Delete Budget Modal**:
  - Confirmation prompt displaying category, month, year, budgeted amount, and actual spending.

---

## 5. Reports & Analytics (`/reports`)

In-depth financial analysis and historical trend evaluation:

- **Period Selection Toolbar**:
  - Preset filters: "This Month", "Last Month", "Last 6 Months", "This Year".
  - Custom Date Range pickers ("From" and "To" dates) with an instant "Apply" button.
- **Summary KPI Cards**:
  - Total Income for the selected period.
  - Total Expenses for the selected period.
  - Net Savings (`TotalIncome - TotalExpenses`).
  - Savings Rate percentage (`NetSavings / TotalIncome * 100`) with a visual gauge bar.
- **Income vs. Expenses Over Time Chart**:
  - Chart.js bar chart comparing monthly income against monthly expenses across all months falling within the selected period.
- **Category Breakdown Visualizations**:
  - **Expense Breakdown**: Chart.js doughnut chart and table detailing category name, icon, total spent, and percentage share.
  - **Income Breakdown**: Chart.js doughnut chart and table detailing income sources, amounts, and percentage shares.
- **Monthly Period Summary Table**:
  - Tabular breakdown listing Month, Income (+₸), Expenses (-₸), and Net Savings (+/-₸).
- **Empty States**:
  - Informative empty state card with advice and a direct shortcut to the Transactions page when no records exist for the chosen period.

---

## 6. Global Formatting & Usability Standards

- **Currency**: Kazakhstani Tenge (KZT, `₸`). Formatted consistently across all cards, tables, modals, and charts with thousands separators and 2 decimal places (e.g., `125,000.00 ₸`).
- **Dates**: Formatted as `dd.MM.yyyy` (e.g., `17.11.2026`) in tables and `MMMM yyyy` in period selectors.
- **Responsive Layout**: Designed for mobile, tablet, and desktop screens with a collapsible hamburger navigation drawer on mobile (`< 768px`).
- **Safety**: Cascade deletes are permanently disabled (`DeleteBehavior.Restrict`).
