using System.Net;
using System.Text.Json;

namespace Ewallet.Exceptions;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public GlobalExceptionMiddleware(RequestDelegate next) => _next = next;

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                await HandleApiExceptionAsync(context, ex);
            }
            else
            {
                throw;
            }
        }
    }

    private static Task HandleApiExceptionAsync(HttpContext context, Exception ex)
    {
        context.Response.ContentType = "application/json";
        
        var statusCode = ex switch
        {
            WalletNotFoundException or UserNotFoundException => HttpStatusCode.NotFound,
            InsufficientBalanceException or InvalidAmountException or SelfTransferException => HttpStatusCode.BadRequest,
            _ => HttpStatusCode.InternalServerError
        };

        context.Response.StatusCode = (int)statusCode;
        var response = new { status = context.Response.StatusCode, error = ex.Message };

        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}