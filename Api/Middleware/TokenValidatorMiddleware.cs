﻿using Api.Services;
using Common.Extentions;
using Common.Consts;
using System.IdentityModel.Tokens.Jwt;
using System.Net;

namespace Api.Middleware
{
    public class TokenValidatorMiddleware
    {
        private readonly RequestDelegate _next;
        public TokenValidatorMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, AuthService authService)
        {
            var isOk = true;
            var sessionId = context.User.GetClaimValue<Guid>(ClaimNames.SessionId);
            if (sessionId != default)
            {
                var session = await authService.GetSessionById(sessionId);
                if (!session.IsActive)
                {
                    context.Response.Clear();
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
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
