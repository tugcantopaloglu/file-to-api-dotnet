using FileToApi.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace FileToApi.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class ConditionalAuthorizeAttribute : Attribute, IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var authSettings = context.HttpContext.RequestServices
            .GetRequiredService<IOptions<AuthenticationSettings>>().Value;

        // Allow anonymous access if authentication is disabled or AllowAnonymous is enabled
        if (!authSettings.Enabled || authSettings.AllowAnonymous)
        {
            return;
        }

        var authenticateResult = await context.HttpContext.AuthenticateAsync();

        if (!authenticateResult.Succeeded || authenticateResult.Principal?.Identity?.IsAuthenticated != true)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var authService = context.HttpContext.RequestServices
            .GetRequiredService<IAuthorizationService>();

        var authorizeResult = await authService.AuthorizeAsync(
            authenticateResult.Principal,
            null,
            new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

        if (!authorizeResult.Succeeded)
        {
            context.Result = new ForbidResult();
        }
    }
}
