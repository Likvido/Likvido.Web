using Microsoft.AspNetCore.Http;

namespace Likvido.Web.Services.IP;

public class IpAddressService(IHttpContextAccessor httpContextAccessor) : IIpAddressService
{
    private const string CloudflareConnectingIp = "CF-Connecting-IP";
    private const string XForwardedFor = "X-Forwarded-For";

    public string? GetUserIpAddress()
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            return null;
        }

        // Try to get the IP from Cloudflare header
        if (httpContext.Request.Headers.TryGetValue(CloudflareConnectingIp, out var cloudflareIp))
        {
            return cloudflareIp.ToString();
        }

        // If Cloudflare header is not present, try X-Forwarded-For
        if (httpContext.Request.Headers.TryGetValue(XForwardedFor, out var forwardedIps))
        {
            // X-Forwarded-For can contain multiple IPs; the client IP is the first one
            var ips = forwardedIps.ToString().Split(',', StringSplitOptions.RemoveEmptyEntries);
            if (ips.Length > 0)
            {
                return ips[0].Trim();
            }
        }

        // If all else fails, fall back to the remote IP address
        return httpContext.Connection.RemoteIpAddress?.ToString();
    }
}
