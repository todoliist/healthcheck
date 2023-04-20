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
    .AddCheck("Merrick Tenant DB Check",
        //localhost connectionString:"Data Source=localhost,1433;Initial Catalog=master;User Id=sa;Password=MyV3ryStrong@Passw0rd"
        new SqlConnectionHealthCheck(builder.Configuration.GetConnectionString("DefaultConnection")),
        HealthStatus.Unhealthy,
        new string[] { "SqlServer" })
    .AddApplicationInsightsPublisher(instrumentationKey: "ead27f2a-4f8f-46a3-8c5d-ab81fd07fe99");
//adding healthchecks UI
builder.Services.AddHealthChecksUI(opt =>
{
    opt.SetEvaluationTimeInSeconds(15); //time in seconds between check
    opt.MaximumHistoryEntriesPerEndpoint(60); //maximum history of checks
    opt.SetApiMaxActiveRequests(1); //api requests concurrency

    opt.AddHealthCheckEndpoint("default api", "/health"); //map health check api
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

app.MapHealthChecks("/health", new HealthCheckOptions()
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
app.MapHealthChecksUI();
app.MapControllers();
app.Run();
