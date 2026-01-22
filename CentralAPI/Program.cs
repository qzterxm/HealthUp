using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Dapper;
using DataAccess.DataAccess; // Перевір, щоб тут не дублювались namespace
using DataAccess.Implementation; // Якщо твої репозиторії тут
using DataAccess.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Microsoft.IdentityModel.Logging;
using WebApplication1.EmailSender;
using WebApplication1.Implementation;
using WebApplication1.Interfaces;
// using SQLitePCL;  <-- ВИДАЛЯЄМО ЦЕ

IdentityModelEventSource.ShowPII = true;

var builder = WebApplication.CreateBuilder(args);

// --- Dapper Type Handlers ---
SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());
SqlMapper.AddTypeHandler(new NullableDateOnlyTypeHandler());
// GuidTypeHandler ВИДАЛЯЄМО - PostgreSQL вміє працювати з UUID нативно!

// Add services to the container.
builder.Services.AddControllers().AddJsonOptions(opts =>
{
    opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    opts.JsonSerializerOptions.TypeInfoResolverChain.Add(new DefaultJsonTypeInfoResolver());
}); 

builder.Services.AddEndpointsApiExplorer();

// Configure JWT & Swagger
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "HealthUp API", 
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."
    });
    
    options.OperationFilter<AuthResponsesOperationFilter>();
    options.DocumentFilter<OpenApiVersionFilter>();
});

builder.Services.AddAuthorization();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"])),
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.Zero,
        };
    });

// --- Register services ---
builder.Services.AddScoped<IDbAccessService, DbAccessService>();

// Users & Auth
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICalculationService, CalculationService>();
builder.Services.AddScoped<IPasswordResetService, PasswordResetService>();

// Helpers
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IPasswordHelperService, PasswordHelperService>();

// File
builder.Services.AddScoped<IFileService, FileService>();

// EmailSender
var emailSettings = builder.Configuration.GetSection("EmailSettings").Get<EmailSettings>();
builder.Services.AddSingleton(emailSettings);
builder.Services.AddTransient<IEmailService, EmailSender>();
builder.Services.AddTransient<UseEmailSender>();

// Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog((context, configuration) =>
{
    configuration.ReadFrom.Configuration(context.Configuration);
});

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = 10 * 1024 * 1024; 
});

var app = builder.Build();

// --- INITIALIZATION BLOCK (Виправлено) ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        // 1. Отримуємо сервіс через Інтерфейс
        var dbAccessService = services.GetRequiredService<IDbAccessService>();
        
        // 2. СПОЧАТКУ створюємо таблиці (важливо!)
        logger.LogInformation("Initializing database tables...");
        await dbAccessService.InitDatabase(); 

        // 3. ПОТІМ засіваємо дані (Адмін)
        logger.LogInformation("Seeding data...");
        var seeder = new DataSeeder(dbAccessService);
        await seeder.Seed();
        
        logger.LogInformation("Database initialization completed.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while initializing the database.");
    }
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors(x => x
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

app.UseAuthorization();
app.MapControllers();

app.Run();