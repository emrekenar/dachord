namespace WebApi.Middleware;

public class DevApiKeyMiddleware(RequestDelegate next, IConfiguration config, IWebHostEnvironment env)
{
    private const string HeaderName = "X-Dev-Key";

    public async Task InvokeAsync(HttpContext context)
    {
        if (!env.IsProduction() && !env.IsDevelopment())
        {
            var expectedKey = config["DevApiKey"];
            if (!string.IsNullOrEmpty(expectedKey))
            {
                if (!context.Request.Headers.TryGetValue(HeaderName, out var provided) || provided != expectedKey)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Missing or invalid X-Dev-Key header.");
                    return;
                }
            }
        }

        await next(context);
    }
}
