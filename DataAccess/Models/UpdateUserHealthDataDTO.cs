using System.ComponentModel.DataAnnotations;
using DataAccess.Enums;

namespace DataAccess.Models
{
    public class UpdateUserHealthDataDTO
    {
        [Required]
        public Guid UserId { get; set; }
        
        [Range(1, 150)]
        public int Age { get; set; }
        
        public Gender Gender { get; set; }
        
        public DateOnly? DateOfBirth { get; set; }
        
        public string Country { get; set; }
        
        [Phone]
        public string PhoneNumber { get; set; }
        
        public BloodType? BloodType { get; set; }
        
        [Range(0, 50)]
        public double? SugarLevel { get; set; } 
        
        [Range(1, 300)]
        public int? Height { get; set; } 
        
        [Range(1, 500)]
        public int? Weight { get; set; } 
    }
}