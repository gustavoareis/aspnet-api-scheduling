using System.Text.Json.Serialization;
using aspnet_api_scheduling.Data;
using aspnet_api_scheduling.Middleware;
using aspnet_api_scheduling.Services;
using aspnet_api_scheduling.Validators;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Barbearia")
    ?? throw new InvalidOperationException("A connection string 'Barbearia' precisa estar configurada.");
var timeZoneId = builder.Configuration["Scheduling:TimeZoneId"] ?? "America/Sao_Paulo";
var businessTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));
builder.Services.AddSingleton(businessTimeZone);
builder.Services.AddScoped<ICatalogService, CatalogService>();
builder.Services.AddScoped<IAgendamentoService, AgendamentoService>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateAgendamentoRequestValidator>();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddOpenApi();

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
}

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapControllers();
app.Run();
