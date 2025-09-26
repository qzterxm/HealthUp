using DataAccess.Interfaces;
using WebApplication1.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Implementation
{
    public class CalculationService : ICalculationService
    {
        private readonly IUserRepository _userRepository;

        public CalculationService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public int MinSystolic => 100;
        public int MaxSystolic => 160;
        public int MinDiastolic => 80;
        public int MaxDiastolic => 130;
        public int MinHeartRate => 50;
        public int MaxHeartRate => 160;
 

        
        public async Task<double> CalculateIMT(double height, double weight)
        {
            if (height <= 0) throw new ArgumentException("Height must be greater than 0");
            double heightM = height/ 100.0;
            double imt = weight / (heightM * heightM);
            return await Task.FromResult(imt);
        }

        
        public async Task<double> AverageBP(Guid userId)
        {
            var measurements = await _userRepository.GetMeasurements(userId);
            if (!measurements.Any())
                return 0;

            double avgSystolic = measurements.Average(m => m.Systolic ?? 0);
            double avgDiastolic = measurements.Average(m => m.Diastolic ?? 0);
            return (avgSystolic + avgDiastolic) / 2;
        }

      
        public async Task<double> AverageHR(Guid userId)
        {
            var measurements = await _userRepository.GetMeasurements(userId);
            if (!measurements.Any())
                return 0;

            double avgHR = measurements.Average(m => m.HeartRate ?? 0);
            return avgHR;
        }
    }
}
