using DataAccess.DataAccess;
using DataAccess.Implementation;
using DataAccess.Interfaces;
using Serilog;
using WebApplication1.Implementation;
using WebApplication1.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddControllers(); 

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IDbAccessService, DbAccessService>();



// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog((context, configuration) =>
{
    configuration.ReadFrom.Configuration(context.Configuration);
});


var app = builder.Build();
// SEED DATA
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var dbAccessService = services.GetRequiredService<IDbAccessService>();
    var seeder = new DataSeeder(dbAccessService);

    await seeder.Seed();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();
