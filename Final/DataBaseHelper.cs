using Final;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TreeView;
using System.Windows.Forms;
using EmployeeSchedulingApp;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography.X509Certificates;

public class DataBaseHelper
{
    private static string connectionString = "Data Source=(localdb)\\mssqllocaldb;Initial Catalog=EmployeeScheduling;Integrated Security=True;MultipleActiveResultSets=True";

    // פונקציה לטעינת כל מידע המשתמש המחובר למערכת
    // פרמטרים:
    // username - שם המשתמש המחובר למערכת
    // branches - רשימת הסניפים שיש לטעון
    // employees - רשימת העובדים שיש לטעון
    // ערך מוחזר: אין
    public void LoadDataForUser(string username, List<Branch> branches, List<Employee> employees)
    {
        try
        {
            //ניקוי רשימת הסניפים והעובדים כדיי למנוע כפל מידע
            branches.Clear();
            employees.Clear();
            //יצירת חיבור לבסיס הנתונים
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                //פתיחת חיבור לבסיס הנתונים
                connection.Open();

                // טעינת הסניפים והעובדים של המשתמש הנוכחי לרשימות זמניות
                List<Branch> loadedBranches = LoadUserBranches(username);
                List<Employee> loadedEmployees = LoadUserEmployees(username);
                // טעינת הסניפים והעובדים של המשתמש הנוכחי לרשימות הרצויות
                branches.AddRange(loadedBranches);
                employees.AddRange(loadedEmployees);
            }
        }
        //הדפסת הודעת שגיאה למשתמש במקרה והייתה תקלה בטעינת הנתונים מבסיס הנתונים
        catch (Exception ex)
        {
            MessageBox.Show($"שגיאה בטעינת נתוני המשתמש: {ex.Message}", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
            branches.Clear();
            employees.Clear();
        }
    }


    
    // פונקציה לטעינת רשימת הסניפים של המשתמש המחובר למערכת
    // פרמטרים:
    // username - שם המשתמש המחובר למערכת
    // ערך מוחזר: רשימת סניפים של המשתמש
    public List<Branch> LoadUserBranches(string username)
    {
        //אתחול רשימת סניפים חדשה
        List<Branch> branches = new List<Branch>();

        try
        {
            //יצירת חיבור חדש לבסיס הנתונים
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                //שאילתה השולפת מבסיס הנתונים את כל המזהיי הסניפים ושמות הסניפים של המשתמש הנוכחי
                string query = @"
                   SELECT b.BranchID, b.Name 
                   FROM Branches b
                   INNER JOIN UserBranches ub ON b.BranchID = ub.BranchID
                   INNER JOIN Users u ON ub.UserID = u.UserID
                   WHERE u.Username = @Username";

                //אתחול השאילתה
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    //הוספת הפרמטרים לשאילתה
                    command.Parameters.AddWithValue("@Username", username);

                    //הרצת השאילתה לתוך הקורא
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        //קריאת תוצאות השאילתה 
                        while (reader.Read())
                        {
                            //חילוץ נתוני הסניף הבסיסיים
                            int branchId = reader.GetInt32(0);
                            string branchName = reader.GetString(1);

                            //יצירת אובייקט סניף חדש עם כל המידע
                            Branch branch = new Branch
                            {
                                ID = branchId,
                                Name = branchName
                            };

                            //הוספת הסניף לרשימת הסניפים בתוכנית
                            branches.Add(branch);
                        }
                    }
                }

                //בעבור כל סניף, נטען את המשמרות של הסניף מבסיס הנתונים
                foreach (Branch branch in branches)
                {
                    branch.Shifts = LoadBranchShifts(branch.ID);
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"שגיאה בטעינת סניפים: {ex.Message}", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        return branches;
    }

    // פונקציה לטעינת רשימת העובדים של המשתמש המחובר למערכת
    // פרמטרים:
    // username - שם המשתמש המחובר למערכת
    // ערך מוחזר: רשימת עובדים של המשתמש
    public List<Employee> LoadUserEmployees(string username)
    {
        //אתחול רשימת עובדים חדשה
        List<Employee> employees = new List<Employee>();

        try
        {
            //יצירת חיבור חדש לבסיס הנתונים
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                //שאילתה השולפת מבסיס הנתונים את כל הנתונים הרלוונטים על העובד של המשתמש הנוכחי
                string query = @"
                SELECT DISTINCT e.EmployeeID, e.Name, e.Phone, e.Email, e.HourlySalary, e.Rate, 
               e.IsMentor
               FROM Employees e
               INNER JOIN EmployeeBranches eb ON e.EmployeeID = eb.EmployeeID
               INNER JOIN UserBranches ub ON eb.BranchID = ub.BranchID
               INNER JOIN Users u ON ub.UserID = u.UserID
               WHERE u.Username = @Username";

                //אתחול השאילתה
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    //הוספת הפרמטרים לשאילתה
                    command.Parameters.AddWithValue("@Username", username);

                    //הרצת השאילתה לתוך הקורא
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        //קריאת תוצאות השאילתה 
                        while (reader.Read())
                        {
                            //חילוץ נתוני העובד הבסיסיים
                            int employeeId = reader.GetInt32(0);
                            string name = reader.GetString(1);
                            string phone = reader.IsDBNull(2) ? null : reader.GetString(2);
                            string email = reader.IsDBNull(3) ? null : reader.GetString(3);
                            decimal hourlySalary = reader.GetDecimal(4);
                            int rate = reader.IsDBNull(5) ? 0 : reader.GetInt32(5);
                            bool isMentor = reader.GetBoolean(6);

                            //בעבור כל עובד נטען את התפקידים שלו
                            HashSet<string> roles = LoadEmployeeRoles(employeeId);
                            //בעבור כל עובד נטען את המשמרות שהוא יכול לעבוד בהן
                            HashSet<int> requestedShifts = LoadEmployeePreferredShifts(employeeId);
                            //בעבור כל עובד נטען את הסניפים שלו
                            List<string> branches = LoadEmployeeBranches(employeeId);

                            //יצירת אובייקט עובד חדש עם כל המידע
                            Employee employee = new Employee(
                                employeeId,
                                name,
                                roles,
                                requestedShifts,
                                rate,
                                (int)hourlySalary,
                                isMentor,
                                branches
                            );

                            //הוספת העובד לרשימת העובדים בתוכנית
                            employees.Add(employee);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"שגיאה בטעינת עובדים: {ex.Message}", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        return employees;
    }

    // פונקציה לטעינת רשימת המשמרות של סניף מסוים
    // פרמטרים:
    // branchId - מזהה הסניף שאת המשמרות שלו יש לטעון
    // ערך מוחזר: רשימת משמרות של הסניף
    public List<Shift> LoadBranchShifts(int branchId)
    {
        //אתחול רשימת משמרות חדשה
        List<Shift> shifts = new List<Shift>();

        try
        {
            //יצירת חיבור חדש לבסיס הנתונים
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                //שאילתה השולפת מבסיס הנתונים את כל הנתונים הרלוונטים על המשמרות של הסניף הנוכחי
                string query = @"
                SELECT s.ShiftID, ts.TimeSlotName, s.DayOfWeek, st.TypeName, s.IsBusy
                FROM Shifts s
                INNER JOIN ShiftTypes st ON s.ShiftTypeID = st.ShiftTypeID
                INNER JOIN TimeSlots ts on ts.TimeSlotID = s.TimeSlotID
                WHERE s.BranchID = @BranchID";

                //אתחול השאילתה
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    //הוספת הפרמטרים לשאילתה
                    command.Parameters.AddWithValue("@BranchID", branchId);

                    //הרצת השאילתה לתוך הקורא
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        //קריאת תוצאות השאילתה
                        while (reader.Read())
                        {
                            //חילוץ נתוני המשמרת הבסיסיים
                            int shiftId = reader.GetInt32(0);
                            string timeSlot = reader.GetString(1);
                            string dayOfWeek = reader.GetString(2);
                            string shiftType = reader.GetString(3);
                            bool isBusy = reader.GetBoolean(4);

                            //טעינת התפקידים הנדרשים למשמרת
                            Dictionary<string, int> requiredRoles = LoadShiftRequiredRoles(shiftId);

                            //יצירת אובייקט משמרת חדש
                            Shift shift = new Shift(
                                shiftId,
                                "Branch " + branchId,
                                timeSlot,
                                dayOfWeek,
                                requiredRoles,
                                isBusy,
                                new Dictionary<string, List<Employee>>(),
                                shiftType
                            );

                            //הוספת המשמרת לרשימת המשמרות 
                            shifts.Add(shift);
                        }
                    }
                }
            }
        }
        //הדפסת שגיאה במקרה שהייתה בעיה בטעינת המשמרות
        catch (Exception ex)
        {
            Console.WriteLine("Error loading branch shifts: " + ex.Message);
        }

