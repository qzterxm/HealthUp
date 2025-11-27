namespace DataAccess.Models;

public class UserNoteDTO
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public string NoteText { get; set; } = string.Empty;
    public string NoteTitle { get; set; } = string.Empty;
}
