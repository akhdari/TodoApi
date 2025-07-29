using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading.Tasks;


//It takes an HttpContext and returns a Task
/*
A Request Delegate in ASP.NET Core is a function that handles an HTTP request. 
It processes the incoming request and produces a response.
*/

namespace TodoApi.Middlewares
{
   public class GlobalExceptionHandling : IMiddleware
{
    private readonly ILogger<GlobalExceptionHandling> _logger;

    public GlobalExceptionHandling(ILogger<GlobalExceptionHandling> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
            try
            {
                await next(context);
                Console.WriteLine("Request processed successfully.");
        }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred.");
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                await context.Response.WriteAsync("An unexpected error occurred. Please try again later.");
            }
    }
}
}

/*
Intercepts all requests passing through the pipeline.

Tries to process the request by calling _next(context), forwarding it to the next middleware or endpoint (controller).

If an exception occurs, it:

Logs the error using the injected ILogger

Sets the HTTP response status to 500 (Internal Server Error)

Writes a friendly message back to the client:
"An unexpected error occurred. Please try again later."


*/
