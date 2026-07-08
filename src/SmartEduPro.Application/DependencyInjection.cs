using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SmartEduPro.Application.Behaviours;

namespace SmartEduPro.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        services.AddValidatorsFromAssembly(assembly);
        services.AddTransient(typeof(MediatR.IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));

        return services;
    }
}
