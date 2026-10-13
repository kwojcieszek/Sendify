using System.Security.Claims;

namespace Sendify.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static string? UserId(this ClaimsPrincipal claim)
    {
        var claimsIdentity = claim.Identity as ClaimsIdentity;
        return claimsIdentity!.FindFirst(ClaimTypes.PrimarySid)?.Value;
    }

    public static string? GroupId(this ClaimsPrincipal claim)
    {
        var claimsIdentity = claim.Identity as ClaimsIdentity;
        return claimsIdentity!.FindFirst(ClaimTypes.PrimaryGroupSid)?.Value;
    }
}