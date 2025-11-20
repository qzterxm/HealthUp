namespace DataAccess.Models;

public class FileForVisitDTO
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string? Base64Data { get; set; }
    public string? ContentType { get; set; }
}