using DataAccess.DataAccess;
using DataAccess.Enums;
using DataAccess.Models;
using System.Security.Cryptography;
using System.Text;

public class DataSeeder
{
    private readonly IDbAccessService _dbAccessServices;

    public DataSeeder(IDbAccessService dbAccessServices)
    {
        _dbAccessServices = dbAccessServices;
    }

    public async Task Seed()
    {
        await SeedAdminUser();
    }

    private async Task SeedAdminUser()
    {
        var admin = new User()
        {
            Id = Guid.Parse("E31DFE6A-4EE8-4CDC-8D01-DE3468A18C17"),
            Email = "admin@gmail.com",
            UserName = "admin",
            Password = "admin",
            Gender = Gender.Male,
            Age = 30,
            DateOfBirth = new DateOnly(1990, 1, 1),
            Country = "Unknown",
            PhoneNumber = "",
            UserRole = UserRoles.Admin,
            ProfilePictureUrl = null
        };

        try
        {
            
            var existingAdmin = await _dbAccessServices.GetOneByParameter<User>(SqlQueries.GetUserByEmail, "Email", admin.Email);

            if (existingAdmin == null)
            {
                
                admin.Password = HashPassword("admin"); 
                var addResult = await _dbAccessServices.AddRecord(SqlQueries.CreateUser, admin);

                if (addResult <= 0)
                {
                    throw new Exception("Failed to insert admin user into database.");
                }
                else
                {
                    Console.WriteLine("Admin user seeded successfully.");
                }
            }
            else
            {
                Console.WriteLine("Admin user already exists in the database.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during admin seeding: {ex.Message}");
            throw;
        }
    }

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }
}