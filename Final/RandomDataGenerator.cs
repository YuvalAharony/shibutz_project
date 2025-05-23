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
            //יצירת חיבור לבסיס הנתונים
            using (SqlConnection connection = new SqlConnection(connectionString))
                {
                //פתיחת החיבור לבסיס הנתונים
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

                    // יצירת סניפים חדשים
                    List<int> branchIds = CreateBranches(connection, branchCount, userId);

                    // יצירת משמרות לכל סניף
                    foreach (int branchId in branchIds)
                    {
                        CreateShiftsForBranch(connection, branchId);
                    }

                    // יצירת עובדים לכל הרשת 
                    CreateEmployeesForNetwork(connection, totalEmployees, branchIds);

                    MessageBox.Show($"נוצרו בהצלחה {branchCount} סניפים עם {totalEmployees} עובדים ברחבי הרשת.",
                    "הצלחה", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

        }
        
        //פונקציה למחיקת נתונים קיימים
        // פרמטרים
        // connection - חיבור פתוח למסד הנתונים
        // userId - מזהה המשתמש שהנתונים שלו יימחקו
        // ערך מוחזר: אין
        public static void DeleteExistingData(SqlConnection connection, int userId)
        {
            try
            {
                // שאיללתה למחיקת כל הסניפים של המשתמש
                string deleteBranchesQuery = @"
                DELETE FROM Branches 
                WHERE BranchID IN (
                SELECT BranchID FROM UserBranches WHERE UserID = @UserID
                 )";

                using (SqlCommand command = new SqlCommand(deleteBranchesQuery, connection))
                {
                    //הוספת פרמטר והרצת השאילתה
                    command.Parameters.AddWithValue("@UserID", userId);
                    command.ExecuteNonQuery();
                }

                // שאילתה למחיקת כל העובדים של המשתמש
                //עובדים שלא משויכים לאף סניף- זה ימחוק את כל העובדים של המשתמש כיוון שכל הסניפים שלו כבר נמחקו
                string deleteOrphanEmployeesQuery = @"
                DELETE FROM Employees 
                WHERE EmployeeID NOT IN (
                SELECT DISTINCT EmployeeID FROM EmployeeBranches
            )";

                using (SqlCommand command = new SqlCommand(deleteOrphanEmployeesQuery, connection))
                {
                    //הרצת השאילתה
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"שגיאה במחיקת נתונים: {ex.Message}", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }
        }

        // פונקציה לקבלת מזהה משתמש לפי שם משתמש
        // פרמטרים:
        // connection - חיבור פתוח למסד הנתונים
        // username - שם המשתמש לחיפוש
        // ערך מוחזר: מזהה המשתמש או -1 אם לא נמצא
        private static int GetUserId(SqlConnection connection, string username)
        {
            //שאילתה לקבלת מזהה המשתמש לפי שם משתמש
            string query = "SELECT UserID FROM Users WHERE Username = @Username";
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                //הוספת פרמטר והרצת השאילתה
                command.Parameters.AddWithValue("@Username", username);
                object result = command.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : -1;
            }
        }

        // פונקציה ליצירת סניפים חדשים
        // פרמטרים
        // connection - חיבור פתוח למסד הנתונים
        // count - מספר הסניפים ליצירה
        // userId - מזהה המשתמש שיהיה בעל הסניפים
        // ערך מוחזר: רשימת מזהי הסניפים שנוצרו
        private static List<int> CreateBranches(SqlConnection connection, int count, int userId)
        {
            List<int> branchIds = new List<int>();

            // רשימת מיקומים אפשריים לסניפים
            string[] locations = { "New York", "Los Angeles", "Chicago", "Houston", "Phoenix", "Philadelphia", "San Antonio", "San Diego", "Dallas", "San Jose" };
            for (int i = 0; i < count; i++)
            {
                //הגרלת שם הסניף
                string locationName = locations[random.Next(locations.Length)];
                string branchName = $"Branch {locationName} {random.Next(1, 100)}";
                //שאילתה להכנסת הסניף לדאטא בייס
                string query = "INSERT INTO Branches (Name) VALUES (@Name); SELECT SCOPE_IDENTITY();";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    //הוספת פרמטר והרצת השאילתה
                    command.Parameters.AddWithValue("@Name", branchName);
                    object result = command.ExecuteScalar();

                    if (result != null)
                    {
                        int branchId = Convert.ToInt32(result);
                        branchIds.Add(branchId);

                        // שאילתה לשיוך הסניף למשתמש
                        string assignQuery = "INSERT INTO UserBranches (UserID, BranchID) VALUES (@UserID, @BranchID)";
                        using (SqlCommand assignCommand = new SqlCommand(assignQuery, connection))
                        {
                            //הוספת פרמטרים והרצת השאילתה
                            assignCommand.Parameters.AddWithValue("@UserID", userId);
                            assignCommand.Parameters.AddWithValue("@BranchID", branchId);
                            assignCommand.ExecuteNonQuery();
                        }
                    }
                }
            }

            return branchIds;
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
            List<string> roles = helper.getRoles();

            // מעקב אחר שמות שכבר בשימוש למניעת כפילויות
            HashSet<string> usedNames = new HashSet<string>();

            // שאילתה לקבלת שמות עובדים קיימים למניעת התנגשויות
            string existingNamesQuery = "SELECT Name FROM Employees";
            using (SqlCommand command = new SqlCommand(existingNamesQuery, connection))
            {
                //הרצת השאילתה והוספת השמות לרשימה
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        usedNames.Add(reader.GetString(0));
                    }
                }
            }
            //איפוס מונה עובדים
            int employeesCreated = 0;
            // הגבלת ניסיונות למניעת לולאה אינסופית
            int maxAttempts = count * 3; 
            //איפוס מונה נסיונות
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
                    //הוספתה לרשימת השמות שכבר יש 
                    usedNames.Add(name);
                    //הגרלת נתונים לעובד
                    int hourlySalary = random.Next(30, 70); //משכורות שעתית
                    int rate = random.Next(1, 11);//ציון עובד
                    bool isMentor = random.Next(10) < 2; // 20% סיכוי להיות מנטור
                    //שאילתה להכנסת העובד לדאטא בייס
                    string query = @"INSERT INTO Employees (Name, Phone, Email, HourlySalary, Rate, IsMentor) 
                       VALUES (@Name, @Phone, @Email, @HourlySalary, @Rate, @IsMentor); 
                       SELECT SCOPE_IDENTITY();";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        //הוספת פרמטקרים 
                        command.Parameters.AddWithValue("@Name", name);
                        command.Parameters.AddWithValue("@Phone", $"{random.Next(100, 1000)}-{random.Next(100, 1000)}-{random.Next(1000, 10000)}");
                        command.Parameters.AddWithValue("@Email", $"{firstName.ToLower()}.{lastName.ToLower()}@example.com");
                        command.Parameters.AddWithValue("@HourlySalary", hourlySalary);
                        command.Parameters.AddWithValue("@Rate", rate);
                        command.Parameters.AddWithValue("@IsMentor", isMentor);
                        //הרצת השאילתה
                        object result = command.ExecuteScalar();

                        if (result != null)
                        {
                            //העלאת מונה עובדים
                            employeesCreated++;
                            int employeeId = Convert.ToInt32(result);

                            // שאילתה לעדכון סיסמה  לעובד
                            string updatePassword = @"UPDATE Employees set Password=@Password where EmployeeID=@EmployeeID ";
                            using (SqlCommand command2 = new SqlCommand(updatePassword, connection))
                            {
                                //הוספת פרמטרים והרצת השאילתה
                                command2.Parameters.AddWithValue("@Password", result.ToString());
                                command2.Parameters.AddWithValue("@EmployeeID", result);
                                command2.ExecuteScalar();
                            }

                            // קביעת מספר הסניפים שהעובד יעבוד בהם 
                            int branchCount = random.Next(1, Math.Min(4, allBranchIds.Count + 1));

                            // בחירת סניפים אקראיים 
                            var selectedBranches = allBranchIds
                                .OrderBy(x => random.Next())
                                .Take(branchCount)
                                .ToList();

                            // הקצאת העובד לסניפים שנבחרו
                            foreach (int branchId in selectedBranches)
                            {
                                //שאילתה לשיוך עובד לסניף
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
                            var selectedRoles = roles
                            .OrderBy(x => random.Next())
                            .Take(Math.Min(roleCount, roles.Count))
                            .ToList();
                            // הקצאת התפקידים 
                            foreach(string role in selectedRoles)
                            {
                                //שאילתה לשיוך עובד לתפקיד
                                string roleAssignQuery = "INSERT INTO EmployeeRoles (EmployeeID, RoleID) VALUES (@EmployeeID, @RoleID)";
                                using (SqlCommand roleAssignCommand = new SqlCommand(roleAssignQuery, connection))
                                {
                                    //הוספת פרמטרים והרצת השאילתה
                                    roleAssignCommand.Parameters.AddWithValue("@EmployeeID", employeeId);
                                    roleAssignCommand.Parameters.AddWithValue("@RoleID", GetRoleId(role));
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
            List<int> branchShiftIds = new List<int>();
            //שאילתה לקבלת משמרות בסניף הנוכחי
            string shiftsQuery = "SELECT ShiftID FROM Shifts WHERE BranchID = @BranchID";
            using (SqlCommand command = new SqlCommand(shiftsQuery, connection))
            {
                //הוספת פרמטרים והרצת השאילתה
                command.Parameters.AddWithValue("@BranchID", branchId);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        //הוספת המשמרת לרשימת המשמרות
                        branchShiftIds.Add(reader.GetInt32(0));
                    }
                }
            }

            // אם אין משמרות בסניף זה, סיום הפונקציה
            if (branchShiftIds.Count == 0)
                return;

            // החלטה על כמה משמרות מועדפות להקצות- 30-80% מהמשמרות הזמינות
            int preferredShiftsCount = random.Next(branchShiftIds.Count / 3, (branchShiftIds.Count * 8 / 10) + 1);

            // בחירת משמרות אקראיות 
            var selectedShifts = branchShiftIds
                .OrderBy(x => random.Next())
                .Take(preferredShiftsCount)
                .ToList();

            // הקצאת המשמרות המועדפות
            foreach(int shiftID in selectedShifts)
            {
                //שאילתה לשיוך משמרת לעובד
                string insertQuery = "INSERT INTO EmployeePreferredShifts (EmployeeID, ShiftID) VALUES (@EmployeeID, @ShiftID)";
                using (SqlCommand command = new SqlCommand(insertQuery, connection))
                {
                    //הוספת פרמטרים והרצת השאילתה
                    command.Parameters.AddWithValue("@EmployeeID", employeeId);
                    command.Parameters.AddWithValue("@ShiftID", shiftID);
                    command.ExecuteNonQuery();
                }
            }
        }

        // פונקציה ליצירת משמרות לסניף
        // פרמטרים
        // connection - חיבור פתוח למסד הנתונים
        // branchId - מזהה הסניף ליצירת המשמרות
        // ערך מוחזר: אין
        private static void CreateShiftsForBranch(SqlConnection connection, int branchId)
        {
            // קבלת מזהי פרקי זמן
            List<string> timeSlots = helper.getTimeSlots();
            // קבלת מזהי סוגי משמרות
            List<string> shiftTypes = helper.getShiftTypes();
            // קבלת תפקידים
            List<string> roles = helper.getRoles();

            // עבור כל יום בשבוע
            foreach (string day in daysOfWeek)
            {
                // עבור כל סוג זמן
                foreach (var timeSlot in timeSlots)
                {
                    // בחירת סוג משמרת אקראי
                    string shiftTypeName = "Regular"; // ברירת מחדל
                    if (shiftTypes.Count > 0)
                    {
                        var shiftTypeNames = shiftTypes.ToList();
                        shiftTypeName = shiftTypeNames[random.Next(shiftTypeNames.Count)];
                    }

                    int shiftTypeId = GetShiftTypeId(shiftTypeName);
                    bool isBusy = random.Next(2) == 0; // 50% סיכוי למשמרת עמוסה

                    // שאילתה להוספת המשמרת לדאטאבייס
                    string insertShiftQuery = @"INSERT INTO Shifts (BranchID, TimeSlotID, DayOfWeek, ShiftTypeID, IsBusy) 
                         VALUES (@BranchID, @TimeSlotID, @DayOfWeek, @ShiftTypeID, @IsBusy);
                         SELECT SCOPE_IDENTITY();";

                    using (SqlCommand command = new SqlCommand(insertShiftQuery, connection))
                    {
                        //הוספת פרמטרים
                        command.Parameters.AddWithValue("@BranchID", branchId);
                        command.Parameters.AddWithValue("@ShiftTypeID", shiftTypeId);
                        command.Parameters.AddWithValue("@TimeSlotID", GetTimeSlotId(timeSlot));
                        command.Parameters.AddWithValue("@DayOfWeek", day);
                        command.Parameters.AddWithValue("@IsBusy", isBusy);
                        //הרצת השאילתה
                        object result = command.ExecuteScalar();

                        if (result != null)
                        {
                            int shiftId = Convert.ToInt32(result);

                            // הוספת דרישות התפקידים למשמרת
                            foreach (var role in roles)
                            {
                               
                                    int requiredCount = random.Next(0, 4); // 1-3 עובדים נדרשים
                                    //שאילתה להכנסת דרישות כוח אדם עבור תפקיד
                                    string insertRoleReqQuery = @"INSERT INTO ShiftRequiredRoles (ShiftID, RoleID, RequiredCount) 
                                                           VALUES (@ShiftID, @RoleID, @RequiredCount)";

                                    using (SqlCommand roleCommand = new SqlCommand(insertRoleReqQuery, connection))
                                    {
                                    //הוספת פרמטרים
                                        roleCommand.Parameters.AddWithValue("@ShiftID", shiftId);
                                        roleCommand.Parameters.AddWithValue("@RoleID", GetRoleId(role));
                                        roleCommand.Parameters.AddWithValue("@RequiredCount", requiredCount);
                                    //הרצת השאילתה
                                        roleCommand.ExecuteNonQuery();
                                    }
                                
                            }
                        }
                    }
                }
            }
        }
        // פונקציה לקבלת מזהה סוג משמרת לפי שם
        // פרמטרים
        // shiftTypeName - שם סוג המשמרת
        // ערך מוחזר: מזהה סוג המשמרת, או -1 אם לא נמצא
        private static int GetShiftTypeId(string shiftTypeName)
        {
            try
            {
                //יצירת חיבור לבסיס הנתונים
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    //פתיחת החיבור לבסיס הנתונים
                    connection.Open();
                    //שאילתה לקבלת מזהה סוג המשמרת לפי השם
                    string query = "SELECT ShiftTypeID FROM ShiftTypes WHERE TypeName = @TypeName";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        //הוספת פרמטר והרצת השאילתה
                        command.Parameters.AddWithValue("@TypeName", shiftTypeName);
                        object result = command.ExecuteScalar();
                        return result != null ? Convert.ToInt32(result) : -1;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"שגיאה בקבלת מזהה סוג משמרת: {ex.Message}");
                return -1;
            }
        }

        // פונקציה לקבלת מזהה משבצת זמן לפי שם
        // פרמטרים:
        // timeSlotName - שם משבצת הזמן
        // ערך מוחזר: מזהה משבצת הזמן, או -1 אם לא נמצא
        private static int GetTimeSlotId(string timeSlotName)
        {
            try
            {
                //יצירת חיבור לבסיס הנתונים
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    //פתיחת החיבור לבסיס הנתונים
                    connection.Open();
                    //שאילתה לקבלת מזהה זמן המשמרת לפי שם הזמן
                    string query = "SELECT TimeSlotID FROM TimeSlots WHERE TimeSlotName = @TimeSlotName";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        //הוספת פרמטר והרצת השאילתה
                        command.Parameters.AddWithValue("@TimeSlotName", timeSlotName);
                        object result = command.ExecuteScalar();
                        return result != null ? Convert.ToInt32(result) : -1;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"שגיאה בקבלת מזהה משבצת זמן: {ex.Message}");
                return -1;
            }
        }

        // פונקציה לקבלת מזהה תפקיד לפי שם
        // פרמטרים:
        // roleName - שם התפקיד
        // ערך מוחזר: מזהה התפקיד, או -1 אם לא נמצא
        private static int GetRoleId(string roleName)
        {
            try
            {
                //יצירת חיבור לבסיס הנתונים
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    //פתיחת החיבור לבסיס הנתונים
                    connection.Open();
                    //שאילתה לקבלת מזהה התפקיד לפי שם התפקיד
                    string query = "SELECT RoleID FROM Roles WHERE RoleName = @RoleName";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        //הוספת פרמטר והרצת השאילתה
                        command.Parameters.AddWithValue("@RoleName", roleName);
                        object result = command.ExecuteScalar();
                        return result != null ? Convert.ToInt32(result) : -1;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"שגיאה בקבלת מזהה תפקיד: {ex.Message}");
                return -1;
            }
        }
    }

}