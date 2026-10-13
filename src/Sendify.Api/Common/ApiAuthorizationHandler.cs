using Microsoft.AspNetCore.Authorization;

namespace Sendify.Api.Common;

public class ApiAuthorizationHandler : AuthorizationHandler<ApiTokenRequirement>
{
    private readonly ITokensService _tokensService;

    public ApiAuthorizationHandler(ITokensService tokensService)
    {
        _tokensService = tokensService;
    }

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ApiTokenRequirement requirement)
    {
        if (context.Resource is DefaultHttpContext resource)
        {
            if(resource.Request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                var token = authHeader.ToString().Replace("Bearer ", string.Empty);

                if (!_tokensService.IsTokenRevoked(token))
                {
                    context.Succeed(requirement);
                }
            }
        }

        return Task.CompletedTask;
    }
}
