using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using DataAccess.DataAccess;
using DataAccess.Implementation;
using DataAccess.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Microsoft.IdentityModel.Logging;
using WebApplication1.Implementation;
using WebApplication1.Interfaces;


IdentityModelEventSource.ShowPII = true;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers().AddJsonOptions(opts =>
{
    // конвертер, який дозволяє серіалізувати/десеріалізувати enum як рядок
    opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    opts.JsonSerializerOptions.TypeInfoResolverChain.Add(new DefaultJsonTypeInfoResolver());
}); 

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();


// Configure JWT authentication
builder.Services.AddSwaggerGen(options =>
{
    // This ensures the OpenAPI 3.0 specification version is set
    options.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "HealthUp API", 
        Version = "v1",
        Description = "API for HealthUp application",
        Contact = new OpenApiContact
        {
            Name = "Your Name",
            Email = "contact@healthup.com"
        }
    });

    // Add JWT Bearer token support
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

    // This ensures the OpenAPI specification version is included in the document
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
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
               
                var accessToken = context.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(accessToken))
                    context.Token = accessToken;
                return Task.CompletedTask;
            }
        };
        options.Events.OnAuthenticationFailed = ctx =>
        {
          
            Console.Error.WriteLine($"{ctx.Exception}, JWT auth failed");
            return Task.CompletedTask;
        };
    });


// Register services
builder.Services.AddScoped<IDbAccessService, DbAccessService>();

//users & auth
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICalculationService, CalculationService>();
//helpers
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IPasswordHelperService, PasswordHelperService>();


// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog((context, configuration) =>
{
    configuration.ReadFrom.Configuration(context.Configuration);
});



// Build the application
var app = builder.Build();



// SEED DATA
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var dbAccessService = services.GetRequiredService<IDbAccessService>();
    var seeder = new DataSeeder(dbAccessService);

    await seeder.Seed();
}


app.UseSwagger();
app.UseSwaggerUI();



app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

var audience = builder.Configuration["JwtSettings:Audience"];


app.Run();

