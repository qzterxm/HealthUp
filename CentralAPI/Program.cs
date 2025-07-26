using DataAccess.DataAccess;
using DataAccess.Implementation;
using DataAccess.Interfaces;
using MeetUp.TrustLocker.DataAccess;
using WebApplication1.Implementation;
using WebApplication1.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService> ();
builder.Services.AddScoped<IDbAccessService, DbAccessService>();




var app = builder.Build();
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

app.Run();


