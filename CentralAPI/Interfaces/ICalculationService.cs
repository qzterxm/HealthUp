

namespace WebApplication1.Interfaces
{
    public interface ICalculationService
    {
        
        Task<double> CalculateIMT(double heightCm, double weightKg);
        Task<double> AverageBP(Guid userId);
        Task<double> AverageHR(Guid userId);

       
        int MinSystolic { get; }
        int MaxSystolic { get; }
        int MinDiastolic { get; }
        int MaxDiastolic { get; }
        int MinHeartRate { get; }
        int MaxHeartRate { get; }
   
    }
}