namespace DataAccess.DataAccess
{
    public static class SqlQueries
    {
        public const string UserDbSchema = @"... (ваш існуючий код схеми, не чіпаємо) ...";
        
        // --- USERS ---
        public const string GetAllUsers = "SELECT * FROM Users;";
        // Додаємо ::uuid для ID у Users
        public const string CreateUser = "INSERT INTO Users (Id, Email, UserName, Password, Gender, Age, DateOfBirth, Country, PhoneNumber, UserRole) VALUES (@Id::uuid, @Email, @UserName, @Password, @Gender, @Age, @DateOfBirth, @Country, @PhoneNumber, @UserRole);";
        public const string GetUserById = "SELECT * FROM Users WHERE Id = @Id::uuid;";
        public const string UpdateUser = @"UPDATE Users SET Email = @Email, UserName = @UserName, Gender = @Gender, Age = @Age, DateOfBirth = @DateOfBirth, Country = @Country, PhoneNumber = @PhoneNumber, UserRole = @UserRole, ProfilePictureUrl = @ProfilePictureUrl WHERE Id = @Id::uuid;";
        public const string DeleteUser = "DELETE FROM Users WHERE Id = @Id::uuid;";
        public const string GetUserByEmail = "SELECT * FROM Users WHERE Email = @Email;";
        public const string UpdateUserHealthData = @"UPDATE Users SET Age = @Age, Gender = @Gender, DateOfBirth = @DateOfBirth, Country = @Country, PhoneNumber = @PhoneNumber WHERE Id = @Id::uuid;";

        // --- PASSWORD RESET ---
        public const string AddPasswordResetCode = "INSERT INTO PasswordResetCodes (Id, UserId, ResetCode, ExpiresAt, IsUsed) VALUES (@Id::uuid, @UserId::uuid, @ResetCode, @ExpiresAt, false);"; 
        public const string GetValidPasswordResetCode = "SELECT * FROM PasswordResetCodes WHERE UserId = @UserId::uuid AND ResetCode = @ResetCode AND ExpiresAt > @CurrentTime AND IsUsed = false;"; 
        public const string UpdatePasswordResetCode = "UPDATE PasswordResetCodes SET IsUsed = true WHERE Id = @Id::uuid;";
       
        // --- FILES ---
        public const string GetUserFilesByUserId = "SELECT * FROM UserFiles WHERE UserId = @UserId::uuid ORDER BY UploadedAt DESC;";
        public const string AddUserFile = "INSERT INTO UserFiles (Id, UserId, FileName, ContentType, FileData, UploadedAt, VisitId, NoteId) VALUES (@Id::uuid, @UserId::uuid, @FileName, @ContentType, @FileData, @UploadedAt, @VisitId::uuid, @NoteId::uuid);";
        public const string GetUserFile = "SELECT * FROM UserFiles WHERE Id = @Id::uuid;";
        public const string DeleteUserFile = "DELETE FROM UserFiles WHERE Id = @Id::uuid;";
        
        // --- NOTES ---
        public const string GetUserNotes = "SELECT * FROM UserNotes WHERE UserId = @UserId::uuid ORDER BY CreatedAt DESC;";
        public const string AddUserNote = @"INSERT INTO UserNotes (Id, UserId, CreatedAt, NoteText, NoteTitle) VALUES (@Id::uuid, @UserId::uuid, @CreatedAt, @NoteText, @NoteTitle);";
        public const string DeleteUserNote = "DELETE FROM UserNotes WHERE Id = @Id::uuid AND UserId = @UserId::uuid;";
        
        // --- DOCTOR VISITS ---
        public const string AddDoctorVisit = "INSERT INTO DoctorVisits (Id, UserId, Specialist, VisitType, Diagnosis, Prescription, VisitedAt) VALUES (@Id::uuid, @UserId::uuid, @Specialist, @VisitType, @Diagnosis, @Prescription, @VisitedAt);";
        public const string GetDoctorVisits = "SELECT * FROM DoctorVisits WHERE UserId = @UserId::uuid ORDER BY VisitedAt DESC;";
        
        // --- HEALTH MEASUREMENTS ---
        public const string AddHealthMeasurement = "INSERT INTO HealthMeasurements (Id, UserId, MeasuredAt, Systolic, Diastolic, HeartRate) VALUES (@Id::uuid, @UserId::uuid, @MeasuredAt, @Systolic, @Diastolic, @HeartRate);";
        public const string GetMeasurements = "SELECT * FROM HealthMeasurements WHERE UserId = @UserId::uuid ORDER BY MeasuredAt DESC;";
        
        // --- ANTHROPOMETRY ---
        public const string AddAnthropometry = @"INSERT INTO HealthAnthropometry (Id, UserId, MeasuredAt, Weight, Height, Sugar, BloodType, Age) VALUES (@Id::uuid, @UserId::uuid, @MeasuredAt, @Weight, @Height, @Sugar, @BloodType, @Age);";
        public const string GetAnthropometries = @"SELECT * FROM HealthAnthropometry WHERE UserId = @UserId::uuid ORDER BY MeasuredAt DESC;";
       
        // --- MEDICATIONS (ТУТ БУЛА ПОМИЛКА ПРИ ОТРИМАННІ/ОНОВЛЕННІ) ---
        public const string AddMedication = @"
            INSERT INTO Medications 
            (Id, UserId, NameOfMedication, Dose, TimesJson, WeekDaysJson, Type, Duration, StartDate, EndDate, CreatedAt, UpdatedAt) 
            VALUES 
            (@Id::uuid, @UserId::uuid, @NameOfMedication, @Dose, @TimesJson, @WeekDaysJson, @Type, @Duration, @StartDate, @EndDate, @CreatedAt, @UpdatedAt);";
        
        // 👇 Додано ::uuid до UserId
        public const string GetMedications = @"SELECT * FROM Medications WHERE UserId = @UserId::uuid ORDER BY CreatedAt DESC;";
        
        // 👇 Додано ::uuid до Id та UserId
        public const string UpdateMedication = @"UPDATE Medications SET NameOfMedication = @NameOfMedication, Dose = @Dose, TimesJson = @TimesJson, WeekDaysJson = @WeekDaysJson, Type = @Type, Duration = @Duration, StartDate = @StartDate, EndDate = @EndDate, UpdatedAt = @UpdatedAt WHERE Id = @Id::uuid AND UserId = @UserId::uuid;";
        
        // 👇 Додано ::uuid до Id
        public const string DeleteMedication = "DELETE FROM Medications WHERE Id = @Id::uuid;";
       
        // --- SLEEP ---
        public const string AddSleep = @"INSERT INTO Sleep (Id, UserId, Date, StartTime, EndTime, TotalDurationMinutes, SleepScore, SleepStatus) VALUES (@Id::uuid, @UserId::uuid, @Date, @StartTime, @EndTime, @TotalDurationMinutes, @SleepScore, @SleepStatus);";
        public const string GetSleepByUserId = @"SELECT * FROM Sleep WHERE UserId = @UserId::uuid ORDER BY Date DESC;";
    }
}