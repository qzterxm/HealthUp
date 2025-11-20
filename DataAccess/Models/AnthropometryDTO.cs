using DataAccess.Enums;

namespace DataAccess.Models;

public class AnthropometryDTO
{
    public Guid UserId { get; set; }
    public DateTime MeasuredAt { get; set; }
    public double? Weight { get; set; }
    public int? Height { get; set; }
    public double? Sugar { get; set; }
    public BloodType? BloodType { get; set; }
    public int? Age { get; set; }

}
