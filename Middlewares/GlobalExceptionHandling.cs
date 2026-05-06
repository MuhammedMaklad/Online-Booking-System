using Microsoft.AspNetCore.Mvc;

namespace Online_Booking_System.Middlewares
{
  public class GlobalExceptionHandling
  {
    private readonly ILogger<GlobalExceptionHandling> _logger;
    private readonly RequestDelegate _next;
    private readonly IWebHostEnvironment _env;
    public GlobalExceptionHandling(ILogger<GlobalExceptionHandling> _logger, RequestDelegate _next, IWebHostEnvironment _env)
    {
      this._logger = _logger;
      this._next = _next;
      this._env = _env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
      try
      {
        await _next(context);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex.Message, "An unhandled exception occurred.");

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

        var problemDetails = new ProblemDetails
        {
          Status = StatusCodes.Status500InternalServerError,
          Title = "An error occurred while processing your request.",
          Detail = _env.IsDevelopment() ? ex.Message : null,
        };

        await context.Response.WriteAsJsonAsync(problemDetails);
      }
    }
  }
}