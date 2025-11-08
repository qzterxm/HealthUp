using DataAccess.Interfaces;
using WebApplication1.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;
using DataAccess.Enums;

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
 
        public async Task<AverageAndLatestDataDTO> GetAverageAndLatestData(Guid userId)
        {
            var measurementsTask = _userRepository.GetMeasurements(userId);
            var anthropometryTask = _userRepository.GetLatestAnthropometry(userId);

            await Task.WhenAll(measurementsTask, anthropometryTask);

            var measurements = measurementsTask.Result;
            var latestAnthropometry = anthropometryTask.Result;

            double avgHR = measurements.Any() ? measurements.Average(m => m.HeartRate ?? 0) : 0;
            double avgSystolic = measurements.Any() ? measurements.Average(m => m.Systolic ?? 0) : 0;
            double avgDiastolic = measurements.Any() ? measurements.Average(m => m.Diastolic ?? 0) : 0;

            var latestMeasurement = measurements.OrderByDescending(m => m.MeasuredAt).FirstOrDefault();

            return new AverageAndLatestDataDTO
            {
                AverageHeartRate = avgHR,
                AverageSystolic = avgSystolic,
                AverageDiastolic = avgDiastolic,
                LatestHeartRate = latestMeasurement?.HeartRate ?? 0,
                LatestSystolic = latestMeasurement?.Systolic ?? 0, 
                LatestDiastolic = latestMeasurement?.Diastolic ?? 0, 
                LatestHeight = latestAnthropometry?.Height ?? 0.0,
                LatestWeight = latestAnthropometry?.Weight ?? 0.0,
                BloodGroup = GetBloodGroupString(latestAnthropometry?.BloodType),
                LatestSugar = latestAnthropometry?.Sugar ?? 0.0
            };
        }

        public string GetBloodGroupString(BloodType? type)
        {
            return type switch
            {
                BloodType.A_Positive => "A+",
                BloodType.A_Negative => "A-",
                BloodType.B_Positive => "B+",
                BloodType.B_Negative => "B-",
                BloodType.AB_Positive => "AB+",
                BloodType.AB_Negative => "AB-",
                BloodType.O_Positive => "O+",
                BloodType.O_Negative => "O-",
                _ => "N/A"
            };
        }
        
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
