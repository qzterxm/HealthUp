namespace DataAccess.Models;

public class FileForVisitDTO
{
    public string FileName { get; set; } = string.Empty;
    public string? Base64Data { get; set; } // Файл у вигляді Base64 рядка
    public string? ContentType { get; set; } 
}