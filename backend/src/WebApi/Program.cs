using WebApi.Configuration;
using WebApi.Endpoints;

var builder = WebApplication.CreateBuilder(args);

ServiceConfiguration.ConfigureServices(builder);

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

EndpointMapper.MapEndpoints(app);

app.Run();
