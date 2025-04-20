using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Windows.Forms;

namespace EmployeeSchedulingApp
{
    public static class RandomDataGenerator
    {
        private static Random random = new Random();
        private static string connectionString = "Data Source=(localdb)\\mssqllocaldb;Initial Catalog=EmployeeScheduling;Integrated Security=True";

        // English names for random generation
        private static string[] firstNames = { "John", "Michael", "David", "Robert", "James", "William", "Richard", "Thomas", "Joseph", "Daniel", "Matthew", "Anthony", "Mark", "Paul", "Steven", "Andrew", "Kenneth" };
        private static string[] lastNames = { "Smith", "Johnson", "Williams", "Jones", "Brown", "Davis", "Miller", "Wilson", "Moore", "Taylor", "Anderson", "Thomas", "Jackson", "White", "Harris", "Martin", "Thompson" };

        // Days of the week
        private static string[] daysOfWeek = { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };

        /// <summary>
        /// Main function to generate random data
        /// </summary>
        public static void GenerateRandomData(int branchCount, int totalEmployees, string username)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // 1. Find user ID
                    int userId = GetUserId(connection, username);
                    if (userId == -1)
                    {
                        MessageBox.Show($"User {username} not found in the system.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // Ask user for confirmation to delete existing data
                    DialogResult result = MessageBox.Show(
                        "This will delete all existing branches, employees, and shifts associated with your account. Are you sure you want to continue?",
                        "Warning",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);

                    if (result != DialogResult.Yes)
                    {
                        return;
                    }

                    // Delete existing data
                    DeleteExistingData(connection, userId);

                    // 2. Create shift types if they don't exist
                    EnsureShiftTypesExist(connection);

                    // 3. Ensure roles exist
                    EnsureRolesExist(connection);

                    // 4. Create branches
                    List<int> branchIds = CreateBranches(connection, branchCount, userId);

                    // 5. Create shifts for each branch
                    foreach (int branchId in branchIds)
                    {
                        CreateShiftsForBranch(connection, branchId);
                    }

                    // 6. Create employees for the entire network and assign them to branches
                    CreateEmployeesForNetwork(connection, totalEmployees, branchIds);

                    MessageBox.Show($"Successfully created {branchCount} branches with {totalEmployees} employees across the network.",
                        "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating random data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        //private static void ResetIdentityCounters(SqlConnection connection)
        //{
        //    try
        //    {
        //        // רשימת הטבלאות עם עמודות Identity שצריך לאפס
        //        string[] tablesToReset = {
        //    "Branches", "Employees",
        //    "Shifts", "ShiftTypes"

        //};

        //        foreach (string tableName in tablesToReset)
        //        {
        //            string resetQuery = $"DBCC CHECKIDENT ('{tableName}', RESEED, 0)";
        //            using (SqlCommand command = new SqlCommand(resetQuery, connection))
        //            {
        //                command.ExecuteNonQuery();
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"Error resetting identity counters: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //        throw;
        //    }
        //}
        #region Helper functions for deleting existing data

        public static void DeleteExistingData(SqlConnection connection, int userId)
        {
            try
            {
                // Get the list of branches for this user
                List<int> userBranchIds = new List<int>();
                string branchQuery = "SELECT BranchID FROM UserBranches WHERE UserID = @UserID";
                using (SqlCommand command = new SqlCommand(branchQuery, connection))
                {
                    command.Parameters.AddWithValue("@UserID", userId);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            userBranchIds.Add(reader.GetInt32(0));
                        }
                    }
                }

                // For each branch
                foreach (int branchId in userBranchIds)
                {
                    // Delete shift assignments and required roles
                    DeleteShiftsForBranch(connection, branchId);

                    // Get list of employees in the branch
                    List<int> branchEmployeeIds = new List<int>();
                    string employeeQuery = "SELECT EmployeeID FROM EmployeeBranches WHERE BranchID = @BranchID";
                    using (SqlCommand command = new SqlCommand(employeeQuery, connection))
                    {
                        command.Parameters.AddWithValue("@BranchID", branchId);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                branchEmployeeIds.Add(reader.GetInt32(0));
                            }
                        }
                    }

                    // Delete employees and their role assignments
                    foreach (int employeeId in branchEmployeeIds)
                    {
                        // Delete employee preferred shifts
                        string deletePrefShiftsQuery = "DELETE FROM EmployeePreferredShifts WHERE EmployeeID = @EmployeeID";
                        using (SqlCommand command = new SqlCommand(deletePrefShiftsQuery, connection))
                        {
                            command.Parameters.AddWithValue("@EmployeeID", employeeId);
                            command.ExecuteNonQuery();
                        }

                        // Delete employee roles
                        string deleteRolesQuery = "DELETE FROM EmployeeRoles WHERE EmployeeID = @EmployeeID";
                        using (SqlCommand command = new SqlCommand(deleteRolesQuery, connection))
                        {
                            command.Parameters.AddWithValue("@EmployeeID", employeeId);
                            command.ExecuteNonQuery();
                        }

                        // Delete employee-branch association
                        string deleteEmployeeBranchQuery = "DELETE FROM EmployeeBranches WHERE EmployeeID = @EmployeeID AND BranchID = @BranchID";
                        using (SqlCommand command = new SqlCommand(deleteEmployeeBranchQuery, connection))
                        {
                            command.Parameters.AddWithValue("@EmployeeID", employeeId);
                            command.Parameters.AddWithValue("@BranchID", branchId);
                            command.ExecuteNonQuery();
                        }

                        // Check if employee is associated with other branches
                        string checkOtherBranchesQuery = "SELECT COUNT(*) FROM EmployeeBranches WHERE EmployeeID = @EmployeeID";
                        using (SqlCommand command = new SqlCommand(checkOtherBranchesQuery, connection))
                        {
                            command.Parameters.AddWithValue("@EmployeeID", employeeId);
                            int count = (int)command.ExecuteScalar();

                            // If not associated with other branches, delete entirely
                            if (count == 0)
                            {
                                string deleteEmployeeQuery = "DELETE FROM Employees WHERE EmployeeID = @EmployeeID";
                                using (SqlCommand deleteCommand = new SqlCommand(deleteEmployeeQuery, connection))
                                {
                                    deleteCommand.Parameters.AddWithValue("@EmployeeID", employeeId);
                                    deleteCommand.ExecuteNonQuery();
                                }
                            }
                        }
                    }

                    // Delete user-branch association
                    string deleteUserBranchQuery = "DELETE FROM UserBranches WHERE BranchID = @BranchID AND UserID = @UserID";
                    using (SqlCommand command = new SqlCommand(deleteUserBranchQuery, connection))
                    {
                        command.Parameters.AddWithValue("@BranchID", branchId);
                        command.Parameters.AddWithValue("@UserID", userId);
                        command.ExecuteNonQuery();
                    }

                    // Delete branch itself
                    string deleteBranchQuery = "DELETE FROM Branches WHERE BranchID = @BranchID";
                    using (SqlCommand command = new SqlCommand(deleteBranchQuery, connection))
                    {
                        command.Parameters.AddWithValue("@BranchID", branchId);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting existing data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw; // Re-throw the exception so the calling function knows there was a problem
            }
        }

        private static void DeleteShiftsForBranch(SqlConnection connection, int branchId)
        {
            // Get all shifts for the branch
            List<int> shiftIds = new List<int>();
            string shiftsQuery = "SELECT ShiftID FROM Shifts WHERE BranchID = @BranchID";
            using (SqlCommand command = new SqlCommand(shiftsQuery, connection))
            {
                command.Parameters.AddWithValue("@BranchID", branchId);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        shiftIds.Add(reader.GetInt32(0));
                    }
                }
            }

