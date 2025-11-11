
using System.ComponentModel.DataAnnotations;
using DataAccess.Enums;

namespace DataAccess.Models;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
    public Gender Gender { get; set; }
    public int Age { get; set; }
    public DateOnly?  DateOfBirth { get; set; }
    public string Country { get; set; }
    [Phone]
    public string PhoneNumber { get; set; }
    public UserRoles UserRole { get; set; }
    public string? ProfilePictureUrl { get; set; }
}