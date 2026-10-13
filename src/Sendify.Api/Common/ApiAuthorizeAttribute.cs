using Microsoft.AspNetCore.Authorization;

namespace Sendify.Api.Common;

public class ApiAuthorizeAttribute : AuthorizeAttribute
{
    public ApiAuthorizeAttribute()
    {
        Policy = "ApiTokenPolicy";
    }
}