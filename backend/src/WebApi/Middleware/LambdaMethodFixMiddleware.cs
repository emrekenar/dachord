using Amazon.Lambda.APIGatewayEvents;

namespace WebApi.Middleware;

// Amazon.Lambda.AspNetCoreServer.Hosting incorrectly defaults the HTTP method to GET
// when deserializing API Gateway events. The original event is available in HttpContext.Items
// under "LambdaRequestObject" — read the real method from there and fix it.
public class LambdaMethodFixMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Items.TryGetValue("LambdaRequestObject", out var raw))
        {
            var method = raw switch
            {
                APIGatewayProxyRequest r => r.HttpMethod,
                APIGatewayHttpApiV2ProxyRequest r => r.RequestContext?.Http?.Method,
                _ => null
            };

            if (!string.IsNullOrEmpty(method))
                context.Request.Method = method;
        }

        await next(context);
    }
}
