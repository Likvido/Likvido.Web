using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;

namespace Likvido.Web.Services.IP;

[PublicAPI]
public interface IWebSecurityService
{
    void EnsureLinkIsLikvido(string url, IUrlHelper urlHelper);
}
