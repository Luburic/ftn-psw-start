using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Payment.Infrastructure;

public static class PaymentModuleExtensions
{
    public static IServiceCollection AddPaymentModule(this IServiceCollection services, IConfiguration configuration)
    {
        return services;
    }
}
