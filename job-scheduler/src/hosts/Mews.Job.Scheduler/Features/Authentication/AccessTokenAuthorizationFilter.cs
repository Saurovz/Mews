using System.Collections.Immutable;
using Mews.Job.Scheduler.BuildingBlocks.Configuration.Authentication;
using Mews.Job.Scheduler.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace Mews.Job.Scheduler.Features.Authentication;

public sealed class AccessTokenAuthorizationFilter : IAuthorizationFilter
{
    private readonly ImmutableHashSet<string> _accessTokens;

    public AccessTokenAuthorizationFilter(IOptions<AuthenticationTokensConfiguration> configuration)
    {
        _accessTokens = configuration.Value.AccessTokens?.ToImmutableHashSet() ?? ImmutableHashSet<string>.Empty;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        if (!context.HttpContext.Request.Headers.TryGetValue(HttpConstants.AccessTokenHeaderName, out var accessTokenHeaderValue))
        {
            context.Result = new UnauthorizedObjectResult($"Missing {HttpConstants.AccessTokenHeaderName}.");
            return;
        }

        var accessToken = accessTokenHeaderValue.ToString();
        if (!_accessTokens.Contains(accessToken))
        {
            context.Result = new UnauthorizedObjectResult($"Invalid {HttpConstants.AccessTokenHeaderName}.");
        }
    }
}
