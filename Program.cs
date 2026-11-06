using System.Text.Json.Serialization;
using aspnet_api_scheduling.Data;
using aspnet_api_scheduling.Middleware;
using aspnet_api_scheduling.Models;
using aspnet_api_scheduling.Services;
using aspnet_api_scheduling.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Barbearia")
    ?? throw new InvalidOperationException("A connection string 'Barbearia' precisa estar configurada.");
var timeZoneId = builder.Configuration["Scheduling:TimeZoneId"] ?? "America/Sao_Paulo";
var businessTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
var jwtSettings = JwtSettings.Create(builder.Configuration, builder.Environment);

builder.Services.AddScoped<IBarbeariaContext, BarbeariaContext>();
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));
builder.Services.AddSingleton(businessTimeZone);
builder.Services.AddSingleton(jwtSettings);
builder.Services.AddScoped<DatabaseSchemaInitializer>();
builder.Services.AddScoped<JwtTokenService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<ICatalogService, CatalogService>();
builder.Services.AddScoped<IAgendamentoService, AgendamentoService>();
builder.Services.AddIdentityCore<Usuario>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.Password.RequiredLength = 12;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddSignInManager();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = jwtSettings.SigningKey,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });
builder.Services.AddAuthorization();
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
    var schemaInitializer = scope.ServiceProvider.GetRequiredService<DatabaseSchemaInitializer>();
    await schemaInitializer.InitializeAsync();
}

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseMiddleware<BarbeariaContextMiddleware>();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapControllers();
app.Run();
