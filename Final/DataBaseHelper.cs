using Final;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TreeView;
using System.Windows.Forms;

public class DataBaseHelper
{
    private static string connectionString = "Data Source=(localdb)\\mssqllocaldb;Initial Catalog=EmployeeScheduling;Integrated Security=True;MultipleActiveResultSets=True";
  

    //טעינת כל המידע של המשתמש המחובר למערכת
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
                List<Branch> loadedBranches = LoadUserBranches(username, connection);
                List<Employee> loadedEmployees = LoadUserEmployees(username, connection);
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


    #region LoadDataForUser Helper Functions
    //פונקציה הטוענת את הסניפים של המשתמש הנוכחי מבסיס הנתונים
    public  List<Branch> LoadUserBranches(string username, SqlConnection connection)
    {
        //אתחול רשימת סניפים חדשה
        List<Branch> branches = new List<Branch>();
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
            branch.Shifts = LoadBranchShifts(branch.ID, connection);
        }

        return branches;
    }

    //פונקציה הטוענת את העובדים של המשתמש הנוכחי מבסיס הנתונים
    public  List<Employee> LoadUserEmployees(string username, SqlConnection connection)
    {
        //אתחול רשימת עובדים חדשה
        List<Employee> employees = new List<Employee>();

        //שאילתה השולפת מבסיס הנתונים את כל הנתונים הרלוונטים על העובד של המשתמש הנוכחי
        string query = @"
            SELECT DISTINCT e.EmployeeID, e.Name, e.Phone, e.Email, e.HourlySalary, e.Rate, 
           e.IsMentor, e.AssignedHours
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
                    int assignedHours = reader.GetInt32(7);

                    //בעבור כל עובד נטען את התפקידים שלו
                    List<string> roles = LoadEmployeeRoles(employeeId, connection);
                    //בעבור כל עובד נטען את המשמרות שהוא יכול לעבוד בהן
                    HashSet<int> requestedShifts = LoadEmployeePreferredShifts(employeeId, connection);
                    //בעבור כל עובד נטען את הסניפים שלו
                    List<string> branches = LoadEmployeeBranches(employeeId, connection);

                    //יצירת אובייקט עובד חדש עם כל המידע
                    Employee employee = new Employee(
                        employeeId,
                        name,
                        roles,
                        requestedShifts,
                        rate,
                        (int)hourlySalary,
                        assignedHours,
                        isMentor,
                        branches
                    );
                    //הוספת העובד לרשימת העובדים בתוכנית
                    employees.Add(employee);
                }
            }
        }

        return employees;
    }

    //פונקציה הטוענת את המשמרות של סניף מבסיס הנתונים
    private List<Shift> LoadBranchShifts(int branchId, SqlConnection connection)
    {
        //אתחול רשימת משמרות חדשה
        List<Shift> shifts = new List<Shift>();

        try
        {
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
                        Dictionary<string, int> requiredRoles = LoadShiftRequiredRoles(shiftId, connection);

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
        //הדפסת שגיאה במקרה שהייתה בעיה בטעינת המשמרות
        catch (Exception ex)
        {
            Console.WriteLine("Error loading branch shifts: " + ex.Message);
        }

        return shifts;
    }

    //פונקציה הטוענת את התפקידים שמבוקשים של משמרת מבסיס הנתונים
    private Dictionary<string, int> LoadShiftRequiredRoles(int shiftId, SqlConnection connection)
    {
        //אתחול מילון תפקידים נדרשים
        Dictionary<string, int> requiredRoles = new Dictionary<string, int>();

        try
        {
            //יצירת חיבור לבסיס הנתונים
            using (SqlConnection newConnection = new SqlConnection(connection.ConnectionString))
            {
                newConnection.Open();

                //שאילתה השולפת מבסיס הנתונים מידע על התפקידים הנדרשים למשמרת
                string query = @"
                    SELECT r.RoleName, sr.RequiredCount
                    FROM ShiftRequiredRoles sr
                    INNER JOIN Roles r ON sr.RoleID = r.RoleID
                    WHERE sr.ShiftID = @ShiftID";

                //אתחול השאילתה
                using (SqlCommand command = new SqlCommand(query, newConnection))
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

    //פונקציה הטוענת את התפקידים של כל עובד מבסיס הנתונים
    private List<string> LoadEmployeeRoles(int employeeId, SqlConnection connection)
    {
        //אתחול רשימת תפקידים חדשה
        List<string> roles = new List<string>();

        try
        {
            //יצירת חיבור 
            using (SqlConnection newConnection = new SqlConnection(connection.ConnectionString))
            {
                newConnection.Open();

                //ששאילתה השולפת מבסיס הנתונים את תפקידי העובד
                string query = @"
                SELECT r.RoleName
                FROM EmployeeRoles er
                INNER JOIN Roles r ON er.RoleID = r.RoleID
                WHERE er.EmployeeID = @EmployeeID";

                //אתחול השאילתה
                using (SqlCommand command = new SqlCommand(query, newConnection))
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

    //פונקציה הטוענת את המשמרות המבוקשות של כל עובד מבסיס הנתונים
    private  HashSet<int> LoadEmployeePreferredShifts(int employeeId, SqlConnection connection)
    {
        //אתחול האש סט משמרות מועדפות חדש
        HashSet<int> preferredShifts = new HashSet<int>();

        try
        {
            //יצירת חיבור לבסיס הנתונים
            using (SqlConnection newConnection = new SqlConnection(connection.ConnectionString))
            {
                newConnection.Open();

                //שאילתה השולפת מבסיס הנתונים את המשמרות המועדפות של העובד
                string query = @"
                SELECT ShiftID
                FROM EmployeePreferredShifts
                WHERE EmployeeID = @EmployeeID";

                //אתחול השאילתה
                using (SqlCommand command = new SqlCommand(query, newConnection))
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

    //שאילתה הטוענת את הנתונים של כל עובד מבסיס הנתונים
    private  List<string> LoadEmployeeBranches(int employeeId, SqlConnection connection)
    {
        //אתחול רשימת סניפים חדשה
        List<string> branches = new List<string>();

        try
        {
            //יצירת חיבור לבסיס הנתונים
            using (SqlConnection newConnection = new SqlConnection(connection.ConnectionString))
            {
                newConnection.Open();

                //שאילתה השולפת מבסיס הנתונים את הסניפים שהעובד עובד בהם
                string query = @"
                SELECT b.Name
                FROM EmployeeBranches eb
                INNER JOIN Branches b ON eb.BranchID = b.BranchID
                WHERE eb.EmployeeID = @EmployeeID";

                //אתחול השאילתה
                using (SqlCommand command = new SqlCommand(query, newConnection))
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
    #endregion
    //פונקציה המוחקת סניף מבסיס הנתונים
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

    //פונקציה המוחקת עובד מבסיס הנתונים
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

    // פונקציית עזר לקבלת מזהה משתמש לפי שם משתמש
    public int GetUserIdByUsername(string username, SqlConnection connection)
    {
        try
        {
            string query = "SELECT UserID FROM Users WHERE Username = @Username";
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Username", username);
                object result = command.ExecuteScalar();

                if (result != null && result != DBNull.Value)
                {
                    return Convert.ToInt32(result);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"שגיאה בקבלת מזהה משתמש: {ex.Message}");
        }

        return -1;
    }

    // פונקציית עזר לקבלת מזהה סוג משמרת (או יצירת סוג חדש אם אינו קיים)
    public int GetOrCreateShiftType(string typeName, SqlConnection connection)
    {
        try
        {
            // בדיקה אם סוג המשמרת כבר קיים
            string query = "SELECT ShiftTypeID FROM ShiftTypes WHERE TypeName = @TypeName";
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@TypeName", typeName);
                object result = command.ExecuteScalar();

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
                command.Parameters.AddWithValue("@TypeName", typeName);
                return (int)command.ExecuteScalar();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"שגיאה בקבלת/יצירת סוג משמרת: {ex.Message}");
            return -1;
        }
    }

    // פונקציית עזר להוספת משמרת חדשה
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
                command.Parameters.AddWithValue("@BranchID", branchId);
                command.Parameters.AddWithValue("@TimeSlot", timeSlot);
                command.Parameters.AddWithValue("@DayOfWeek", dayOfWeek);
                command.Parameters.AddWithValue("@ShiftTypeID", shiftTypeId);

                shiftId = (int)command.ExecuteScalar();
                Console.WriteLine($"נוספה משמרת חדשה: {dayOfWeek}, {timeSlot}, ID: {shiftId}");
            }

            // הוספת דרישות תפקידים בסיסיות למשמרת
            Dictionary<string, int> defaultRoles = new Dictionary<string, int>
                {
                    { "Waiter", 2 },
                    { "Chef", 1 },
                    { "Bartender", 1 },
                    { "Manager", 1 }
                };

            foreach (var role in defaultRoles)
            {
                int roleId = GetOrCreateRole(role.Key, connection);
                if (roleId > 0)
                {
                    AddShiftRequiredRole(shiftId, roleId, role.Value, connection);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"שגיאה בהוספת משמרת: {ex.Message}");
        }
    }

    // פונקציית עזר לקבלת מזהה תפקיד (או יצירת תפקיד חדש אם אינו קיים)
    public int GetOrCreateRole(string roleName, SqlConnection connection)
    {
        try
        {
            // בדיקה אם התפקיד כבר קיים
            string query = "SELECT RoleID FROM Roles WHERE RoleName = @RoleName";
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@RoleName", roleName);
                object result = command.ExecuteScalar();

                if (result != null && result != DBNull.Value)
                {
                    return Convert.ToInt32(result);
                }
            }

            // אם לא קיים, יצירת תפקיד חדש
            string insertQuery = @"
                    INSERT INTO Roles (RoleName)
                    VALUES (@RoleName);
                    SELECT CAST(SCOPE_IDENTITY() AS INT)";

            using (SqlCommand command = new SqlCommand(insertQuery, connection))
            {
                command.Parameters.AddWithValue("@RoleName", roleName);
                return (int)command.ExecuteScalar();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"שגיאה בקבלת/יצירת תפקיד: {ex.Message}");
            return -1;
        }
    }

    // פונקציית עזר להוספת דרישת תפקיד למשמרת
    public void AddShiftRequiredRole(int shiftId, int roleId, int requiredCount, SqlConnection connection)
    {
        try
        {
            string insertQuery = @"
                    INSERT INTO ShiftRequiredRoles (ShiftID, RoleID, RequiredCount)
                    VALUES (@ShiftID, @RoleID, @RequiredCount)";

            using (SqlCommand command = new SqlCommand(insertQuery, connection))
            {
                command.Parameters.AddWithValue("@ShiftID", shiftId);
                command.Parameters.AddWithValue("@RoleID", roleId);
                command.Parameters.AddWithValue("@RequiredCount", requiredCount);
                command.ExecuteNonQuery();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"שגיאה בהוספת דרישת תפקיד: {ex.Message}");
        }
    }





}