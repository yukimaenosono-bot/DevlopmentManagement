using Microsoft.Extensions.DependencyInjection;

namespace Synapse.Application;

/// <summary>
/// Application層のDI登録。MediatRハンドラを自動スキャンして登録する。
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        return services;
    }
}
