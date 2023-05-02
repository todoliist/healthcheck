using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services
    .AddHealthChecks()
     .AddCheck("urappService", () =>
        HealthCheckResult.Healthy("App Service is healthy."), tags: new string[] { "appServicename" })
        .AddCheck("urdb1",
        new DbHealthCheck("Server=tcp:timdbserver.database.windows.net,1433;Initial Catalog=timsql;Persist Security Info=False;User ID=CloudSA0b4fc139;Password=Welcome54321!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"),
        HealthStatus.Unhealthy,
        new string[] { "db1name" })
        .AddCheck("urdb2",
        new DbHealthCheck("Data Source=tcp:10.216.116.127,1433;Initial Catalog=Mer01_ArcSarcDev1_Procount;User ID=sa;Password=password$1;MultipleActiveResultSets=False;Connection Timeout=10;ConnectRetryCount=3;ConnectRetryInterval=5;Encrypt=false;TrustServerCertificate=true;"),
        HealthStatus.Unhealthy,
        new string[] { "db2name" })
    .AddApplicationInsightsPublisher(instrumentationKey: "ead27f2a-4f8f-46a3-8c5d-ab81fd07fe99");
//adding healthchecks UI
builder.Services.AddHealthChecksUI(opt =>
{
    opt.SetEvaluationTimeInSeconds(15); //time in seconds between check
    opt.MaximumHistoryEntriesPerEndpoint(60); //maximum history of checks
    opt.SetApiMaxActiveRequests(1); //api requests concurrency
    // align with /dbhealth be below, means when HealthChecksUI activated, it will touch /dbhealth and only run healthcheck for tags db1name and db2name
    opt.AddHealthCheckEndpoint("default api", "/dbhealth"); //map health check api
})
.AddInMemoryStorage();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapHealthChecks("/apphealth", new HealthCheckOptions()
{
    Predicate = (check) => check.Tags.Contains("appServicename"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/dbhealth", new HealthCheckOptions()
{
    Predicate = (check) => check.Tags.Contains("db1name") | check.Tags.Contains("db2name"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
app.MapHealthChecksUI();
app.MapControllers();
app.Run();
