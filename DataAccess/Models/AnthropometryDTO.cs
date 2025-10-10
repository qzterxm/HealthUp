namespace DataAccess.Models;

public class AnthropometryDTO
{
    public Guid UserId { get; set; }
    public DateTime MeasuredAt { get; set; }
    public double? Weight { get; set; }
    public int? Height { get; set; }
}
