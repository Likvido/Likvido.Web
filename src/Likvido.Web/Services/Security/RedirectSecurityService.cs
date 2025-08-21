using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace Likvido.Web.Services.Security;

public class RedirectSecurityService(IHttpContextAccessor httpContextAccessor) : IRedirectSecurityService
{
    public void EnsureLinkIsLikvido(string url, IUrlHelper urlHelper)
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            return;
        }

        // safety check for the open-redirect vulnerability
        // we don't want to accept links that are not local to Likvido
        if (!string.IsNullOrWhiteSpace(url))
        {
            if (urlHelper.IsLocalUrl(url))
            {   
                // we're good
                return;
            }

            // this can be a link with a host of the same domain (and in that case, it should be allowed)
            var pattern = @"^(http|https):\/\/(([a-zA-Z0-9\-\.]+)\.([a-zA-Z]{2,5})|localhost)(:\d+)?(\/.*)?$";
            var regex = new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);

            if (regex.IsMatch(url))
            {
                var uri = new Uri(url);
                if (uri.Host == httpContext.Request.Host.Host)
                {
                    // we're good
                    return;
                }

                throw new ApplicationException(string.Format("URL '{0}' is not local to Likvido. URI host is '{1}' and Request host is '{2}'.", url, uri.Host, httpContext.Request.Host.Host));
            }

            throw new ApplicationException(string.Format("The URL '{0}' is not well formed", url));
        }
    }
}
