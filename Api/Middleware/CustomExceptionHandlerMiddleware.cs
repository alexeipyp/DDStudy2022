using Common.CustomExceptions;
using Microsoft.IdentityModel.Tokens;
using System.Net;
using System.Text.Json;

namespace Api.Middleware
{
    public class CustomExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        public CustomExceptionHandlerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch(Exception exception)
            {
                await HandleException(context, exception);
            }
        }

        private async Task HandleException(HttpContext context, Exception exception)
        {
            var code = HttpStatusCode.InternalServerError;
            var result = JsonSerializer.Serialize(new 
            { 
                Error = exception.Message, 
                ExceptionType = exception.GetType().FullName,
            });
            switch(exception)
            {
                case SecurityTokenException:
                    code = HttpStatusCode.Unauthorized;
                    break;
                case FileNotFoundException:
                    code = HttpStatusCode.NotFound;
                    break;
                case DirectoryNotFoundException:
                    code = HttpStatusCode.NotFound;
                    break;
                // Custom Exceptions
                case SessionNotFoundException:
                    code = HttpStatusCode.BadRequest;
                    break;
                case SessionNotActiveException:
                    code = HttpStatusCode.BadRequest;
                    break;
                case UserNotFoundException:
                    code = HttpStatusCode.BadRequest;
                    break;
                case UserCreationException:
                    code = HttpStatusCode.BadRequest;
                    break;
                case FileAlreadyExistsException:
                    code = HttpStatusCode.BadRequest;
                    break;
                case FileIsNullException:
                    code = HttpStatusCode.BadRequest;
                    break;
            }
            context.Response.StatusCode = (int)code;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsync(result);
        }
    }

    public static class CustomExceptionHandlerMiddlewareExtentions
    {
        public static IApplicationBuilder UseCustomExceptionHandler(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CustomExceptionHandlerMiddleware>();
        }
    }
}
