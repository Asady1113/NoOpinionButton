using Microsoft.Extensions.DependencyInjection;
using Core.Application;
using Core.Domain;
using Infrastructure.Repository;

namespace DependencyInjection;

public static class DependencyInjectionConfig
{
    public static IServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();

        services.AddTransient<ISignInService, SignInService>();
        services.AddTransient<ISignInRepository, SignInRepository>();

        return services.BuildServiceProvider();
    }
}