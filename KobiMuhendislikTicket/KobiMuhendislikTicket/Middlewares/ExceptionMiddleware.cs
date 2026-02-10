namespace KobiMuhendislikTicket.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IHostEnvironment _env;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, IHostEnvironment env, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _env = env;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try 
            { 
                await _next(context); 
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Beklenmedik hata: {Message}", ex.Message);
                
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = 500;
                
                if (_env.IsDevelopment())
                {
                    await context.Response.WriteAsJsonAsync(new
                    {
                        Message = "Sistemde beklenmedik bir hata oluştu.",
                        Detail = ex.Message,
                        StackTrace = ex.StackTrace
                    });
                }
                else
                {
                    await context.Response.WriteAsJsonAsync(new
                    {
                        Message = "Sistemde beklenmedik bir hata oluştu."
                    });
                }
            }
        }
    }
}