        return shifts;
    }

    // פונקציה לטעינת מידע על התפקידים הנדרשים למשמרת מסוימת
    // פרמטרים:
    // shiftId - מזהה המשמרת שאת התפקידים הנדרשים בה יש לטעון
    // ערך מוחזר: מילון של שמות תפקידים וכמות העובדים הנדרשת מכל תפקיד
    public Dictionary<string, int> LoadShiftRequiredRoles(int shiftId)
    {
        //אתחול מילון תפקידים נדרשים
        Dictionary<string, int> requiredRoles = new Dictionary<string, int>();

        try
        {
            //יצירת חיבור חדש לבסיס הנתונים
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                //שאילתה השולפת מבסיס הנתונים מידע על התפקידים הנדרשים למשמרת
                string query = @"
                SELECT r.RoleName, sr.RequiredCount
                FROM ShiftRequiredRoles sr
                INNER JOIN Roles r ON sr.RoleID = r.RoleID
                WHERE sr.ShiftID = @ShiftID";

                //אתחול השאילתה
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    //הוספת הפרמטרים לשאילתה
                    command.Parameters.AddWithValue("@ShiftID", shiftId);

                    //הרצת השאילתה לתוך הקורא
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        //קריאת תוצאות השאילתה
                        while (reader.Read())
                        {
                            //חילוץ שם התפקיד וכמות העובדים הנדרשת
                            string roleName = reader.GetString(0);
                            int requiredCount = reader.GetInt32(1);

                            //הכנסת הנתונים למילון
                            requiredRoles[roleName] = requiredCount;
                        }
                    }
                }
            }
        }
        //הצגת הודעת שגיאה למשתשמש אם הייתה שגיאה בטעינת הנתונים מהדאטא בייס
        catch (Exception ex)
        {
            Console.WriteLine("Error loading shift required roles: " + ex.Message);
        }

        return requiredRoles;
    }

    // פונקציה לטעינת רשימת התפקידים של עובד מסוים
    // פרמטרים:
    // employeeId - מזהה העובד שאת התפקידים שלו יש לטעון
    // ערך מוחזר: מערך של שמות תפקידים של העובד
    public HashSet<string> LoadEmployeeRoles(int employeeId)
    {
        //אתחול רשימת תפקידים חדשה
        HashSet<string> roles = new HashSet<string>();

        try
        {
            //יצירת חיבור חדש לבסיס הנתונים
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                //ששאילתה השולפת מבסיס הנתונים את תפקידי העובד
                string query = @"
                SELECT r.RoleName
                FROM EmployeeRoles er
                INNER JOIN Roles r ON er.RoleID = r.RoleID
                WHERE er.EmployeeID = @EmployeeID";

                //אתחול השאילתה
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    //הוספת הפרמטרים לשאילתה
                    command.Parameters.AddWithValue("@EmployeeID", employeeId);

                    //הרצת השאילתה לתוך הקורא
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        //קריאת תוצאות השאילתה והוספת התפקידים לרשימת התפקידים
                        while (reader.Read())
                        {
                            //הוספת שם התפקיד לרשימה
                            roles.Add(reader.GetString(0));
                        }
                    }
                }
            }
        }
        //הצגת הודעת שגיאה למשתשמש אם הייתה שגיאה בטעינת הנתונים מהדאטא בייס
        catch (Exception ex)
        {
            Console.WriteLine("Error loading employee roles: " + ex.Message);
        }

        return roles;
    }

    // פונקציה לטעינת רשימת המשמרות המועדפות של עובד מסוים
    // פרמטרים:
    // employeeId - מזהה העובד שאת המשמרות המועדפות שלו יש לטעון
    // ערך מוחזר: מערך של מזהי משמרות מועדפות של העובד
    private HashSet<int> LoadEmployeePreferredShifts(int employeeId)
    {
        //אתחול האש סט משמרות מועדפות חדש
        HashSet<int> preferredShifts = new HashSet<int>();

        try
        {
            //יצירת חיבור חדש לבסיס הנתונים
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                //שאילתה השולפת מבסיס הנתונים את המשמרות המועדפות של העובד
                string query = @"
                SELECT ShiftID
                FROM EmployeePreferredShifts
                WHERE EmployeeID = @EmployeeID";

                //אתחול השאילתה
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    //הוספת הפרמטרים לשאילתה
                    command.Parameters.AddWithValue("@EmployeeID", employeeId);

                    //הרצת השאילתה לתוך הקורא
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        //קריאת תוצאות השאילתה והוספת המשמרות למערך המשמרות המועדפות
                        while (reader.Read())
                        {
                            //הוספת מזהה המשמרת למערך המשמרות המועדפות
                            preferredShifts.Add(reader.GetInt32(0));
                        }
                    }
                }
            }
        }
        //הצגת הודעת שגיאה למשתשמש אם הייתה שגיאה בטעינת הנתונים מהדאטא בייס
        catch (Exception ex)
        {
            Console.WriteLine("Error loading employee preferred shifts: " + ex.Message);
        }

        return preferredShifts;
    }

    // פונקציה לטעינת רשימת הסניפים של עובד מסוים
    // פרמטרים:
    // employeeId - מזהה העובד שאת הסניפים שלו יש לטעון
    // ערך מוחזר: רשימת שמות הסניפים של העובד
    private List<string> LoadEmployeeBranches(int employeeId)
    {
        //אתחול רשימת סניפים חדשה
        List<string> branches = new List<string>();

        try
        {
            //יצירת חיבור חדש לבסיס הנתונים
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                //שאילתה השולפת מבסיס הנתונים את הסניפים שהעובד עובד בהם
                string query = @"
                SELECT b.Name
                FROM EmployeeBranches eb
                INNER JOIN Branches b ON eb.BranchID = b.BranchID
                WHERE eb.EmployeeID = @EmployeeID";

                //אתחול השאילתה
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    //הוספת הפרמטרים לשאילתה
                    command.Parameters.AddWithValue("@EmployeeID", employeeId);

                    //הרצת השאילתה לתוך הקורא
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        //קריאת תוצאות השאילתה והוספת הסניפים לרשימת הסניפים
                        while (reader.Read())
                        {
                            //הוספת שם הסניף לרשימה
                            branches.Add(reader.GetString(0));
                        }
                    }
                }
            }
        }
        //הצגת הודעת שגיאה למשתשמש אם הייתה שגיאה בטעינת הנתונים מהדאטא בייס
        catch (Exception ex)
        {
            Console.WriteLine("Error loading employee branches: " + ex.Message);
        }

        return branches;
    }
   

    // פונקציה למחיקת סניף מבסיס הנתונים
    // פרמטרים:
    // branchId - מזהה הסניף שיש למחוק
    // ערך מוחזר: אמת אם המחיקה הצליחה, שקר אחרת
    public bool DeleteBranch(int branchId)
    {
        try
        {
            //יצירת חיבור לבסיס הנתונים
            using (SqlConnection newConnection = new SqlConnection(connectionString))
            {
                newConnection.Open();
                // שאילתה המוחקת סניף
                string deleteBranch = @"
                        DELETE FROM Branches WHERE BranchID = @BranchID";
                //אתחול השאילתה
                using (SqlCommand command = new SqlCommand(deleteBranch, newConnection))
                {
                    //הוספת הפרמטרים לשאילתה
                    command.Parameters.AddWithValue("@BranchID", branchId);
                    command.ExecuteNonQuery();
                }
            }
        }
        //הצגת הודעת שגיאה למשתשמש אם הייתה שגיאה במחיקת הנתונים מהדאטא בייס
        catch (Exception ex)
        {
            MessageBox.Show($"שגיאה במחיקת הסניף: {ex.Message}", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
        return true;
    }

    // פונקציה למחיקת עובד מבסיס הנתונים
    // פרמטרים:
    // EmployeeID - מזהה העובד שיש למחוק
    // ערך מוחזר: אמת אם המחיקה הצליחה, שקר אחרת
    public bool DeleteEmployee(int EmployeeID)
    {
        try
        {
            //יצירת חיבור לבסיס הנתונים
            using (SqlConnection newConnection = new SqlConnection(connectionString))
            {
                newConnection.Open();
                // שאילתה המוחקת עובד
                string deleteBranch = @"
                        DELETE FROM Employees WHERE EmployeeID = @EmployeeID";
                //אתחול השאילתה
                using (SqlCommand command = new SqlCommand(deleteBranch, newConnection))
                {
                    //הוספת הפרמטרים לשאילתה
                    command.Parameters.AddWithValue("@EmployeeID", EmployeeID);
                    command.ExecuteNonQuery();
                }
            }
        }
        //הצגת הודעת שגיאה למשתשמש אם הייתה שגיאה במחיקת הנתונים מהדאטא בייס
        catch (Exception ex)
        {
            MessageBox.Show($"שגיאה במחיקת הסניף: {ex.Message}", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
        return true;
    }

    // פונקציה לקבלת מזהה משתמש לפי שם משתמש
    // פרמטרים:
    // username - שם המשתמש לחיפוש
    // connection - חיבור פתוח למסד הנתונים
    // ערך מוחזר: מזהה המשתמש, או -1 אם לא נמצא
    public int GetUserIdByUsername(string username, SqlConnection connection)
    {
        try
        {
            // הכנת שאילתה לקבלת מזהה המשתמש
            string query = "SELECT UserID FROM Users WHERE Username = @Username";
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                // הוספת פרמטר שם המשתמש לשאילתה
                command.Parameters.AddWithValue("@Username", username);
                // ביצוע השאילתה וקבלת התוצאה
                object result = command.ExecuteScalar();

                // בדיקה אם נמצאה תוצאה, והחזרתה בתור מספר
                if (result != null && result != DBNull.Value)
                {
                    return Convert.ToInt32(result);
                }
            }
        }
        catch (Exception ex)
        {
            // רישום השגיאה לקונסול
            Console.WriteLine($"שגיאה בקבלת מזהה משתמש: {ex.Message}");
        }

        // מחזיר -1 במקרה של כישלון
        return -1;
    }

    // פונקציה לקבלת או יצירת סוג משמרת
    // פרמטרים:
    // typeName - שם סוג המשמרת
    // connection - חיבור פתוח למסד הנתונים
    // ערך מוחזר: מזהה סוג המשמרת, או -1 אם נכשל
    public int GetOrCreateShiftType(string typeName, SqlConnection connection)
    {
        try
        {
            // בדיקה אם סוג המשמרת כבר קיים
            string query = "SELECT ShiftTypeID FROM ShiftTypes WHERE TypeName = @TypeName";
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                // הוספת פרמטר שם סוג המשמרת לשאילתה
                command.Parameters.AddWithValue("@TypeName", typeName);
                // ביצוע השאילתה וקבלת התוצאה
                object result = command.ExecuteScalar();

                // אם נמצאה תוצאה, החזרת המזהה
                if (result != null && result != DBNull.Value)
                {
                    return Convert.ToInt32(result);
                }
            }

            // אם לא קיים, יצירת סוג משמרת חדש
            string insertQuery = @"
                    INSERT INTO ShiftTypes (TypeName)
                    VALUES (@TypeName);
                    SELECT CAST(SCOPE_IDENTITY() AS INT)";

            using (SqlCommand command = new SqlCommand(insertQuery, connection))
            {
                // הוספת פרמטר שם סוג המשמרת לשאילתה
                command.Parameters.AddWithValue("@TypeName", typeName);
                // ביצוע השאילתה והחזרת המזהה החדש
                return (int)command.ExecuteScalar();
            }
        }
        catch (Exception ex)
        {
            // רישום השגיאה לקונסול
            Console.WriteLine($"שגיאה בקבלת/יצירת סוג משמרת: {ex.Message}");
            return -1;
        }
    }

    // פונקציה להוספת משמרת חדשה
    // פרמטרים:
    // branchId - מזהה הסניף שאליו תתווסף המשמרת
    // dayOfWeek - יום בשבוע
    // timeSlot - משבצת זמן
    // shiftTypeId - מזהה סוג המשמרת
    // connection - חיבור פתוח למסד הנתונים
    // ערך מוחזר: אין
    public void AddShift(int branchId, string dayOfWeek, string timeSlot, int shiftTypeId, SqlConnection connection)
    {
        try
        {
            // הוספת המשמרת
            string insertShiftQuery = @"
                    INSERT INTO Shifts (BranchID, TimeSlot, DayOfWeek, ShiftTypeID, IsBusy)
                    VALUES (@BranchID, @TimeSlot, @DayOfWeek, @ShiftTypeID, 0);
                    SELECT CAST(SCOPE_IDENTITY() AS INT)";

            int shiftId;
            using (SqlCommand command = new SqlCommand(insertShiftQuery, connection))
            {
                // הוספת פרמטרים לשאילתת הוספת המשמרת
                command.Parameters.AddWithValue("@BranchID", branchId);
                command.Parameters.AddWithValue("@TimeSlot", timeSlot);
                command.Parameters.AddWithValue("@DayOfWeek", dayOfWeek);
                command.Parameters.AddWithValue("@ShiftTypeID", shiftTypeId);

                // ביצוע השאילתה וקבלת מזהה המשמרת החדשה
                shiftId = (int)command.ExecuteScalar();
                Console.WriteLine($"נוספה משמרת חדשה: {dayOfWeek}, {timeSlot}, ID: {shiftId}");
            }

            // הוספת דרישות תפקידים בסיסיות למשמרת
            List<string> roles = getRoles();
            Dictionary<string, int> defaultRoles = new Dictionary<string, int>();
            foreach (String s in roles)
            {
                // מציאת מזהה התפקיד או יצירת תפקיד חדש
                int roleId = GetOrCreateRole(s, connection);
                // הוספת התפקיד כדרישה למשמרת החדשה
                AddShiftRequiredRole(shiftId, roleId, 1, connection);
            }
        }
        catch (Exception ex)
        {
            // רישום השגיאה לקונסול
            Console.WriteLine($"שגיאה בהוספת משמרת: {ex.Message}");
        }
    }

    // פונקציה לקבלת מזהה תפקיד או יצירת תפקיד חדש
    // פרמטרים:
    // roleName - שם התפקיד
    // connection - חיבור פתוח למסד הנתונים
    // ערך מוחזר: מזהה התפקיד, או -1 אם נכשל
    public int GetOrCreateRole(string roleName, SqlConnection connection)
    {
        try
        {
            // בדיקה אם התפקיד כבר קיים
            string query = "SELECT RoleID FROM Roles WHERE RoleName = @RoleName";
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                // הוספת פרמטר שם התפקיד לשאילתה
                command.Parameters.AddWithValue("@RoleName", roleName);
                // ביצוע השאילתה וקבלת התוצאה
                object result = command.ExecuteScalar();
                return Convert.ToInt32(result);
            }
        }
        catch (Exception ex)
        {
            // רישום השגיאה לקונסול
            Console.WriteLine($"שגיאה בקבלת/יצירת תפקיד: {ex.Message}");
            return -1;
        }
    }

    // פונקציה להוספת דרישת תפקיד למשמרת
    // פרמטרים:
    // shiftId - מזהה המשמרת
    // roleId - מזהה התפקיד
    // requiredCount - מספר העובדים הנדרש בתפקיד זה
    // connection - חיבור פתוח למסד הנתונים
    // ערך מוחזר: אין
    public void AddShiftRequiredRole(int shiftId, int roleId, int requiredCount, SqlConnection connection)
    {
        try
        {
            // שאילתה להוספת דרישת תפקיד
            string insertQuery = @"
                    INSERT INTO ShiftRequiredRoles (ShiftID, RoleID, RequiredCount)
                    VALUES (@ShiftID, @RoleID, @RequiredCount)";

            using (SqlCommand command = new SqlCommand(insertQuery, connection))
            {
                // הוספת פרמטרים לשאילתה
                command.Parameters.AddWithValue("@ShiftID", shiftId);
                command.Parameters.AddWithValue("@RoleID", roleId);
                command.Parameters.AddWithValue("@RequiredCount", requiredCount);
                // ביצוע השאילתה
                command.ExecuteNonQuery();
            }
        }
        catch (Exception ex)
        {
            // רישום השגיאה לקונסול
            Console.WriteLine($"שגיאה בהוספת דרישת תפקיד: {ex.Message}");
        }
    }

    // פונקציה לרישום משתמש חדש למערכת
    // פרמטרים:
    // username - שם המשתמש החדש
    // password - סיסמת המשתמש
    // confirmPassword - אימות סיסמה
    // fullName - שם מלא של המשתמש
    // email - כתובת דוא"ל
    // ערך מוחזר: אמת אם הרישום הצליח, שקר אחרת
    public bool PerformRegistration(string username, string password, string confirmPassword, string fullName, string email)
    {
        // בדיקת תקינות השדות
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(confirmPassword))
        {
            MessageBox.Show("נא למלא את כל השדות.", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        // בדיקת התאמת סיסמאות
        if (password != confirmPassword)
        {
            MessageBox.Show("הסיסמאות אינן תואמות.", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        // שאילתה לבדיקה אם שם המשתמש כבר קיים
        string checkUsernameQuery = "SELECT COUNT(*) FROM Users WHERE Username = @Username";

        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            connection.Open();

            // בדיקה אם שם המשתמש כבר תפוס
            using (SqlCommand checkCommand = new SqlCommand(checkUsernameQuery, connection))
            {
                checkCommand.Parameters.AddWithValue("@Username", username);
                int userCount = (int)checkCommand.ExecuteScalar();

                if (userCount > 0)
                {
                    MessageBox.Show("שם המשתמש כבר תפוס, אנא בחר שם משתמש אחר.", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }

            // שימוש בפרמטרים מונע SQL Injection
            string query = @"INSERT INTO Users (Username, Password, FullName, Email, IsActive)
                        VALUES (@Username, @Password, @FullName, @Email, @IsActive);";
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                // הוספת הפרמטרים
                command.Parameters.AddWithValue("@Username", username);
                command.Parameters.AddWithValue("@Password", password);
                command.Parameters.AddWithValue("@FullName", fullName);
                command.Parameters.AddWithValue("@Email", email);
                command.Parameters.AddWithValue("@IsActive", 1);

                // ביצוע השאילתה
                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    // הצגת הודעת הצלחה ופתיחת המסך הראשי
                    MessageBox.Show("המשתמש נוסף בהצלחה!", "הצלחה", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    MainPage main = new MainPage(username);
                    main.Show();
                }
                else
                {
                    MessageBox.Show("הוספת המשתמש נכשלה", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // הצגת הודעת סיום הרשמה
        MessageBox.Show("הרשמה הושלמה בהצלחה!", "הצלחה", MessageBoxButtons.OK, MessageBoxIcon.Information);
        return true;
    }

    // פונקציה לביצוע התחברות למערכת
    // פרמטרים:
    // username - שם המשתמש
    // password - סיסמת המשתמש
    // ערך מוחזר: אמת אם ההתחברות הצליחה, שקר אחרת
    public bool PerformLogin(string username, string password)
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            connection.Open();

            // שאילתה לבדיקת התאמת שם משתמש וסיסמה
            string query = "SELECT UserID, FullName FROM Users WHERE Username = @Username AND Password = @Password AND IsActive = 1";

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                // הוספת פרמטרים לשאילתה
                command.Parameters.AddWithValue("@Username", username);
                command.Parameters.AddWithValue("@Password", password);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read()) // אם נמצא משתמש מתאים
                    {
                        // חילוץ נתוני המשתמש
                        int userId = reader.GetInt32(0);
                        string fullName = reader.GetString(1);

                        // הצגת הודעת ברכה
                        MessageBox.Show($"ברוך הבא, {fullName}!");

                        // פתיחת המסך הראשי
                        MainPage main = new MainPage(username);
                        main.Show();
                        return true;
                    }
                    else
                    {
                        // הצגת הודעת שגיאה
                        MessageBox.Show("שם משתמש או סיסמה שגויים.", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                }
            }
        }
    }

    // פונקציה להוספת סניף חדש
    // פרמטרים:
    // branchName - שם הסניף החדש
    // userName - שם המשתמש שמוסיף את הסניף
    // ערך מוחזר: אמת אם ההוספה הצליחה, שקר אחרת
    public bool AddBranch(string branchName, string userName)
    {
        // בדיקת תקינות
        if (string.IsNullOrEmpty(branchName))
        {
            MessageBox.Show("נא להזין שם סניף", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        try
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // בדיקה אם הסניף כבר קיים
                string checkBranchQuery = "SELECT COUNT(*) FROM Branches WHERE Name = @Name";
                using (SqlCommand command = new SqlCommand(checkBranchQuery, connection))
                {
                    command.Parameters.AddWithValue("@Name", branchName);
                    int count = (int)command.ExecuteScalar();

                    if (count > 0)
                    {
                        MessageBox.Show("סניף בשם זה כבר קיים במערכת", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                }

                // הוספת הסניף החדש
                int branchId;
                string insertBranchQuery = @"
                        INSERT INTO Branches (Name)
                        VALUES (@Name);
                        SELECT CAST(SCOPE_IDENTITY() AS INT)";

                using (SqlCommand command = new SqlCommand(insertBranchQuery, connection))
                {
                    // הוספת פרמטר שם הסניף
                    command.Parameters.AddWithValue("@Name", branchName);
                    // ביצוע השאילתה וקבלת מזהה הסניף החדש
                    branchId = (int)command.ExecuteScalar();

                    Console.WriteLine($"נוסף סניף חדש עם מזהה {branchId}");
                }

                // קישור הסניף למשתמש הנוכחי
                if (!string.IsNullOrEmpty(userName))
                {
                    // קבלת מזהה המשתמש
                    int userId = GetUserIdByUsername(userName, connection);

                    if (userId > 0)
                    {
                        // הוספת הקישור בין המשתמש לסניף
                        string insertUserBranchQuery = @"
                                INSERT INTO UserBranches (UserID, BranchID)
                                VALUES (@UserID, @BranchID)";

                        using (SqlCommand command = new SqlCommand(insertUserBranchQuery, connection))
                        {
                            // הוספת פרמטרים לשאילתה
                            command.Parameters.AddWithValue("@UserID", userId);
                            command.Parameters.AddWithValue("@BranchID", branchId);
                            // ביצוע השאילתה
                            int rowsAffected = command.ExecuteNonQuery();
                            Console.WriteLine($"קישור המשתמש לסניף - שורות שהושפעו: {rowsAffected}");
                        }
                    }
                }
            }

            // הצגת הודעת הצלחה
            MessageBox.Show($"הסניף {branchName} נוסף בהצלחה!", "הצלחה", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return true;
        }
        catch (Exception ex)
        {
            // הצגת הודעת שגיאה
            MessageBox.Show($"אירעה שגיאה בהוספת הסניף: {ex.Message}", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
    }

    // פונקציה להוספת עובד חדש
    // פרמטרים:
    // employeeId - מזהה עובד (אופציונלי)
    // name - שם העובד
    // phone - מספר טלפון
    // email - דוא"ל
    // rate - דירוג העובד
    // roles - מערך של תפקידים
    // salary - שכר שעתי
    // isExperienced - האם העובד מנוסה/מנטור
    // password - סיסמת העובד
    // branchesCheckedListBox - תיבת בחירה של סניפים
    // ערך מוחזר: אמת אם ההוספה הצליחה, שקר אחרת
    public bool AddEmployee(
     string employeeId,
     string name,
     string phone,
     string email,
     string rate,
     HashSet<string> roles,
     string salary,
     bool isExperienced,
     string password,
     CheckedListBox branchesCheckedListBox)
    {
        // בדיקת תקינות שדות
        if (string.IsNullOrWhiteSpace(name) ||
          roles.Count == 0 ||  // בדיקה שנבחר לפחות תפקיד אחד
          string.IsNullOrWhiteSpace(salary) ||
          string.IsNullOrWhiteSpace(rate) ||
          string.IsNullOrWhiteSpace(password))
        {
            MessageBox.Show("נא למלא את כל השדות הדרושים ולבחור לפחות תפקיד אחד.", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        int newEmployeeId = 0;
        bool useCustomId = false;

        // בדיקה אם יש מזהה עובד ספציפי
        if (!string.IsNullOrWhiteSpace(employeeId))
        {
            // וידוא שמזהה העובד הוא מספר
            if (!int.TryParse(employeeId, out newEmployeeId))
            {
                MessageBox.Show("מזהה העובד חייב להיות מספר.", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            // בדיקה אם המזהה כבר קיים במערכת
            using (SqlConnection checkConnection = new SqlConnection(connectionString))
            {
                checkConnection.Open();
                string checkQuery = "SELECT COUNT(*) FROM Employees WHERE EmployeeID = @EmployeeID";
                using (SqlCommand checkCommand = new SqlCommand(checkQuery, checkConnection))
                {
                    checkCommand.Parameters.AddWithValue("@EmployeeID", newEmployeeId);
                    int count = (int)checkCommand.ExecuteScalar();

                    if (count > 0)
                    {
                        MessageBox.Show("מזהה העובד כבר קיים במערכת.", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                }
            }

            useCustomId = true;
        }

        // איסוף הסניפים שנבחרו
        List<string> branchList = new List<string>();
        foreach (var item in branchesCheckedListBox.CheckedItems)
        {
            branchList.Add(item.ToString());
        }

        // בדיקה שנבחר לפחות סניף אחד
        if (branchList.Count == 0)
        {
            MessageBox.Show("נא לבחור לפחות סניף אחד.", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        try
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string insertEmployeeQuery;

                if (useCustomId)
                {
                    // שימוש במזהה מותאם אישית
                    insertEmployeeQuery = @"
                SET IDENTITY_INSERT Employees ON;
                INSERT INTO Employees (EmployeeID, Name, Phone, Email, HourlySalary, Rate, IsMentor, Password)
                VALUES (@EmployeeID, @Name, @Phone, @Email, @HourlySalary, @Rate, @IsMentor, @Password);
                SET IDENTITY_INSERT Employees OFF;
                SELECT @EmployeeID;";
                }
                else
                {
                    // שימוש במזהה אוטומטי
                    insertEmployeeQuery = @"
                INSERT INTO Employees (Name, Phone, Email, HourlySalary, Rate, IsMentor, Password)
                VALUES (@Name, @Phone, @Email, @HourlySalary, @Rate, @IsMentor, @Password);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";
                }

                using (SqlCommand command = new SqlCommand(insertEmployeeQuery, connection))
                {
                    if (useCustomId)
                    {
                        // הוספת פרמטר מזהה העובד אם משתמשים במזהה מותאם אישית
                        command.Parameters.AddWithValue("@EmployeeID", newEmployeeId);
                    }

                    // הוספת שאר הפרמטרים לשאילתה
                    command.Parameters.AddWithValue("@Name", name);
                    command.Parameters.AddWithValue("@Phone", string.IsNullOrEmpty(phone) ? (object)DBNull.Value : phone);
                    command.Parameters.AddWithValue("@Email", string.IsNullOrEmpty(email) ? (object)DBNull.Value : email);
                    command.Parameters.AddWithValue("@HourlySalary", Convert.ToDecimal(salary));
                    command.Parameters.AddWithValue("@Rate", Convert.ToInt32(rate));
                    command.Parameters.AddWithValue("@IsMentor", isExperienced);
                    command.Parameters.AddWithValue("@Password", password);

                    // קבלת ה-ID של העובד החדש
                    newEmployeeId = (int)command.ExecuteScalar();
                }

                // הוספת התפקידים של העובד לטבלת EmployeeRoles
                if (roles != null && roles.Count > 0)
                {
                    foreach (string roleName in roles)
                    {
                        // בדיקה אם התפקיד קיים בטבלת Roles, אם לא - הוספתו
                        int roleId;
                        string checkRoleQuery = "SELECT RoleID FROM Roles WHERE RoleName = @RoleName";
                        using (SqlCommand command = new SqlCommand(checkRoleQuery, connection))
                        {
                            command.Parameters.AddWithValue("@RoleName", roleName);
                            object result = command.ExecuteScalar();

                            if (result == null) // התפקיד לא קיים
                            {
                                // יצירת תפקיד חדש
                                string insertRoleQuery = "INSERT INTO Roles (RoleName) VALUES (@RoleName); SELECT CAST(SCOPE_IDENTITY() AS INT)";
                                using (SqlCommand insertCommand = new SqlCommand(insertRoleQuery, connection))
                                {
                                    insertCommand.Parameters.AddWithValue("@RoleName", roleName);
                                    roleId = (int)insertCommand.ExecuteScalar();
                                }
                            }
                            else
                            {
                                roleId = (int)result;
                            }
                        }

                        // קישור העובד לתפקיד
                        string insertEmployeeRoleQuery = "INSERT INTO EmployeeRoles (EmployeeID, RoleID) VALUES (@EmployeeID, @RoleID)";
                        using (SqlCommand command = new SqlCommand(insertEmployeeRoleQuery, connection))
                        {
                            command.Parameters.AddWithValue("@EmployeeID", newEmployeeId);
                            command.Parameters.AddWithValue("@RoleID", roleId);
                            command.ExecuteNonQuery();
                        }
                    }
                }

                // קישור העובד לסניפים שנבחרו
                foreach (string branchName in branchList)
                {
                    // קבלת ה-ID של הסניף לפי שמו
                    int branchId;
                    string getBranchIdQuery = "SELECT BranchID FROM Branches WHERE Name = @BranchName";
                    using (SqlCommand command = new SqlCommand(getBranchIdQuery, connection))
                    {
                        command.Parameters.AddWithValue("@BranchName", branchName);
                        object result = command.ExecuteScalar();

                        if (result != null) // נמצא סניף עם השם הזה
                        {
                            branchId = (int)result;

                            // קישור העובד לסניף
                            string insertEmployeeBranchQuery = "INSERT INTO EmployeeBranches (EmployeeID, BranchID) VALUES (@EmployeeID, @BranchID)";
                            using (SqlCommand insertCommand = new SqlCommand(insertEmployeeBranchQuery, connection))
                            {
                                insertCommand.Parameters.AddWithValue("@EmployeeID", newEmployeeId);
                                insertCommand.Parameters.AddWithValue("@BranchID", branchId);
                                insertCommand.ExecuteNonQuery();
                            }
                        }
                    }
                }

                // הוספה לרשימת העובדים בזיכרון
                Employee newEmployee = new Employee(
                    newEmployeeId,
                    name,
                    roles,  // העברת רשימת התפקידים
                    null,
                    Convert.ToInt32(rate),
                    Convert.ToInt32(salary),
                    isExperienced,
                    branchList
                );
                Program.Employees.Add(newEmployee);

                // הצגת הודעת הצלחה
                MessageBox.Show($"העובד {name} נוסף בהצלחה!", "הצלחה", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return true;
            }
        }
        catch (Exception ex)
        {
            // הצגת הודעת שגיאה
            MessageBox.Show($"אירעה שגיאה בהוספת העובד: {ex.Message}", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
    }

    // פונקציה לשמירת שינויים במשמרת לבסיס הנתונים
    // פרמטרים:
    // shift - אובייקט המשמרת לשמירה
    // ערך מוחזר: אין
    public void SaveShiftToDatabase(Shift shift)
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            connection.Open();

            // בדיקה אם יש משמרת אחרת באותו יום ושעה באותו סניף (מלבד המשמרת הנוכחית)
            string checkDuplicateQuery = @"
            SELECT COUNT(*) FROM Shifts 
            WHERE BranchID = (SELECT BranchID FROM Shifts WHERE ShiftID = @ShiftID)
            AND TimeSlotID = (SELECT ts.TimeSlotID FROM TimeSlots ts WHERE ts.TimeSlotName = @TimeSlotName)
            AND DayOfWeek = @DayOfWeek
            AND ShiftID <> @ShiftID";

            using (SqlCommand checkCommand = new SqlCommand(checkDuplicateQuery, connection))
            {
                // הוספת פרמטרים לבדיקת כפילויות
                checkCommand.Parameters.AddWithValue("@ShiftID", shift.Id);
                checkCommand.Parameters.AddWithValue("@TimeSlotName", shift.TimeSlot);
                checkCommand.Parameters.AddWithValue("@DayOfWeek", shift.day);

                // בדיקה אם קיימת משמרת דומה
                int count = (int)checkCommand.ExecuteScalar();

                if (count > 0)
                {
                    // כבר קיימת משמרת אחרת באותו זמן ויום - הצגת הודעה למשתמש
                    MessageBox.Show(
                        $"לא ניתן לעדכן משמרת: כבר קיימת משמרת ביום {shift.day} בשעות {shift.TimeSlot} בסניף זה.",
                        "שגיאה",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    // מחיקת המשמרת הנוכחית אם יש כפילות
                    DeleteShiftFromDatabase(shift.Id);
                    return;
                }
            }

            // רק אם אין כפילות, ממשיכים לקוד העדכון

            // עדכון פרטי המשמרת
            string UpdateShiftQuery = $"UPDATE Shifts " +
                $"SET " +
                $"TimeSlotID = (SELECT ts.TimeSlotID FROM TimeSlots ts WHERE ts.TimeSlotName = @TimeSlotName)," +
                $"ShiftTypeID = (SELECT st.ShiftTypeID FROM ShiftTypes st WHERE st.TypeName = @ShiftTypeName)," +
                $"DayOfWeek = @DayOfWeek WHERE ShiftID = @ShiftID";

            using (SqlCommand command = new SqlCommand(UpdateShiftQuery, connection))
            {
                // הוספת פרמטרים לעדכון המשמרת
                command.Parameters.AddWithValue("@ShiftID", shift.Id);
                command.Parameters.AddWithValue("@TimeSlotName", shift.TimeSlot);
                command.Parameters.AddWithValue("@DayOfWeek", shift.day);
                command.Parameters.AddWithValue("@ShiftTypeName", shift.EventType);
                command.ExecuteNonQuery();
            }

            // עדכון התפקידים הנדרשים למשמרת
            foreach (var role in shift.RequiredRoles)
            {
                // עדכון פרטי המשמרת
                string UpdateRequirsRolesQuery = $"UPDATE s " +
                    $"SET s.RequiredCount = @RequiredCount " +
                    $"FROM ShiftRequiredRoles s " +
                    $"JOIN Roles r ON s.RoleID = r.RoleID " +
                    $"WHERE s.ShiftID = @ShiftID AND r.RoleName = @RoleName ";

                using (SqlCommand command = new SqlCommand(UpdateRequirsRolesQuery, connection))
                {
                    // הוספת פרמטרים לעדכון דרישות התפקידים
                    command.Parameters.AddWithValue("@ShiftID", shift.Id);
                    command.Parameters.AddWithValue("@RoleName", role.Key);
                    command.Parameters.AddWithValue("@RequiredCount", role.Value);
                    command.ExecuteNonQuery();
                }
            }

            // הודעת הצלחה למשתמש
            MessageBox.Show("משמרת עודכנה בהצלחה!", "הצלחה", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    // פונקציה לקבלת רשימת התפקידים מבסיס הנתונים
    // פרמטרים: אין
    // ערך מוחזר: רשימת שמות תפקידים
    public List<string> getRoles()
    {
        List<string> roles = new List<string>();
        // שימוש ב־SqlConnection ו־SqlCommand לשליפת הנתונים מטבלת Roles
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            // שאילתה לקבלת כל התפקידים
            string query = "SELECT RoleName FROM Roles";
            SqlCommand command = new SqlCommand(query, connection);

            // פתיחת החיבור לבסיס הנתונים
            connection.Open();

            // קריאת הנתונים והוספתם לרשימה
            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    // קורא את הערך של RoleName מהשאילתה ומוסיף לרשימה
                    roles.Add(reader["RoleName"].ToString());
                }
            }
        }
        return roles;
    }

    // פונקציה לקבלת רשימת סוגי המשמרות מבסיס הנתונים
    // פרמטרים: אין
    // ערך מוחזר: רשימת שמות סוגי משמרות
    public List<string> getShiftTypes()
    {
        List<string> shiftTypes = new List<string>();
        
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            // שאילתה לקבלת כל סוגי המשמרות
            string query = "SELECT TypeName FROM ShiftTypes";
            SqlCommand command = new SqlCommand(query, connection);

            // פתיחת החיבור לבסיס הנתונים
            connection.Open();

            // קריאת הנתונים והוספתם לרשימה
            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    // קורא את הערך של TypeName מהשאילתה ומוסיף לרשימה
                    shiftTypes.Add(reader["TypeName"].ToString());
                }
            }
        }
        return shiftTypes;
    }

    // פונקציה לקבלת רשימת משבצות הזמן מבסיס הנתונים
    // פרמטרים: אין
    // ערך מוחזר: רשימת שמות משבצות זמן
    public List<string> getTimeSlots()
    {
        List<string> TimeSlots = new List<string>();
        // שימוש ב־SqlConnection ו־SqlCommand לשליפת הנתונים מטבלת TimeSlots
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            // שאילתה לקבלת כל משבצות הזמן
            string query = "SELECT TimeSlotName FROM TimeSlots";
            SqlCommand command = new SqlCommand(query, connection);

            // פתיחת החיבור לבסיס הנתונים
            connection.Open();

            // קריאת הנתונים והוספתם לרשימה
            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    // קורא את הערך של TimeSlotName מהשאילתה ומוסיף לרשימה
                    TimeSlots.Add(reader["TimeSlotName"].ToString());
                }
            }
        }
        return TimeSlots;
    }

    // פונקציה למחיקת משמרת מבסיס הנתונים
    // פרמטרים:
    // shiftId - מזהה המשמרת למחיקה
    // ערך מוחזר: אין
    public bool DeleteShiftFromDatabase(int shiftId)
    {
        try
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                // שאילתה למחיקת משמרת
                string DeleteShift = @"
                    DELETE FROM Shifts WHERE shiftID= @ShiftID";

                using (SqlCommand command = new SqlCommand(DeleteShift, connection))
                {
                    // הוספת פרמטרים לשאילתה
                    command.Parameters.AddWithValue("@ShiftID", shiftId);
                    // ביצוע השאילתה
                    command.ExecuteNonQuery();
                }

            }
        }
        catch (Exception ex)
        {
            // הדפסת הודעת שגיאה
            Console.WriteLine($"שגיאה במחיקת המשמרת: {ex.Message}");
            return false;
        }
        return true;
    }

    // פונקציה להוספת משמרת לבסיס הנתונים
    // פרמטרים:
    // branchId - מזהה הסניף שאליו תתווסף המשמרת
    // ערך מוחזר: מזהה המשמרת החדשה
    public int AddShiftToDatabase(int branchId)
    {
        int newShiftId = 0;
        try
        {
            // קבלת רשימות של ערכים קיימים במסד הנתונים
            List<string> timeSlots = getTimeSlots();
            List<string> shiftTypes = getShiftTypes();
            List<string> roles = getRoles();

            // בחירת הערכים הראשונים כברירת מחדל
            string timeSlot = timeSlots.Count > 0 ? timeSlots[0] : "Morning";
            string shiftType = shiftTypes.Count > 0 ? shiftTypes[0] : "Regular";

            // יצירת מילון של תפקידים נדרשים למשמרת - מספר 1 לכל תפקיד
            Dictionary<string, int> requiredRoles = new Dictionary<string, int>();
            foreach (string role in roles)
            {
                requiredRoles.Add(role, 1);
            }

            //יצירת חיבור חדש לבסיס הנתונים
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                //פתיחת חיבור לבסיס הנתונים
                connection.Open();

                //שליפת TimeSlotID
                int timeSlotId = 0;
                string getTimeSlotIdQuery = "SELECT TimeSlotID FROM TimeSlots WHERE TimeSlotName = @TimeSlot";
                using (SqlCommand getTimeSlotIdCommand = new SqlCommand(getTimeSlotIdQuery, connection))
                {
                    // הוספת פרמטר שם משבצת הזמן
                    getTimeSlotIdCommand.Parameters.AddWithValue("@TimeSlot", timeSlot);
                    // ביצוע השאילתה וקבלת מזהה משבצת הזמן
                    var result = getTimeSlotIdCommand.ExecuteScalar();
                    timeSlotId = Convert.ToInt32(result);
                }

                //שליפת ShiftTypeID
                int shiftTypeId = 0;
                string getShiftTypeIdQuery = "SELECT ShiftTypeID FROM ShiftTypes WHERE TypeName = @ShiftType";
                using (SqlCommand getShiftTypeIdCommand = new SqlCommand(getShiftTypeIdQuery, connection))
                {
                    // הוספת פרמטר שם סוג המשמרת
                    getShiftTypeIdCommand.Parameters.AddWithValue("@ShiftType", shiftType);
                    // ביצוע השאילתה וקבלת מזהה סוג המשמרת
                    var result = getShiftTypeIdCommand.ExecuteScalar();
                    shiftTypeId = Convert.ToInt32(result);
                }

                //הוספת המשמרת החדשה
                string insertShiftQuery = @"
            INSERT INTO Shifts (BranchID, TimeSlotID, DayOfWeek, ShiftTypeID)
            VALUES (@BranchID, @TimeSlotID, @DayOfWeek, @ShiftTypeID);
            SELECT CAST(SCOPE_IDENTITY() AS INT)";

                using (SqlCommand insertShiftCommand = new SqlCommand(insertShiftQuery, connection))
                {
                    //הוספת הפרמטרים לשאילתה
                    insertShiftCommand.Parameters.AddWithValue("@BranchID", branchId);
                    insertShiftCommand.Parameters.AddWithValue("@TimeSlotID", timeSlotId);
                    insertShiftCommand.Parameters.AddWithValue("@DayOfWeek", "Sunday");
                    insertShiftCommand.Parameters.AddWithValue("@ShiftTypeID", shiftTypeId);

                    //ביצוע השאילתה וקבלת המזהה של המשמרת החדשה
                    newShiftId = (int)insertShiftCommand.ExecuteScalar();
                }

                //הוספת התפקידים הנדרשים למשמרת
                foreach (var role in requiredRoles)
                {
                    int roleId = 0;
                    string getRoleIdQuery = "SELECT RoleID FROM Roles WHERE RoleName = @RoleName";
                    using (SqlCommand getRoleIdCommand = new SqlCommand(getRoleIdQuery, connection))
                    {
                        // הוספת פרמטר שם התפקיד
                        getRoleIdCommand.Parameters.AddWithValue("@RoleName", role.Key);
                        // ביצוע השאילתה וקבלת מזהה התפקיד
                        var roleIdResult = getRoleIdCommand.ExecuteScalar();

                        if (roleIdResult != null)
                        {
                            roleId = Convert.ToInt32(roleIdResult);

                            // הוספת התפקיד כדרישה למשמרת
                            string insertRoleQuery = @"
                        INSERT INTO ShiftRequiredRoles (ShiftID, RoleID, RequiredCount)
                        VALUES (@ShiftID, @RoleID, @RequiredCount)";

                            using (SqlCommand insertRoleCommand = new SqlCommand(insertRoleQuery, connection))
                            {
                                //הוספת הפרמטרים לשאילתה
                                insertRoleCommand.Parameters.AddWithValue("@ShiftID", newShiftId);
                                insertRoleCommand.Parameters.AddWithValue("@RoleID", roleId);
                                insertRoleCommand.Parameters.AddWithValue("@RequiredCount", role.Value);

                                //ביצוע השאילתה
                                insertRoleCommand.ExecuteNonQuery();
                            }
                        }
                    }
                }
            }
        }
        //הדפסת הודעת שגיאה למשתמש במקרה והייתה תקלה בהוספת המשמרת
        catch (Exception ex)
        {
            MessageBox.Show($"שגיאה בהוספת משמרת: {ex.Message}", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        return newShiftId;
    }

    // פונקציה לטעינת הסניפים של עובד מסוים
    // פרמטרים:
    // selectedEmployee - אובייקט העובד שאת הסניפים שלו יש לטעון
    // ערך מוחזר: רשימת שמות הסניפים של העובד
    public List<String> LoademployeeBranches(Employee selectedEmployee)
    {
        List<String> branches = new List<String>();
        // טעינת הסניפים שהעובד משויך אליהם
        try
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                // שאילתה לקבלת הסניפים של העובד
                string employeeBranchesQuery = @"
                        SELECT b.Name
                        FROM Branches b
                        INNER JOIN EmployeeBranches eb ON b.BranchID = eb.BranchID
                        WHERE eb.EmployeeID = @EmployeeID";

                using (SqlCommand command = new SqlCommand(employeeBranchesQuery, connection))
                {
                    // הוספת פרמטר מזהה העובד
                    command.Parameters.AddWithValue("@EmployeeID", selectedEmployee.ID);

                    // קריאת התוצאות והוספתן לרשימה
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string branchName = reader.GetString(0);
                            branches.Add(branchName);
                        }
                    }
                }
                return branches;
            }
        }
        catch (Exception ex)
        {
            // הצגת הודעת שגיאה במקרה של כישלון
            MessageBox.Show("אירעה שגיאה בטעינת הסניפים : " + ex.Message, "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return null;
        }
    }

    // פונקציה לטעינת המשמרות המועדפות של עובד מסוים
    // פרמטרים:
    // selectedEmployee - אובייקט העובד שאת המשמרות המועדפות שלו יש לטעון
    // ערך מוחזר: מערך של מזהי המשמרות המועדפות של העובד
    public HashSet<int> LoademployeePrefferdShifts(Employee selectedEmployee)
    {
        try
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                // טעינת המשמרות המועדפות של העובד
                string preferredShiftsQuery = @"
                        SELECT ShiftID
                        FROM EmployeePreferredShifts
                        WHERE EmployeeID = @EmployeeID";

                HashSet<int> preferredShiftIds = new HashSet<int>();
                using (SqlCommand command = new SqlCommand(preferredShiftsQuery, connection))
                {
                    // הוספת פרמטר מזהה העובד
                    command.Parameters.AddWithValue("@EmployeeID", selectedEmployee.ID);

                    // קריאת התוצאות והוספתן למערך
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            preferredShiftIds.Add(reader.GetInt32(0));
                        }
                    }
                }
                return preferredShiftIds;
            }
        }
        catch (Exception ex)
        {
            // הצגת הודעת שגיאה במקרה של כישלון
            MessageBox.Show("אירעה שגיאה בטעינת המשמרות: " + ex.Message, "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return null;
        }
    }

    // פונקציה לטעינת פרטי המשמרות של סניף מסוים
    // פרמטרים:
    // branchId - מזהה הסניף שאת המשמרות שלו יש לטעון
    // branchName - שם הסניף
    // ערך מוחזר: רשימת אובייקטים המכילים פרטי משמרות
    public List<ShiftDisplayInfo> LoadBranchShiftsDetails(int branchId, string branchName)
    {
        List<ShiftDisplayInfo> branchShifts = new List<ShiftDisplayInfo>();
        try
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // שאילתה לקבלת פרטי המשמרות של הסניף
                string query = @"
                        SELECT s.ShiftID, ts.TimeSlotName, s.DayOfWeek, st.TypeName
                        FROM Shifts s
                        INNER JOIN ShiftTypes st ON s.ShiftTypeID = st.ShiftTypeID
                        INNER JOIN TimeSlots ts ON s.TimeSlotID=ts.TimeSlotID 
                        WHERE s.BranchID = @BranchID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // הוספת פרמטר מזהה הסניף
                    command.Parameters.AddWithValue("@BranchID", branchId);

                    // קריאת התוצאות ויצירת אובייקטי מידע על משמרות
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        List<ShiftDisplayInfo> shifts = new List<ShiftDisplayInfo>();

                        while (reader.Read())
                        {
                            shifts.Add(new ShiftDisplayInfo
                            {
                                ShiftID = reader.GetInt32(0),
                                BranchName = branchName,
                                TimeSlot = reader.GetString(1),
                                DayOfWeek = reader.GetString(2),
                                ShiftType = reader.GetString(3)
                            });
                        }

                        branchShifts = shifts;
                    }
                }
            }
            return branchShifts;
        }
        catch (Exception ex)
        {
            // הצגת הודעת שגיאה במקרה של כישלון
            MessageBox.Show("אירעה שגיאה בטעינת המשמרות: " + ex.Message, "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return null;
        }
    }

    // פונקציה לשמירת פרטי עובד לבסיס הנתונים
    // פרמטרים:
    // selectedEmployee - העובד שאת פרטיו יש לשמור
    // name - שם העובד המעודכן
    // phone - מספר הטלפון המעודכן
    // email - כתובת הדוא"ל המעודכנת
    // salary - השכר השעתי המעודכן
    // rate - הדירוג המעודכן
    // isExperienced - סטטוס המנטור המעודכן
    // roles - רשימת התפקידים המעודכנת
    // branches - רשימת הסניפים המעודכנת
    // shifts - רשימת המשמרות המועדפות המעודכנת
    // ערך מוחזר: אמת אם העדכון הצליח, שקר אחרת
    public bool SaveEmployeeToDataBase(Employee selectedEmployee, string name, string phone, string email,
        string salary, string rate, bool isExperienced, List<string> roles,
        List<Branch> branches, List<ShiftDisplayInfo> shifts)
    {
        try
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                // שימוש בטרנזקציה כדי להבטיח שכל העדכונים יתבצעו או יבוטלו יחד
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // עדכון פרטי העובד
                        string updateEmployeeQuery = @"
                        UPDATE Employees 
                        SET Name = @Name, 
                            Phone = @Phone, 
                            Email = @Email, 
                            HourlySalary = @HourlySalary, 
                            Rate = @Rate, 
                            IsMentor = @IsMentor 
                        WHERE EmployeeID = @EmployeeID";

                        using (SqlCommand command = new SqlCommand(updateEmployeeQuery, connection, transaction))
                        {
                            // הוספת פרמטרים לעדכון פרטי העובד הבסיסיים
                            command.Parameters.AddWithValue("@EmployeeID", selectedEmployee.ID);
                            command.Parameters.AddWithValue("@Name", name);
                            command.Parameters.AddWithValue("@Phone", string.IsNullOrEmpty(phone) ? (object)DBNull.Value : phone);
                            command.Parameters.AddWithValue("@Email", string.IsNullOrEmpty(email) ? (object)DBNull.Value : email);
                            command.Parameters.AddWithValue("@HourlySalary", Convert.ToDecimal(salary));
                            command.Parameters.AddWithValue("@Rate", Convert.ToInt32(rate));
                            command.Parameters.AddWithValue("@IsMentor", isExperienced);

                            command.ExecuteNonQuery();
                        }

                        // עדכון התפקידים - מחיקת התפקידים הקיימים והוספת התפקידים החדשים
                        string deleteRolesQuery = "DELETE FROM EmployeeRoles WHERE EmployeeID = @EmployeeID";
                        using (SqlCommand command = new SqlCommand(deleteRolesQuery, connection, transaction))
                        {
                            // הוספת פרמטר מזהה העובד
                            command.Parameters.AddWithValue("@EmployeeID", selectedEmployee.ID);
                            command.ExecuteNonQuery();
                        }

                        // הוספת התפקידים הנבחרים
                        foreach (string roleName in roles)
                        {
                            // בדיקה אם התפקיד קיים, אם לא - הוספתו
                            int roleId;
                            string getRoleIdQuery = "SELECT RoleID FROM Roles WHERE RoleName = @RoleName";
                            using (SqlCommand command = new SqlCommand(getRoleIdQuery, connection, transaction))
                            {
                                command.Parameters.AddWithValue("@RoleName", roleName);
                                object result = command.ExecuteScalar();

                                if (result == null) // התפקיד לא קיים
                                {
                                    // יצירת תפקיד חדש
                                    string insertRoleQuery = "INSERT INTO Roles (RoleName) VALUES (@RoleName); SELECT CAST(SCOPE_IDENTITY() AS INT)";
                                    using (SqlCommand insertCommand = new SqlCommand(insertRoleQuery, connection, transaction))
                                    {
                                        insertCommand.Parameters.AddWithValue("@RoleName", roleName);
                                        roleId = (int)insertCommand.ExecuteScalar();
                                    }
                                }
                                else
                                {
                                    roleId = (int)result;
                                }
                            }

                            // הוספת התפקיד החדש לעובד
                            string insertEmployeeRoleQuery = "INSERT INTO EmployeeRoles (EmployeeID, RoleID) VALUES (@EmployeeID, @RoleID)";
                            using (SqlCommand command = new SqlCommand(insertEmployeeRoleQuery, connection, transaction))
                            {
                                command.Parameters.AddWithValue("@EmployeeID", selectedEmployee.ID);
                                command.Parameters.AddWithValue("@RoleID", roleId);
                                command.ExecuteNonQuery();
                            }
                        }

                        // עדכון סניפים - מחיקת כל השיוכים הקיימים והוספת החדשים
                        string deleteEmployeeBranchesQuery = "DELETE FROM EmployeeBranches WHERE EmployeeID = @EmployeeID";
                        using (SqlCommand command = new SqlCommand(deleteEmployeeBranchesQuery, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@EmployeeID", selectedEmployee.ID);
                            command.ExecuteNonQuery();
                        }

                        // הוספת הסניפים החדשים
                        List<string> newBranches = new List<string>();
                        foreach (Branch branch in branches)
                        {
                            newBranches.Add(branch.Name);

                            // הוספת שיוך עובד-סניף חדש
                            string insertEmployeeBranchQuery = "INSERT INTO EmployeeBranches (EmployeeID, BranchID) VALUES (@EmployeeID, @BranchID)";
                            using (SqlCommand insertCommand = new SqlCommand(insertEmployeeBranchQuery, connection, transaction))
                            {
                                insertCommand.Parameters.AddWithValue("@EmployeeID", selectedEmployee.ID);
                                insertCommand.Parameters.AddWithValue("@BranchID", branch.ID);
                                insertCommand.ExecuteNonQuery();
                            }
                        }

                        // עדכון משמרות מועדפות - מחיקת כל ההעדפות הקיימות והוספת החדשות
                        string deletePreferredShiftsQuery = "DELETE FROM EmployeePreferredShifts WHERE EmployeeID = @EmployeeID";
                        using (SqlCommand command = new SqlCommand(deletePreferredShiftsQuery, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@EmployeeID", selectedEmployee.ID);
                            command.ExecuteNonQuery();
                        }

                        // הוספת המשמרות המועדפות החדשות
                        HashSet<int> newPreferredShifts = new HashSet<int>();
                        foreach (ShiftDisplayInfo shift in shifts)
                        {
                            int shiftId = shift.ShiftID;
                            newPreferredShifts.Add(shiftId);

                            // הוספת שיוך עובד-משמרת מועדפת
                            string insertPreferredShiftQuery = "INSERT INTO EmployeePreferredShifts (EmployeeID, ShiftID) VALUES (@EmployeeID, @ShiftID)";
                            using (SqlCommand command = new SqlCommand(insertPreferredShiftQuery, connection, transaction))
                            {
                                command.Parameters.AddWithValue("@EmployeeID", selectedEmployee.ID);
                                command.Parameters.AddWithValue("@ShiftID", shiftId);
                                command.ExecuteNonQuery();
                            }
                        }

                        // עדכון האובייקט בזיכרון
                        selectedEmployee.Name = name;
                        selectedEmployee.roles = new HashSet<string>(roles);
                        selectedEmployee.HourlySalary = int.Parse(salary);
                        selectedEmployee.Rate = int.Parse(rate);
                        selectedEmployee.isMentor = isExperienced;
                        selectedEmployee.Branches = newBranches;
                        selectedEmployee.requestedShifts = newPreferredShifts;

                        // אישור העסקה
                        transaction.Commit();

                        // הצגת הודעת הצלחה
                        MessageBox.Show($"פרטי העובד {selectedEmployee.Name} עודכנו בהצלחה!", "הצלחה", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        // ביטול העסקה במקרה של שגיאה
                        transaction.Rollback();
                        throw new Exception("אירעה שגיאה בשמירת השינויים: " + ex.Message);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // הצגת הודעת שגיאה
            MessageBox.Show(ex.Message, "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
    }

    // פונקציה לטעינת מספר הטלפון של עובד מסוים
    // פרמטרים:
    // employeeID - מזהה העובד שאת מספר הטלפון שלו יש לטעון
    // ערך מוחזר: מספר הטלפון של העובד
    public string LoadEmployeePhone(int employeeID)
    {
        string phone = "";
        try
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                // שאילתה לקבלת מספר הטלפון של העובד
                string query = "SELECT Phone FROM Employees WHERE EmployeeID = @EmployeeID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // הוספת פרמטר מזהה העובד
                    command.Parameters.AddWithValue("@EmployeeID", employeeID);

                    // קריאת התוצאה
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // טיפול במקרה של ערך null
                            phone = reader["Phone"] != DBNull.Value ? reader["Phone"].ToString() : "";
                        }
                    }
                }
            }
            return phone;
        }
        catch (Exception ex)
        {
            // הצגת הודעת שגיאה במקרה של כישלון
            MessageBox.Show("אירעה שגיאה בטעינת נתוני העובד: " + ex.Message, "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return null;
        }
    }

    // פונקציה לטעינת כתובת הדוא"ל של עובד מסוים
    // פרמטרים:
    // employeeID - מזהה העובד שאת כתובת הדוא"ל שלו יש לטעון
    // ערך מוחזר: כתובת הדוא"ל של העובד
    public string LoadEmployeeEmail(int employeeID)
    {
        string Email = "";
        try
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                // שאילתה לקבלת כתובת הדוא"ל של העובד
                string query = "SELECT Email FROM Employees WHERE EmployeeID = @EmployeeID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // הוספת פרמטר מזהה העובד
                    command.Parameters.AddWithValue("@EmployeeID", employeeID);

                    // קריאת התוצאה
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // טיפול במקרה של ערך null
                            Email = reader["Email"] != DBNull.Value ? reader["Email"].ToString() : "";
                        }
                    }
                }
            }
            return Email;
        }
        catch (Exception ex)
        {
            // הצגת הודעת שגיאה במקרה של כישלון
            MessageBox.Show("אירעה שגיאה בטעינת נתוני העובד: " + ex.Message, "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return null;
        }
    }
}