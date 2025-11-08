namespace DataAccess.Models;

public class FileForVisitDTO
{
    public string FileName { get; set; } = string.Empty;
    public string? Base64Data { get; set; }
    public string? ContentType { get; set; } 
}