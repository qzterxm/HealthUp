using System.Text.Json.Serialization;
using DataAccess.Enums;
using DataAccess.Models;

public class DoctorVisit
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Specialist { get; set; } = string.Empty;
    public string VisitType { get; set; } = string.Empty;
    public string? Diagnosis { get; set; }
    public string? Prescription { get; set; }
    public DateTime VisitedAt { get; set; } = DateTime.UtcNow;


}