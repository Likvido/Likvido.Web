using JetBrains.Annotations;

namespace Likvido.Web.Services.IP;

[PublicAPI]
public interface IIpAddressService
{
    string? GetUserIpAddress();
}
