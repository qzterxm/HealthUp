
using DataAccess.Enums;

namespace WebApplication1.Interfaces
{
   
    public class AverageAndLatestDataDTO
    {
     
        public double AverageHeartRate { get; set; } 
        public double AverageSystolic { get; set; }  
        public double AverageDiastolic { get; set; } 
      
        
        
        public int LatestHeartRate { get; set; }    
        public int LatestSystolic { get; set; }      
        public int LatestDiastolic { get; set; }     
        public double LatestHeight { get; set; }    
        public double LatestWeight { get; set; }     
        public string BloodGroup { get; set; }
        public double LatestSugar { get; set; }
    }

    public interface ICalculationService
    {
      
        Task<AverageAndLatestDataDTO> GetAverageAndLatestData(Guid userId);

        Task<double> CalculateIMT(double heightCm, double weightKg);
        Task<double> AverageBP(Guid userId); 
        Task<double> AverageHR(Guid userId); 
        string GetBloodGroupString(BloodType? type);
        int MinSystolic { get; }
        int MaxSystolic { get; }
        int MinDiastolic { get; }
        int MaxDiastolic { get; }
        int MinHeartRate { get; }
        int MaxHeartRate { get; }
    }
}