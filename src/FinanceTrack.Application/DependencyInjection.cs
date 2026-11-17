using FinanceTrack.Application.Common.Interfaces;
using FinanceTrack.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FinanceTrack.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<ITransactionService, TransactionService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IBudgetService, BudgetService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IReportService, ReportService>();

        return services;
    }
}
