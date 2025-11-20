namespace DataAccess.Models;

public class SleepDTO
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DateTime Date { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int TotalDurationMinutes { get; set; }
    public int SleepScore { get; set; }
    public string SleepStatus { get; set; } = string.Empty;
    
    public void CalculateDuration()
    {
        if (EndTime > StartTime)
        {
            TotalDurationMinutes = (int)(EndTime - StartTime).TotalMinutes;
        }
        else
        {
            TotalDurationMinutes = (int)((EndTime.AddDays(1) - StartTime).TotalMinutes);
        }
    }
    
    public void CalculateSleepScore()
    {
        var durationHours = TotalDurationMinutes / 60.0;
        
        double durationScore = durationHours switch
        {
            < 4 => 30,
            >= 4 and < 6 => 50,
            >= 6 and < 7 => 70,
            >= 7 and <= 9 => 90,
            > 9 => 60,
            _ => 0
        };
        
        double timingScore;
        if (StartTime.Hour >= 22 || StartTime.Hour < 3)
        {
            timingScore = 80;  
        }
        else
        {
            timingScore = 60;   
        }
        
        SleepScore = (int)((durationScore * 0.7) + (timingScore * 0.3));
        SleepStatus = SleepScore switch
        {
            >= 85 => "Excellent",
            >= 70 and < 85 => "Good",
            >= 50 and < 70 => "Fair",
            _ => "Poor"
        };
    }
}