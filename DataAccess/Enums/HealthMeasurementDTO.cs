using System.Text.Json.Serialization;
using Swashbuckle.AspNetCore.Annotations;

namespace DataAccess.Enums;

public class HealthMeasurementDTO
{     
    [JsonIgnore]
    [SwaggerSchema(ReadOnly = true)]
    public Guid? Id { get; set; }
    public Guid UserId { get; set; }
    public DateTime MeasuredAt { get; set; }
    public int? Systolic { get; set; }
    public int? Diastolic { get; set; }
    public int? HeartRate { get; set; }
   
    
}
