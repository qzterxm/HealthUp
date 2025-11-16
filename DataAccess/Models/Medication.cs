using System.Text.Json;
using System.Text.Json.Serialization;

namespace DataAccess.Models
{
    public class MedicationTime
    {
        [JsonPropertyName("time")]
        public string Time { get; set; }
    }

    public class Medication
    {
        [JsonIgnore]
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public string NameOfMedication { get; set; }
        public string Dose { get; set; }
        public string WeekDaysJson { get; set; }
        public string Duration { get; set; }
        public string Type { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<MedicationTime> Times { get; set; } = new();

        public string TimesJson
        {
            get => JsonSerializer.Serialize(Times);
            set => Times = string.IsNullOrEmpty(value) ? new List<MedicationTime>() : JsonSerializer.Deserialize<List<MedicationTime>>(value);
        }
        
        
    }
    
}