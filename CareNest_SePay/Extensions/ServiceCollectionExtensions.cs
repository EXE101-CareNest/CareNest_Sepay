using CareNest_SePay.Middleware;

namespace CareNest_SePay.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddGlobalExceptionHandling(this IServiceCollection services)
        {
            // Middleware không cần đăng ký trong DI container
            // Chỉ cần sử dụng UseMiddleware trong pipeline
            return services;
        }
    }
}