            // For each shift, delete assignments and required roles
            foreach (int shiftId in shiftIds)
            {
                // Delete employee preferred shifts
                string deletePreferredQuery = "DELETE FROM EmployeePreferredShifts WHERE ShiftID = @ShiftID";
                using (SqlCommand command = new SqlCommand(deletePreferredQuery, connection))
                {
                    command.Parameters.AddWithValue("@ShiftID", shiftId);
                    command.ExecuteNonQuery();
                }

                // Delete shift assignments
                string deleteAssignmentsQuery = "DELETE FROM ShiftAssignments WHERE ShiftID = @ShiftID";
                using (SqlCommand command = new SqlCommand(deleteAssignmentsQuery, connection))
                {
                    command.Parameters.AddWithValue("@ShiftID", shiftId);
                    command.ExecuteNonQuery();
                }

                // Delete shift required roles
                string deleteRolesQuery = "DELETE FROM ShiftRequiredRoles WHERE ShiftID = @ShiftID";
                using (SqlCommand command = new SqlCommand(deleteRolesQuery, connection))
                {
                    command.Parameters.AddWithValue("@ShiftID", shiftId);
                    command.ExecuteNonQuery();
                }
            }

            // Delete all shifts for the branch
            string deleteShiftsQuery = "DELETE FROM Shifts WHERE BranchID = @BranchID";
            using (SqlCommand command = new SqlCommand(deleteShiftsQuery, connection))
            {
                command.Parameters.AddWithValue("@BranchID", branchId);
                command.ExecuteNonQuery();
            }
        }

