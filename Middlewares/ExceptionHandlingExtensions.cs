namespace Online_Booking_System.Middlewares
{
  public static class ExceptionHandlingExtensions
  {
    public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder app) =>
        app.UseMiddleware<GlobalExceptionHandling>();
  }
}
