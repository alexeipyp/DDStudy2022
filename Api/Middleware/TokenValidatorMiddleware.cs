using Api.Services;
using System.IdentityModel.Tokens.Jwt;

namespace Api.Middleware
{
    public class TokenValidatorMiddleware
    {
        private readonly RequestDelegate _next;
        public TokenValidatorMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, UserService userService)
        {
            var isOk = true;
            var sessionIdString = context.User.Claims.FirstOrDefault(x => x.Type == "sessionId")?.Value;
            if (Guid.TryParse(sessionIdString, out var sessionId))
            {
                var session = await userService.GetSessionById(sessionId);
                if (!session.IsActive)
                {
                    context.Response.Clear();
                    context.Response.StatusCode = 401;
                }
            }
            if (isOk)
            {
                await _next(context);
            }
        }
    }

    public static class TokenValidatorMiddlewareExtentions
    {
        public static IApplicationBuilder UseTokenValidator(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<TokenValidatorMiddleware>();
        }
    }
}
