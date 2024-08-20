using JetBrains.Annotations;
using Likvido.Web.Services.IP;
using Microsoft.Extensions.DependencyInjection;

namespace Likvido.Web;

[PublicAPI]
public static class DependencyInjection
{
    public static IServiceCollection AddLikvidoWeb(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<IIpAddressService, IpAddressService>();

        return services;
    }
}
