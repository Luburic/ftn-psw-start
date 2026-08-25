using Microsoft.Extensions.DependencyInjection;

namespace Payment.Api;

public static class PaymentApiExtensions
{
    public static IMvcBuilder AddPaymentControllers(this IMvcBuilder mvc)
    {
        return mvc.AddApplicationPart(typeof(PaymentApiExtensions).Assembly);
    }
}
