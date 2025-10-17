namespace DataAccess.Models;

public class UserFile
{
    public Guid Id { get; set; } 
    public string FileName { get; set; } 
    public string ContentType { get; set; } 
    public byte[] FileData { get; set; } 
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    public Guid? UserId { get; set; } 
}
