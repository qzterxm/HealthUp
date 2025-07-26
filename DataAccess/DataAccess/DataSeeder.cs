using System.Security.Cryptography;
using System.Text;
using DataAccess.DataAccess;
using DataAccess.Enums;
using WebApplication1.Enums;

namespace MeetUp.TrustLocker.DataAccess
{
    public class DataSeeder(IDbAccessService dbAccessServices)
    {
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
                UserRole = UserRoles.Admin
            };
            var existAdmin = await dbAccessServices.GetRecordById<User>(StoredProceduresNames.GetUserById, admin.Id);

            if (existAdmin == null)
            {
                admin.Password = HashPassword(admin.Password);
                var addResult = await dbAccessServices.AddRecord(StoredProceduresNames.CreateUser, admin);
            }
        }

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }
}
