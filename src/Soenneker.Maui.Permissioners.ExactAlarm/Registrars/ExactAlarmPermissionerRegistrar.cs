using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Soenneker.Maui.Permissioners.ExactAlarm.Abstract;

namespace Soenneker.Maui.Permissioners.ExactAlarm.Registrars;

/// <summary>Registers the ExactAlarmPermissioner service.</summary>
public static class ExactAlarmPermissionerRegistrar
{
    /// <summary>Registers the permissioner with singleton lifetime.</summary>
    public static IServiceCollection AddExactAlarmPermissionerAsSingleton(this IServiceCollection services)
    {
        services.TryAddSingleton<IExactAlarmPermissioner, ExactAlarmPermissioner>();
        return services;
    }

    /// <summary>Registers the permissioner with scoped lifetime.</summary>
    public static IServiceCollection AddExactAlarmPermissionerAsScoped(this IServiceCollection services)
    {
        services.TryAddScoped<IExactAlarmPermissioner, ExactAlarmPermissioner>();
        return services;
    }
}

