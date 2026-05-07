using FluentValidation;
using FinCore.Application.DataGateways;
using FinCore.Application.Services;
using FinCore.DB.Accounts;
using FinCore.DB.Base;
using FinCore.Infrastructure.Identity;
using FinCore.SharedKernel.PipelineBehaviors;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace FinCore.Infrastructure.DependencyInjection;

public static class DefaultInfrastructureModule
{
    /// <summary>
    /// Registers all application and infrastructure services.
    /// Call this from Program.cs.
    /// </summary>
    public static IServiceCollection AddFinCoreInfrastructure(this IServiceCollection services)
    {
        // MediatR — scan Application assembly for handlers + validators
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(Application.Accounts.Queries.ListAccounts.ListAccountsQuery).Assembly);
        });

        // FluentValidation — scan Application assembly
        services.AddValidatorsFromAssembly(
            typeof(Application.Accounts.Queries.ListAccounts.ListAccountsQueryValidator).Assembly);

        // MediatR validation pipeline behavior
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        // HTTP context accessor (needed by CurrentUserContext)
        services.AddHttpContextAccessor();

        // Identity / user context
        services.AddScoped<ICurrentUserContext, CurrentUserContext>();

        // DB2 connection factory
        services.AddSingleton<IDbConnectionFactory, Db2ConnectionFactory>();

        // Vendor field configuration stub
        services.AddSingleton<IVendorFieldConfiguration, DefaultVendorFieldConfiguration>();

        // Data providers
        services.AddScoped<IAccountDataProvider, AccountDataProvider>();
        services.AddScoped<IAliasDataProvider, AliasDataProvider>();

        return services;
    }
}
