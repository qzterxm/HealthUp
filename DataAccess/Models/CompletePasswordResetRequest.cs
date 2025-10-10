namespace DataAccess.Models;

public class CompletePasswordResetRequest
{
    public string Email { get; set; }
    public int ResetCode { get; set; }
    public string NewPassword { get; set; }
}