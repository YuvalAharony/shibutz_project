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
        // מופע של מחלקת העזר לבסיס הנתונים
        private static DataBaseHelper helper = new DataBaseHelper();

        // שמות באנגלית לייצור אקראי
        private static string[] firstNames = { "John", "Michael", "David", "Robert", "James", "William", "Richard", "Thomas", "Joseph", "Daniel", "Matthew", "Anthony", "Mark", "Paul", "Steven", "Andrew", "Kenneth" };
        private static string[] lastNames = { "Smith", "Johnson", "Williams", "Jones", "Brown", "Davis", "Miller", "Wilson", "Moore", "Taylor", "Anderson", "Thomas", "Jackson", "White", "Harris", "Martin", "Thompson" };

        // ימי השבוע
        private static string[] daysOfWeek = { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };

        // פונקציה ראשית לייצור נתונים אקראיים
        // פרמטרים
        // branchCount - מספר הסניפים ליצירה
        // totalEmployees - מספר העובדים הכולל ליצירה
        // username - שם המשתמש שעבורו יווצרו הנתונים
        // ערך מוחזר: אין
        public static void GenerateRandomData(int branchCount, int totalEmployees, string username)
        {
           
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // מציאת מזהה המשתמש במערכת
                    int userId = GetUserId(connection, username);
                    if (userId == -1)
                    {
                        MessageBox.Show($"המשתמש {username} לא נמצא במערכת.", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // בקשת אישור מהמשתמש למחיקת נתונים קיימים
                    DialogResult result = MessageBox.Show(
                    "פעולה זו תמחק את כל הסניפים, העובדים והמשמרות הקשורים לחשבון שלך. האם אתה בטוח שברצונך להמשיך?",
                    "אזהרה",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);
                    if (result != DialogResult.Yes)
                    {
                        return;
                    }

                    // מחיקת נתונים קיימים במסד הנתונים
                    DeleteExistingData(connection, userId);

                    //// יצירת סוגי משמרות אם לא קיימים
                    //EnsureShiftTypesExist(connection);

                    //// וידוא קיום תפקידים במערכת
                    //EnsureRolesExist(connection);

                    // יצירת סניפים חדשים
                    List<int> branchIds = CreateBranches(connection, branchCount, userId);

                    // יצירת משמרות לכל סניף
                    foreach (int branchId in branchIds)
                    {
                        CreateShiftsForBranch(connection, branchId);
                    }

                    // יצירת עובדים לכל הרשת והקצאתם לסניפים
                    CreateEmployeesForNetwork(connection, totalEmployees, branchIds);

                    MessageBox.Show($"נוצרו בהצלחה {branchCount} סניפים עם {totalEmployees} עובדים ברחבי הרשת.",
                    "הצלחה", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
         
        }

        // פונקציה למחיקת נתונים קיימים
        // פרמטרים
        // connection - חיבור פתוח למסד הנתונים
        // userId - מזהה המשתמש שהנתונים שלו יימחקו
        // ערך מוחזר: אין
        public static void DeleteExistingData(SqlConnection connection, int userId)
        {
            try
            {
                // קבלת רשימת הסניפים של המשתמש
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

                // עבור כל סניף
                foreach (int branchId in userBranchIds)
                {
                    // מחיקת הקצאות משמרות ותפקידים נדרשים
                    DeleteShiftsForBranch(connection, branchId);

                    // קבלת רשימת העובדים בסניף
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

                    // מחיקת עובדים והקצאות תפקידים שלהם
                    foreach (int employeeId in branchEmployeeIds)
                    {
                        // מחיקת משמרות מועדפות של העובד
                        string deletePrefShiftsQuery = "DELETE FROM EmployeePreferredShifts WHERE EmployeeID = @EmployeeID";
                        using (SqlCommand command = new SqlCommand(deletePrefShiftsQuery, connection))
                        {
                            command.Parameters.AddWithValue("@EmployeeID", employeeId);
                            command.ExecuteNonQuery();
                        }

                        // מחיקת תפקידי העובד
                        string deleteRolesQuery = "DELETE FROM EmployeeRoles WHERE EmployeeID = @EmployeeID";
                        using (SqlCommand command = new SqlCommand(deleteRolesQuery, connection))
                        {
                            command.Parameters.AddWithValue("@EmployeeID", employeeId);
                            command.ExecuteNonQuery();
                        }

                        // מחיקת השיוך בין העובד לסניף
                        string deleteEmployeeBranchQuery = "DELETE FROM EmployeeBranches WHERE EmployeeID = @EmployeeID AND BranchID = @BranchID";
                        using (SqlCommand command = new SqlCommand(deleteEmployeeBranchQuery, connection))
                        {
                            command.Parameters.AddWithValue("@EmployeeID", employeeId);
                            command.Parameters.AddWithValue("@BranchID", branchId);
                            command.ExecuteNonQuery();
                        }

                        // בדיקה אם העובד משויך לסניפים אחרים
                        string checkOtherBranchesQuery = "SELECT COUNT(*) FROM EmployeeBranches WHERE EmployeeID = @EmployeeID";
                        using (SqlCommand command = new SqlCommand(checkOtherBranchesQuery, connection))
                        {
                            command.Parameters.AddWithValue("@EmployeeID", employeeId);
                            int count = (int)command.ExecuteScalar();

                            // אם לא משויך לסניפים אחרים, מחיקה מוחלטת
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

                    // מחיקת השיוך בין המשתמש לסניף
                    string deleteUserBranchQuery = "DELETE FROM UserBranches WHERE BranchID = @BranchID AND UserID = @UserID";
                    using (SqlCommand command = new SqlCommand(deleteUserBranchQuery, connection))
                    {
                        command.Parameters.AddWithValue("@BranchID", branchId);
                        command.Parameters.AddWithValue("@UserID", userId);
                        command.ExecuteNonQuery();
                    }

                    // מחיקת הסניף עצמו
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
                MessageBox.Show($"שגיאה במחיקת נתונים קיימים: {ex.Message}", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw; // העברת החריגה הלאה כדי שהפונקציה הקוראת תדע שהייתה בעיה
            }
        }

        // פונקציה למחיקת משמרות מסניף
        // פרמטרים:
        // connection - חיבור פתוח למסד הנתונים
        // branchId - מזהה הסניף שממנו יש למחוק משמרות
        // ערך מוחזר: אין
        private static void DeleteShiftsForBranch(SqlConnection connection, int branchId)
        {
            // קבלת כל המשמרות בסניף
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

            // עבור כל משמרת, מחיקת הקצאות ותפקידים נדרשים
            foreach (int shiftId in shiftIds)
            {
                // מחיקת משמרות מועדפות של עובדים
                string deletePreferredQuery = "DELETE FROM EmployeePreferredShifts WHERE ShiftID = @ShiftID";
                using (SqlCommand command = new SqlCommand(deletePreferredQuery, connection))
                {
                    command.Parameters.AddWithValue("@ShiftID", shiftId);
                    command.ExecuteNonQuery();
                }

                // מחיקת תפקידים נדרשים למשמרת
                string deleteRolesQuery = "DELETE FROM ShiftRequiredRoles WHERE ShiftID = @ShiftID";
                using (SqlCommand command = new SqlCommand(deleteRolesQuery, connection))
                {
                    command.Parameters.AddWithValue("@ShiftID", shiftId);
                    command.ExecuteNonQuery();
                }
            }

            // מחיקת כל המשמרות בסניף
            string deleteShiftsQuery = "DELETE FROM Shifts WHERE BranchID = @BranchID";
            using (SqlCommand command = new SqlCommand(deleteShiftsQuery, connection))
            {
                command.Parameters.AddWithValue("@BranchID", branchId);
                command.ExecuteNonQuery();
            }
        }

        // פונקציה לקבלת מזהה משתמש לפי שם משתמש
        // פרמטרים:
        // connection - חיבור פתוח למסד הנתונים
        // username - שם המשתמש לחיפוש
        // ערך מוחזר: מזהה המשתמש או -1 אם לא נמצא
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

        // פונקציה ליצירת סניפים חדשים
        // פרמטרים:
        // connection - חיבור פתוח למסד הנתונים
        // count - מספר הסניפים ליצירה
        // userId - מזהה המשתמש שיהיה בעל הסניפים
        // ערך מוחזר: רשימת מזהי הסניפים שנוצרו
        private static List<int> CreateBranches(SqlConnection connection, int count, int userId)
        {
            List<int> branchIds = new List<int>();

            // רשימת מיקומים אפשריים לסניפים
            string[] locations = { "תל אביב", "חיפה", "ירושלים", "באר שבע", "נתניה", "פתח תקווה", "אשדוד", "ראשון לציון", "רמת גן", "בני ברק" };

            for (int i = 0; i < count; i++)
            {
                string locationName = locations[random.Next(locations.Length)];
                string branchName = $"סניף {locationName} {random.Next(1, 100)}";

                string query = "INSERT INTO Branches (Name) VALUES (@Name); SELECT SCOPE_IDENTITY();";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Name", branchName);
                    object result = command.ExecuteScalar();

                    if (result != null)
                    {
                        int branchId = Convert.ToInt32(result);
                        branchIds.Add(branchId);

                        // שיוך הסניף למשתמש
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

        // פונקציה לוידוא קיום סוגי משמרות במערכת
        // פרמטרים:
        // connection - חיבור פתוח למסד הנתונים
        // ערך מוחזר: אין
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

        // פונקציה לקבלת מזהי סוגי משמרות
        // פרמטרים:
        // connection - חיבור פתוח למסד הנתונים
        // ערך מוחזר: מילון המכיל את שמות סוגי המשמרות ואת המזהים שלהם
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

        // פונקציה לוידוא קיום תפקידים במערכת
        // פרמטרים:
        // connection - חיבור פתוח למסד הנתונים
        // ערך מוחזר: אין
        private static void EnsureRolesExist(SqlConnection connection)
        {
            // בדיקה אם תפקידים כבר קיימים
            string checkQuery = "SELECT COUNT(*) FROM Roles";
            using (SqlCommand checkCommand = new SqlCommand(checkQuery, connection))
            {
                int count = Convert.ToInt32(checkCommand.ExecuteScalar());

                if (count == 0)
                {
                    // אם אין תפקידים, נוסיף תפקידים בסיסיים
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

        // פונקציה לקבלת רשימת התפקידים במערכת
        // פרמטרים:
        // connection - חיבור פתוח למסד הנתונים
        // ערך מוחזר: רשימת זוגות של מזהה תפקיד ושם תפקיד
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

        // פונקציה לקבלת מזהי פרקי זמן
        // פרמטרים:
        // connection - חיבור פתוח למסד הנתונים
        // ערך מוחזר: מילון המכיל את שמות פרקי הזמן ואת המזהים שלהם
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

        // פונקציה ליצירת עובדים לכל הרשת
        // פרמטרים:
        // connection - חיבור פתוח למסד הנתונים
        // count - מספר העובדים ליצירה
        // allBranchIds - רשימת מזהי הסניפים להקצאת העובדים
        // ערך מוחזר: אין
        private static void CreateEmployeesForNetwork(SqlConnection connection, int count, List<int> allBranchIds)
        {
            // קבלת כל התפקידים
            List<KeyValuePair<int, string>> roles = GetRoles(connection);

            // מעקב אחר שמות שכבר בשימוש למניעת כפילויות
            HashSet<string> usedNames = new HashSet<string>();

            // קבלת שמות עובדים קיימים למניעת התנגשויות
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
            int maxAttempts = count * 3; // הגבלת ניסיונות למניעת לולאה אינסופית
            int attempts = 0;

            while (employeesCreated < count && attempts < maxAttempts)
            {
                attempts++;

                // יצירת שם עובד אקראי
                string firstName = firstNames[random.Next(firstNames.Length)];
                string lastName = lastNames[random.Next(lastNames.Length)];
                string name = $"{firstName} {lastName}";

                // דילוג אם השם כבר בשימוש
                if (!usedNames.Contains(name))
                {



                    usedNames.Add(name);

                    int hourlySalary = random.Next(30, 70);
                    int rate = random.Next(1, 11);
                    bool isMentor = random.Next(10) < 2; // 20% סיכוי להיות מנטור
                    int assignedHours = random.Next(20, 41);

                    string query = @"INSERT INTO Employees (Name, Phone, Email, HourlySalary, Rate, IsMentor) 
                       VALUES (@Name, @Phone, @Email, @HourlySalary, @Rate, @IsMentor); 
                       SELECT SCOPE_IDENTITY();";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Name", name);
                        command.Parameters.AddWithValue("@Phone", $"{random.Next(100, 1000)}-{random.Next(100, 1000)}-{random.Next(1000, 10000)}");
                        command.Parameters.AddWithValue("@Email", $"{firstName.ToLower()}.{lastName.ToLower()}@example.com");
                        command.Parameters.AddWithValue("@HourlySalary", hourlySalary);
                        command.Parameters.AddWithValue("@Rate", rate);
                        command.Parameters.AddWithValue("@IsMentor", isMentor);

                        object result = command.ExecuteScalar();

                        if (result != null)
                        {
                            employeesCreated++;
                            int employeeId = Convert.ToInt32(result);

                            // עדכון סיסמה זמנית לעובד
                            string updatePassword = @"UPDATE Employees set Password=@Password where EmployeeID=@EmployeeID ";
                            using (SqlCommand command2 = new SqlCommand(updatePassword, connection))
                            {
                                command2.Parameters.AddWithValue("@Password", result.ToString());
                                command2.Parameters.AddWithValue("@EmployeeID", result);
                                command2.ExecuteScalar();
                            }

                            // קביעת מספר הסניפים שהעובד יעבוד בהם (1-3)
                            int branchCount = random.Next(1, Math.Min(4, allBranchIds.Count + 1));

                            // ערבוב מזהי הסניפים
                            var shuffledBranches = new List<int>(allBranchIds);
                            for (int j = 0; j < shuffledBranches.Count; j++)
                            {
                                int k = random.Next(j, shuffledBranches.Count);
                                int temp = shuffledBranches[j];
                                shuffledBranches[j] = shuffledBranches[k];
                                shuffledBranches[k] = temp;
                            }

                            // בחירת הסניפים הראשונים
                            var selectedBranches = shuffledBranches.Take(branchCount).ToList();

                            // הקצאת העובד לסניפים שנבחרו
                            foreach (int branchId in selectedBranches)
                            {
                                string branchAssignQuery = "INSERT INTO EmployeeBranches (EmployeeID, BranchID) VALUES (@EmployeeID, @BranchID)";
                                using (SqlCommand branchAssignCommand = new SqlCommand(branchAssignQuery, connection))
                                {
                                    branchAssignCommand.Parameters.AddWithValue("@EmployeeID", employeeId);
                                    branchAssignCommand.Parameters.AddWithValue("@BranchID", branchId);
                                    branchAssignCommand.ExecuteNonQuery();
                                }

                                // הקצאת משמרות מועדפות לעובד בסניף זה
                                AssignPreferredShiftsForEmployee(connection, employeeId, branchId);
                            }

                            // הקצאת 1-3 תפקידים אקראיים לעובד
                            int roleCount = random.Next(1, 4);
                            var shuffledRoles = new List<KeyValuePair<int, string>>(roles);

                            // ערבוב רשימת התפקידים
                            for (int j = 0; j < shuffledRoles.Count; j++)
                            {
                                int k = random.Next(j, shuffledRoles.Count);
                                var temp = shuffledRoles[j];
                                shuffledRoles[j] = shuffledRoles[k];
                                shuffledRoles[k] = temp;
                            }

                            // הקצאת התפקידים הראשונים מהרשימה המעורבבת
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
            }

            // אם לא הצלחנו ליצור מספיק עובדים בשל התנגשויות שמות
            if (employeesCreated < count)
            {
                MessageBox.Show($"נוצרו רק {employeesCreated} מתוך {count} עובדים בשל מגבלות ייחוד שמות.",
                "אזהרה", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // פונקציה להקצאת משמרות מועדפות לעובד
        // פרמטרים:
        // connection - חיבור פתוח למסד הנתונים
        // employeeId - מזהה העובד
        // branchId - מזהה הסניף
        // ערך מוחזר: אין
        private static void AssignPreferredShiftsForEmployee(SqlConnection connection, int employeeId, int branchId)
        {
            // קבלת רשימת משמרות בסניף זה
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

            // אם אין משמרות בסניף זה, סיום הפונקציה
            if (branchShiftIds.Count == 0)
                return;

            // החלטה על כמה משמרות מועדפות להקצות (50-80% מהמשמרות הזמינות)
            int preferredShiftsCount = random.Next(branchShiftIds.Count / 3, (branchShiftIds.Count * 8 / 10) + 1);

            // ערבוב רשימת המשמרות
            for (int i = 0; i < branchShiftIds.Count; i++)
            {
                int j = random.Next(i, branchShiftIds.Count);
                int temp = branchShiftIds[i];
                branchShiftIds[i] = branchShiftIds[j];
                branchShiftIds[j] = temp;
            }

            // הקצאת המשמרות המועדפות
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

        // פונקציה ליצירת משמרות לסניף
        // פרמטרים:
        // connection - חיבור פתוח למסד הנתונים
        // branchId - מזהה הסניף ליצירת המשמרות
        // ערך מוחזר: אין
        private static void CreateShiftsForBranch(SqlConnection connection, int branchId)
        {
            // קבלת מזהי פרקי זמן
            Dictionary<string, int> timeSlots = GetTimeSlotIds(connection);

            // קבלת מזהי סוגי משמרות
            Dictionary<string, int> shiftTypes = GetShiftTypeIds(connection);

            // קבלת תפקידים
            List<KeyValuePair<int, string>> roles = GetRoles(connection);

            // עבור כל יום בשבוע
            foreach (string day in daysOfWeek)
            {
                // עבור כל סוג זמן (בוקר/ערב)
                foreach (var timeSlot in timeSlots)
                {
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
    }
}