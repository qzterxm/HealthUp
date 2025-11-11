namespace DataAccess.DataAccess
{
    public static class SqlQueries
    {
        #region Users
        public const string GetAllUsers = "SELECT * FROM Users;";
        public const string CreateUser = "INSERT INTO Users (Id, Email, UserName, Password, Gender, Age, DateOfBirth, Country, PhoneNumber, UserRole) VALUES (@Id, @Email, @UserName, @Password, @Gender, @Age, @DateOfBirth, @Country, @PhoneNumber, @UserRole);";
        public const string GetUserById = "SELECT * FROM Users WHERE Id = @Id;";
        public const string UpdateUser = @"UPDATE Users SET Email = @Email, UserName = @UserName, Password = @Password, Gender = @Gender, Age = @Age, DateOfBirth = @DateOfBirth, Country = @Country,  PhoneNumber = @PhoneNumber,UserRole = @UserRole WHERE Id = @Id;";
        public const string DeleteUser = "DELETE FROM Users WHERE Id = @Id;";
        public const string GetUserByEmail = "SELECT * FROM Users WHERE Email = @Email;";
        public const string AddPasswordResetCode = "INSERT INTO PasswordResetCodes (Id, UserId, ResetCode, ExpiresAt, IsUsed) VALUES (@Id, @UserId, @ResetCode, @ExpiresAt, 0);";
        public const string GetValidPasswordResetCode = "SELECT * FROM PasswordResetCodes WHERE UserId = @UserId AND ResetCode = @ResetCode AND ExpiresAt > @CurrentTime AND IsUsed = 0;"; 
        public const string UpdatePasswordResetCode = "UPDATE PasswordResetCodes SET IsUsed = 1 WHERE Id = @Id;";
        public const string AddUserFile = "INSERT INTO UserFiles (Id, FileName, ContentType, FileData, UploadedAt, UserId, VisitId) VALUES (@Id, @FileName, @ContentType, @FileData, @UploadedAt, @UserId, @VisitId);";
        public const string GetUserFile = "SELECT * FROM UserFiles WHERE Id = @Id;";
        public const string DeleteUserFile = "DELETE FROM UserFiles WHERE Id = @Id;";
        public const string GetUserNotes = "SELECT * FROM UserNotes WHERE UserId = @UserId ORDER BY CreatedAt DESC;";
        public const string AddUserNote = "INSERT INTO UserNotes (Id, UserId, CreatedAt, NoteText) VALUES (@Id, @UserId, @CreatedAt, @NoteText);";
        public const string DeleteUserNote = "DELETE FROM UserNotes WHERE Id = @Id AND UserId = @UserId;";
        public const string AddDoctorVisit = "INSERT INTO DoctorVisits (Id, UserId, Specialist, VisitType, Diagnosis, Prescription, VisitedAt) VALUES (@Id, @UserId, @Specialist, @VisitType, @Diagnosis, @Prescription, @VisitedAt);";
        public const string GetDoctorVisits = "SELECT * FROM DoctorVisits WHERE UserId = @UserId ORDER BY VisitedAt DESC;";
        public const string UpdateUserHealthData = @" UPDATE Users  SET Age = @Age, Gender = @Gender, DateOfBirth = @DateOfBirth, Country = @Country, PhoneNumber = @PhoneNumber WHERE Id = @Id;";
        #endregion

        #region Health
        public const string AddHealthMeasurement = "INSERT INTO HealthMeasurements (Id, UserId, MeasuredAt, Systolic, Diastolic, HeartRate) VALUES (@Id, @UserId, @MeasuredAt, @Systolic, @Diastolic, @HeartRate);";
        public const string GetMeasurements = "SELECT * FROM HealthMeasurements WHERE UserId = @UserId ORDER BY MeasuredAt DESC;";
        public const string AddAnthropometry = "INSERT INTO HealthAnthropometry (Id, UserId, MeasuredAt, Weight, Height, Sugar, BloodType) VALUES (@Id, @UserId, @MeasuredAt, @Weight, @Height, @Sugar, @BloodType);";
        public const string GetAnthropometries = "SELECT * FROM HealthAnthropometry WHERE UserId = @UserId ORDER BY MeasuredAt DESC;";
        #endregion
    }
}