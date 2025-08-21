using System.Security.Principal;
using Likvido.Identity.PrincipalProviders;
using Microsoft.AspNetCore.Http;

namespace Likvido.Web.PrincipalProviders;

public class WebPrincipalProvider(IHttpContextAccessor httpContextAccessor) : IPrincipalProvider
{
    public IPrincipal? User => httpContextAccessor.HttpContext?.User;

    public bool IsSystemProvider => false;
}
