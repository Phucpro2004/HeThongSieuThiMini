using Microsoft.AspNetCore.Http;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace MiniShop.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                httpContext.Response.ContentType = "application/json";
                
                var statusCode = StatusCodes.Status500InternalServerError;
                var message = "Đã xảy ra lỗi hệ thống.";

                if (ex is Exceptions.BadRequestException badRequestEx)
                {
                    statusCode = StatusCodes.Status400BadRequest;
                    message = badRequestEx.Message;
                }
                else if (ex is Exceptions.NotFoundException notFoundEx)
                {
                    statusCode = StatusCodes.Status404NotFound;
                    message = notFoundEx.Message;
                }
                else if (ex is Exceptions.AppException appEx)
                {
                    statusCode = StatusCodes.Status400BadRequest;
                    message = appEx.Message;
                }
                else
                {
                    // For system exceptions, log the actual message (or hide it in prod).
                    // For now we just return the real message to help debugging, but in Prod we should mask it.
                    message = ex.Message;
                }

                httpContext.Response.StatusCode = statusCode;

                var response = new
                {
                    statusCode = statusCode,
                    message = message
                };

                var json = JsonSerializer.Serialize(response);
                await httpContext.Response.WriteAsync(json);
            }
        }
    }
}
