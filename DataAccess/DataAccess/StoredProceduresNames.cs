namespace DataAccess.DataAccess;

    public static class StoredProceduresNames
    {
        #region Users
        public const string GetAllUsers = "sp_Users_GetAllUsers";
        public const string CreateUser = "sp_Users_CreateUser";
        public const string GetUserById = "sp_Users_GetUserById";
        public const string UpdateUser = "sp_Users_UpdateUser";
        public const string DeleteUser = "sp_Users_DeleteUser";
        public const string GetUserByEmail = "sp_Users_GetUserByEmail";
        #endregion

        #region Health
        public const string AddHealthMeasurement = "sp_Health_AddMeasurement";
        public const string GetMeasurements = "sp_Health_GetMeasurements";
        public const string GetAnthropometries = "sp_Health_GetAnthropometries";
        #endregion
        
    }

