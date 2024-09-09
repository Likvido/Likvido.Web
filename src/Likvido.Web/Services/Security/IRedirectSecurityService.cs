using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;

namespace Likvido.Web.Services.Security;

[PublicAPI]
public interface IRedirectSecurityService
{
    void EnsureLinkIsLikvido(string url, IUrlHelper urlHelper);
}
