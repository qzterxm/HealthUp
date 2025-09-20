using Microsoft.AspNetCore.Components.Server.Circuits;

namespace UI.CircuitServicesAccesor
{
    public class ServicesAccessorCuircutHandler (IServiceProvider services, 
        UI.CircuitServicesAccesor.CircuitServicesAccesor servicesAccesor) : CircuitHandler
    {
        public override Func<CircuitInboundActivityContext, Task> CreateInboundActivityHandler(
            Func<CircuitInboundActivityContext, Task> next) =>
            async context =>
            {
                servicesAccesor.Service = services;
                await next(context);
                servicesAccesor.Service = null;
            };

    }

    public static class CircuitServicesServiceCollectionExtensions
    {
        public static IServiceCollection AddCircuitServicesAccesor(
            this IServiceCollection services)
        {
            services.AddScoped<UI.CircuitServicesAccesor.CircuitServicesAccesor>();
            services.AddScoped<CircuitHandler, ServicesAccessorCuircutHandler>();

            return services;
        }
    }
}
