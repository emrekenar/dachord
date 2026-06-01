using Amazon.CloudWatch.EMF.Config;
using Amazon.CloudWatch.EMF.Logger;
using WebApi.Configuration;
using WebApi.Endpoints;
using WebApi.Middleware;

var builder = WebApplication.CreateBuilder(args);

var isLambda = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("AWS_LAMBDA_FUNCTION_NAME"));

// JSON logs in Lambda (CloudWatch), plain console locally
builder.Logging.ClearProviders();
if (isLambda)
    builder.Logging.AddJsonConsole(o => o.JsonWriterOptions = new() { Indented = false });
else
    builder.Logging.AddSimpleConsole(o => { o.SingleLine = true; o.IncludeScopes = false; });

// secrets.json is gitignored — copy it locally or into any deployment environment as needed
builder.Configuration.AddJsonFile("secrets.json", optional: true, reloadOnChange: false);

if (isLambda)
{
    var region = Amazon.RegionEndpoint.GetBySystemName(builder.Configuration["AWS:Region"] ?? "eu-central-1");
    var ssmPath = builder.Configuration["SSM:ParameterPath"] ?? "/dachord/prod/";
    builder.Configuration.AddSystemsManager(ssmPath, new Amazon.Extensions.NETCore.Setup.AWSOptions { Region = region });
    builder.Services.AddAWSLambdaHosting(LambdaEventSource.RestApi);
}

// EMF: custom metrics emitted as structured log lines — free, no CloudWatch Metrics API calls
EnvironmentConfigurationProvider.Config.ServiceName = builder.Configuration["AWS:ServiceName"] ?? "dachord-api";
EnvironmentConfigurationProvider.Config.LogGroupName = builder.Configuration["AWS:MetricsLogGroup"] ?? "/dachord/api/metrics";
builder.Services.AddScoped<IMetricsLogger, MetricsLogger>();

ServiceConfiguration.ConfigureServices(builder);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<LambdaMethodFixMiddleware>();
app.UseCors();
app.UseMiddleware<DevApiKeyMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

EndpointMapper.MapEndpoints(app);

app.Run();
