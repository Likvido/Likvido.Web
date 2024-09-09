using JetBrains.Annotations;
using Likvido.Web.Services.IP;
using Likvido.Web.Services.Security;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Likvido.Web;

[PublicAPI]
public static class DependencyInjection
{
    public static IServiceCollection AddLikvidoWeb(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.TryAddSingleton<IIpAddressService, IpAddressService>();
        services.TryAddSingleton<IRedirectSecurityService, RedirectSecurityService>();

        return services;
    }
}