        #endregion

        #region Helper functions for users and branches

        private static int GetUserId(SqlConnection connection, string username)
        {
            string query = "SELECT UserID FROM Users WHERE Username = @Username";
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Username", username);
                object result = command.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : -1;
            }
        }

        private static List<int> CreateBranches(SqlConnection connection, int count, int userId)
        {
            List<int> branchIds = new List<int>();

            // List of possible branch locations
            string[] locations = { "New York", "Chicago", "Los Angeles", "Boston", "Miami", "Seattle", "Denver", "Austin", "Atlanta", "Philadelphia" };

            for (int i = 0; i < count; i++)
            {
                string locationName = locations[random.Next(locations.Length)];
                string branchName = $"{locationName} Branch {random.Next(1, 100)}";

                string query = "INSERT INTO Branches (Name) VALUES (@Name); SELECT SCOPE_IDENTITY();";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Name", branchName);
                    object result = command.ExecuteScalar();

                    if (result != null)
                    {
                        int branchId = Convert.ToInt32(result);
                        branchIds.Add(branchId);

                        // Assign branch to user
                        string assignQuery = "INSERT INTO UserBranches (UserID, BranchID) VALUES (@UserID, @BranchID)";
                        using (SqlCommand assignCommand = new SqlCommand(assignQuery, connection))
                        {
                            assignCommand.Parameters.AddWithValue("@UserID", userId);
                            assignCommand.Parameters.AddWithValue("@BranchID", branchId);
                            assignCommand.ExecuteNonQuery();
                        }
                    }
                }
            }

            return branchIds;
        }

        #endregion

        #region Helper functions for shift types,timeSlots and roles

        private static void EnsureShiftTypesExist(SqlConnection connection)
        {
            // וידוא קיום TimeSlots
            string checkTimeSlotsQuery = "SELECT COUNT(*) FROM TimeSlots";
            using (SqlCommand checkCommand = new SqlCommand(checkTimeSlotsQuery, connection))
            {
                int count = Convert.ToInt32(checkCommand.ExecuteScalar());

                if (count == 0)
                {
                    // אם אין רשומות ב-TimeSlots, נוסיף את הבסיסיות
                    string[] timeSlots = { "Evening", "Morning" };

                    foreach (string timeSlot in timeSlots)
                    {
                        string insertQuery = "INSERT INTO TimeSlots (TimeSlotName) VALUES (@TimeSlotName)";
                        using (SqlCommand insertCommand = new SqlCommand(insertQuery, connection))
                        {
                            insertCommand.Parameters.AddWithValue("@TimeSlotName", timeSlot);
                            insertCommand.ExecuteNonQuery();
                        }
                    }
                }
            }

            // בדיקה וטיפול ב-ShiftTypes
            string checkShiftTypesQuery = "SELECT COUNT(*) FROM ShiftTypes";
            using (SqlCommand checkCommand = new SqlCommand(checkShiftTypesQuery, connection))
            {
                int count = Convert.ToInt32(checkCommand.ExecuteScalar());

                if (count == 0)
                {
                    // אם אין רשומות ב-ShiftTypes, נוסיף את הבסיסיות
                    string[] shiftTypes = { "Regular", "Holiday", "Special" };

                    foreach (string type in shiftTypes)
                    {
                        string insertQuery = "INSERT INTO ShiftTypes (TypeName) VALUES (@TypeName)";
                        using (SqlCommand insertCommand = new SqlCommand(insertQuery, connection))
                        {
                            insertCommand.Parameters.AddWithValue("@TypeName", type);
                            insertCommand.ExecuteNonQuery();
                        }
                    }
                }
            }
        }
        private static Dictionary<string, int> GetShiftTypeIds(SqlConnection connection)
        {
            Dictionary<string, int> shiftTypeIds = new Dictionary<string, int>();

            string query = "SELECT ShiftTypeID, TypeName FROM ShiftTypes";
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int id = reader.GetInt32(0);
                        string name = reader.GetString(1);
                        shiftTypeIds[name] = id;
                    }
                }
            }

            return shiftTypeIds;
        }

        private static void EnsureRolesExist(SqlConnection connection)
        {
            // Check if roles already exist
            string checkQuery = "SELECT COUNT(*) FROM Roles";
            using (SqlCommand checkCommand = new SqlCommand(checkQuery, connection))
            {
                int count = Convert.ToInt32(checkCommand.ExecuteScalar());

                if (count == 0)
                {
                    // If no roles exist, add basic roles
                    string[] roleNames = { "Waiter", "Chef", "Bartender", "Manager" };

                    foreach (string role in roleNames)
                    {
                        string insertQuery = "INSERT INTO Roles (RoleName) VALUES (@RoleName)";
                        using (SqlCommand insertCommand = new SqlCommand(insertQuery, connection))
                        {
                            insertCommand.Parameters.AddWithValue("@RoleName", role);
                            insertCommand.ExecuteNonQuery();
                        }
                    }
                }
            }
        }

        private static List<KeyValuePair<int, string>> GetRoles(SqlConnection connection)
        {
            List<KeyValuePair<int, string>> roles = new List<KeyValuePair<int, string>>();

            string query = "SELECT RoleID, RoleName FROM Roles";
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int id = reader.GetInt32(0);
                        string name = reader.GetString(1);
                        roles.Add(new KeyValuePair<int, string>(id, name));
                    }
                }
            }

            return roles;
        }
        private static Dictionary<string, int> GetTimeSlotIds(SqlConnection connection)
        {
            Dictionary<string, int> timeSlotIds = new Dictionary<string, int>();

            string query = "SELECT TimeSlotID, TimeSlotName FROM TimeSlots";
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int id = reader.GetInt32(0);
                        string name = reader.GetString(1);
                        timeSlotIds[name] = id;
                    }
                }
            }

            return timeSlotIds;
        }


        #endregion

        #region Helper functions for employees

        private static void CreateEmployeesForNetwork(SqlConnection connection, int count, List<int> allBranchIds)
        {
            // Get all roles
            List<KeyValuePair<int, string>> roles = GetRoles(connection);

            // Keep track of used names to avoid duplicates
            HashSet<string> usedNames = new HashSet<string>();

            // Get existing employee names to avoid conflicts
            string existingNamesQuery = "SELECT Name FROM Employees";
            using (SqlCommand command = new SqlCommand(existingNamesQuery, connection))
            {
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        usedNames.Add(reader.GetString(0));
                    }
                }
            }

            int employeesCreated = 0;
            int maxAttempts = count * 3; // Limit attempts to avoid infinite loop
            int attempts = 0;

            while (employeesCreated < count && attempts < maxAttempts)
            {
                attempts++;

                // Create random employee name
                string firstName = firstNames[random.Next(firstNames.Length)];
                string lastName = lastNames[random.Next(lastNames.Length)];
                string name = $"{firstName} {lastName}";

                // Skip if name already used
                if (usedNames.Contains(name))
                {
                    continue;
                }

                usedNames.Add(name);

                int hourlySalary = random.Next(30, 70);
                int rate = random.Next(1, 6);
                bool isMentor = random.Next(10) < 2; // 20% chance to be a mentor
                int assignedHours = random.Next(20, 41);

                string query = @"INSERT INTO Employees (Name, Phone, Email, HourlySalary, Rate, IsMentor, AssignedHours) 
                       VALUES (@Name, @Phone, @Email, @HourlySalary, @Rate, @IsMentor, @AssignedHours); 
                       SELECT SCOPE_IDENTITY();";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Name", name);
                    command.Parameters.AddWithValue("@Phone", $"{random.Next(100, 1000)}-{random.Next(100, 1000)}-{random.Next(1000, 10000)}");
                    command.Parameters.AddWithValue("@Email", $"{firstName.ToLower()}.{lastName.ToLower()}@example.com");
                    command.Parameters.AddWithValue("@HourlySalary", hourlySalary);
                    command.Parameters.AddWithValue("@Rate", rate);
                    command.Parameters.AddWithValue("@IsMentor", isMentor);
                    command.Parameters.AddWithValue("@AssignedHours", assignedHours);

                    object result = command.ExecuteScalar();

                    if (result != null)
                    {
                        employeesCreated++;
                        int employeeId = Convert.ToInt32(result);
                        string updatePassword = @"UPDATE Employees set Password=@Password where EmployeeID=@EmployeeID ";
                        using (SqlCommand command2 = new SqlCommand(updatePassword, connection))
                        {
                            command2.Parameters.AddWithValue("@Password", result.ToString());
                            command2.Parameters.AddWithValue("@EmployeeID", result);
                            command2.ExecuteScalar();
                        }
                            // Determine how many branches this employee will work at (1-3)
                            int branchCount = random.Next(1, Math.Min(4, allBranchIds.Count + 1));

                        // Shuffle branch IDs
                        var shuffledBranches = new List<int>(allBranchIds);
                        for (int j = 0; j < shuffledBranches.Count; j++)
                        {
                            int k = random.Next(j, shuffledBranches.Count);
                            int temp = shuffledBranches[j];
                            shuffledBranches[j] = shuffledBranches[k];
                            shuffledBranches[k] = temp;
                        }

                        // Select the first few branches
                        var selectedBranches = shuffledBranches.Take(branchCount).ToList();

                        // Assign employee to selected branches
                        foreach (int branchId in selectedBranches)
                        {
                            string branchAssignQuery = "INSERT INTO EmployeeBranches (EmployeeID, BranchID) VALUES (@EmployeeID, @BranchID)";
                            using (SqlCommand branchAssignCommand = new SqlCommand(branchAssignQuery, connection))
                            {
                                branchAssignCommand.Parameters.AddWithValue("@EmployeeID", employeeId);
                                branchAssignCommand.Parameters.AddWithValue("@BranchID", branchId);
                                branchAssignCommand.ExecuteNonQuery();
                            }

                            // Assign preferred shifts for the employee in this branch
                            AssignPreferredShiftsForEmployee(connection, employeeId, branchId);
                        }

                        // Assign 1-3 random roles to employee
                        int roleCount = random.Next(1, 4);
                        var shuffledRoles = new List<KeyValuePair<int, string>>(roles);

                        // Shuffle the roles list
                        for (int j = 0; j < shuffledRoles.Count; j++)
                        {
                            int k = random.Next(j, shuffledRoles.Count);
                            var temp = shuffledRoles[j];
                            shuffledRoles[j] = shuffledRoles[k];
                            shuffledRoles[k] = temp;
                        }

                        // Assign the first few roles from the shuffled list
                        for (int j = 0; j < Math.Min(roleCount, shuffledRoles.Count); j++)
                        {
                            string roleAssignQuery = "INSERT INTO EmployeeRoles (EmployeeID, RoleID) VALUES (@EmployeeID, @RoleID)";
                            using (SqlCommand roleAssignCommand = new SqlCommand(roleAssignQuery, connection))
                            {
                                roleAssignCommand.Parameters.AddWithValue("@EmployeeID", employeeId);
                                roleAssignCommand.Parameters.AddWithValue("@RoleID", shuffledRoles[j].Key);
                                roleAssignCommand.ExecuteNonQuery();
                            }
                        }
                    }
                }
            }

            // If we couldn't create enough employees due to name collisions
            if (employeesCreated < count)
            {
                MessageBox.Show($"Created only {employeesCreated} out of {count} employees due to name uniqueness constraints.",
                    "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        private static void AssignPreferredShiftsForEmployee(SqlConnection connection, int employeeId, int branchId)
        {
            // Get list of shifts for this branch
            List<int> branchShiftIds = new List<int>();
            string shiftsQuery = "SELECT ShiftID FROM Shifts WHERE BranchID = @BranchID";
            using (SqlCommand command = new SqlCommand(shiftsQuery, connection))
            {
                command.Parameters.AddWithValue("@BranchID", branchId);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        branchShiftIds.Add(reader.GetInt32(0));
                    }
                }
            }

            // If no shifts exist for this branch, return
            if (branchShiftIds.Count == 0)
                return;

            // Decide how many shifts to prefer (50-80% of available shifts)
            int preferredShiftsCount = random.Next(branchShiftIds.Count / 3, (branchShiftIds.Count * 6 / 10) + 1);

            // Shuffle the shifts list
            for (int i = 0; i < branchShiftIds.Count; i++)
            {
                int j = random.Next(i, branchShiftIds.Count);
                int temp = branchShiftIds[i];
                branchShiftIds[i] = branchShiftIds[j];
                branchShiftIds[j] = temp;
            }

            // Assign the preferred shifts
            for (int i = 0; i < Math.Min(preferredShiftsCount, branchShiftIds.Count); i++)
            {
                string insertQuery = "INSERT INTO EmployeePreferredShifts (EmployeeID, ShiftID) VALUES (@EmployeeID, @ShiftID)";
                using (SqlCommand command = new SqlCommand(insertQuery, connection))
                {
                    command.Parameters.AddWithValue("@EmployeeID", employeeId);
                    command.Parameters.AddWithValue("@ShiftID", branchShiftIds[i]);
                    command.ExecuteNonQuery();
                }
            }
        }

        #endregion

        #region Helper functions for shifts

        private static void CreateShiftsForBranch(SqlConnection connection, int branchId)
        {
            // Get TimeSlot IDs
            Dictionary<string, int> timeSlots = GetTimeSlotIds(connection);

            // Get ShiftType IDs
            Dictionary<string, int> shiftTypes = GetShiftTypeIds(connection);

            // Get roles
            List<KeyValuePair<int, string>> roles = GetRoles(connection);

            // עבור על כל יום בשבוע
            foreach (string day in daysOfWeek)
            {
                // עבור על כל סוג זמן (בוקר/ערב)
                foreach (var timeSlot in timeSlots)
                {
                    // 80% סיכוי ליצור משמרת


                    // בחירת סוג משמרת אקראי (רגילה/חג/מיוחדת)
                    string shiftTypeName = "Regular"; // ברירת מחדל
                    if (shiftTypes.Count > 0)
                    {
                        var shiftTypeNames = shiftTypes.Keys.ToList();
                        shiftTypeName = shiftTypeNames[random.Next(shiftTypeNames.Count)];
                    }

                    int shiftTypeId = shiftTypes.ContainsKey(shiftTypeName) ? shiftTypes[shiftTypeName] : 1;
                    bool isBusy = random.Next(2) == 0; // 50% סיכוי למשמרת עמוסה

                    // הוספת המשמרת לדאטאבייס

                    string insertShiftQuery = @"INSERT INTO Shifts (BranchID, TimeSlotID, DayOfWeek, ShiftTypeID, IsBusy) 
                         VALUES (@BranchID, @TimeSlotID, @DayOfWeek, @ShiftTypeID, @IsBusy);
                         SELECT SCOPE_IDENTITY();";


                    using (SqlCommand command = new SqlCommand(insertShiftQuery, connection))
                    {
                        command.Parameters.AddWithValue("@BranchID", branchId);
                        command.Parameters.AddWithValue("@ShiftTypeID", shiftTypeId);
                        command.Parameters.AddWithValue("@TimeSlotID", timeSlot.Value);
                        command.Parameters.AddWithValue("@DayOfWeek", day);
                        command.Parameters.AddWithValue("@IsBusy", isBusy);

                        object result = command.ExecuteScalar();

                        if (result != null)
                        {
                            int shiftId = Convert.ToInt32(result);

                            // הוספת דרישות התפקידים למשמרת
                            foreach (var role in roles)
                            {
                                // 70% סיכוי לדרוש את התפקיד הזה
                                if (random.Next(10) < 7)
                                {
                                    int requiredCount = random.Next(1, 4); // 1-3 עובדים נדרשים

                                    string insertRoleReqQuery = @"INSERT INTO ShiftRequiredRoles (ShiftID, RoleID, RequiredCount) 
                                                           VALUES (@ShiftID, @RoleID, @RequiredCount)";

                                    using (SqlCommand roleCommand = new SqlCommand(insertRoleReqQuery, connection))
                                    {
                                        roleCommand.Parameters.AddWithValue("@ShiftID", shiftId);
                                        roleCommand.Parameters.AddWithValue("@RoleID", role.Key);
                                        roleCommand.Parameters.AddWithValue("@RequiredCount", requiredCount);
                                        roleCommand.ExecuteNonQuery();
                                    }
                                }
                            }
                        }
                    }

                }
            }
        }
        #endregion
    }
}