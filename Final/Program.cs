using EmployeeSchedulingApp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using System.Xml.Linq;

namespace Final
{
    public class Program
    {
        private static string connectionString = "Data Source=(localdb)\\mssqllocaldb;Initial Catalog=EmployeeScheduling;Integrated Security=True";

        #region Data
        public static List<Employee> Employees{  get; set; }   
        public static List<Branch> Branches{  get; set; }  
        public static DB myDB = new DB();
        public const int ChromosomesEachGene = 100;

        public static Population pop = new Population(new List<Chromosome>(), ChromosomesEachGene);
        #endregion


        #region SQL

        
        // מתודה חדשה לטעינת הנתונים
        public static void LoadDataForUser(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                Employees = new List<Employee>();
                Branches = new List<Branch>();
                return;
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // טעינת הסניפים של המשתמש
                    Branches = LoadUserBranches(username, connection);

                    // טעינת העובדים של המשתמש
                    Employees = LoadUserEmployees(username, connection);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"שגיאה בטעינת נתוני המשתמש: {ex.Message}", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // יצירת רשימות ריקות במקרה של שגיאה
                Employees = new List<Employee>();
                Branches = new List<Branch>();
            }
        }

        // טעינת הסניפים של המשתמש
        private static List<Branch> LoadUserBranches(string username, SqlConnection connection)
        {
            List<Branch> branches = new List<Branch>();

            string query = @"
        SELECT b.BranchID, b.Name 
        FROM Branches b
        INNER JOIN UserBranches ub ON b.BranchID = ub.BranchID
        INNER JOIN Users u ON ub.UserID = u.UserID
        WHERE u.Username = @Username";

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Username", username);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int branchId = reader.GetInt32(0);
                        string branchName = reader.GetString(1);

                        Branch branch = new Branch
                        {
                            ID = branchId,
                            Name = branchName
                        };

                        branches.Add(branch);
                    }
                }
            }

            // טעינת המשמרות לכל סניף
            foreach (Branch branch in branches)
            {
                branch.Shifts = LoadBranchShifts(branch.ID, connection);
            }

            return branches;
        }

        // טעינת העובדים של המשתמש
        private static List<Employee> LoadUserEmployees(string username, SqlConnection connection)
        {
            List<Employee> employees = new List<Employee>();

            string query = @"
                     SELECT DISTINCT e.EmployeeID, e.Name, e.Phone, e.Email, e.HourlySalary, e.Rate, 
                    e.IsMentor, e.AssignedHours
                    FROM Employees e
                    INNER JOIN EmployeeBranches eb ON e.EmployeeID = eb.EmployeeID
                    INNER JOIN UserBranches ub ON eb.BranchID = ub.BranchID
                    INNER JOIN Users u ON ub.UserID = u.UserID
                    WHERE u.Username = @Username";

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Username", username);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int employeeId = reader.GetInt32(0);
                        string name = reader.GetString(1);
                        string phone = reader.IsDBNull(2) ? null : reader.GetString(2);
                        string email = reader.IsDBNull(3) ? null : reader.GetString(3);
                        decimal hourlySalary = reader.GetDecimal(4);
                        int rate = reader.IsDBNull(5) ? 0 : reader.GetInt32(5);
                        bool isMentor = reader.GetBoolean(6);
                        int assignedHours = reader.GetInt32(7);

                        // טעינת תפקידים והעדפות משמרות
                        List<string> roles = LoadEmployeeRoles(employeeId, connection);
                        HashSet<int> requestedShifts = LoadEmployeePreferredShifts(employeeId, connection);
                        List<string> branches = LoadEmployeeBranches(employeeId, connection);

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

                        employees.Add(employee);
                    }
                }
            }

            return employees;
        }
        // טעינת המשמרות של סניף
        private static List<Shift> LoadBranchShifts(int branchId, SqlConnection connection)
        {
            List<Shift> shifts = new List<Shift>();

            try
            {
                string query = @"
            SELECT s.ShiftID, s.TimeSlot, s.DayOfWeek, st.TypeName, s.IsBusy
            FROM Shifts s
            INNER JOIN ShiftTypes st ON s.ShiftTypeID = st.ShiftTypeID
            WHERE s.BranchID = @BranchID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@BranchID", branchId);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int shiftId = reader.GetInt32(0);
                            string timeSlot = reader.GetString(1);
                            string dayOfWeek = reader.GetString(2);
                            string shiftType = reader.GetString(3);
                            bool isBusy = reader.GetBoolean(4);

                            // טעינת דרישות תפקידים למשמרת
                            Dictionary<string, int> requiredRoles = LoadShiftRequiredRoles(shiftId, connection);

                            // יצירת אובייקט משמרת
                            Shift shift = new Shift(
                                shiftId,
                                "Branch " + branchId, // או להחליף עם שם סניף אמיתי
                                timeSlot,
                                dayOfWeek,
                                requiredRoles,
                                isBusy,
                                new Dictionary<string, List<Employee>>(),
                                shiftType
                            );

                            shifts.Add(shift);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading branch shifts: " + ex.Message);
            }

            return shifts;
        }

        // טעינת דרישות התפקידים למשמרת
        private static Dictionary<string, int> LoadShiftRequiredRoles(int shiftId, SqlConnection connection)
        {
            Dictionary<string, int> requiredRoles = new Dictionary<string, int>();

            try
            {
                // יצירת חיבור חדש כי אנחנו בתוך קורא נתונים
                using (SqlConnection newConnection = new SqlConnection(connection.ConnectionString))
                {
                    newConnection.Open();

                    string query = @"
                SELECT r.RoleName, sr.RequiredCount
                FROM ShiftRequiredRoles sr
                INNER JOIN Roles r ON sr.RoleID = r.RoleID
                WHERE sr.ShiftID = @ShiftID";

                    using (SqlCommand command = new SqlCommand(query, newConnection))
                    {
                        command.Parameters.AddWithValue("@ShiftID", shiftId);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string roleName = reader.GetString(0);
                                int requiredCount = reader.GetInt32(1);

                                requiredRoles[roleName] = requiredCount;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading shift required roles: " + ex.Message);
            }

            return requiredRoles;
        }

        // טעינת התפקידים של עובד
        private static List<string> LoadEmployeeRoles(int employeeId, SqlConnection connection)
        {
            List<string> roles = new List<string>();

            try
            {
                // יצירת חיבור חדש כי אנחנו בתוך קורא נתונים
                using (SqlConnection newConnection = new SqlConnection(connection.ConnectionString))
                {
                    newConnection.Open();

                    string query = @"
                SELECT r.RoleName
                FROM EmployeeRoles er
                INNER JOIN Roles r ON er.RoleID = r.RoleID
                WHERE er.EmployeeID = @EmployeeID";

                    using (SqlCommand command = new SqlCommand(query, newConnection))
                    {
                        command.Parameters.AddWithValue("@EmployeeID", employeeId);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                roles.Add(reader.GetString(0));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading employee roles: " + ex.Message);
            }

            return roles;
        }

        // טעינת המשמרות המועדפות של עובד
        private static HashSet<int> LoadEmployeePreferredShifts(int employeeId, SqlConnection connection)
        {
            HashSet<int> preferredShifts = new HashSet<int>();

            try
            {
                // יצירת חיבור חדש כי אנחנו בתוך קורא נתונים
                using (SqlConnection newConnection = new SqlConnection(connection.ConnectionString))
                {
                    newConnection.Open();

                    string query = @"
                SELECT ShiftID
                FROM EmployeePreferredShifts
                WHERE EmployeeID = @EmployeeID";

                    using (SqlCommand command = new SqlCommand(query, newConnection))
                    {
                        command.Parameters.AddWithValue("@EmployeeID", employeeId);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                preferredShifts.Add(reader.GetInt32(0));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading employee preferred shifts: " + ex.Message);
            }

            return preferredShifts;
        }

        // טעינת הסניפים של עובד
        private static List<string> LoadEmployeeBranches(int employeeId, SqlConnection connection)
        {
            List<string> branches = new List<string>();

            try
            {
                // יצירת חיבור חדש כי אנחנו בתוך קורא נתונים
                using (SqlConnection newConnection = new SqlConnection(connection.ConnectionString))
                {
                    newConnection.Open();

                    string query = @"
                SELECT b.Name
                FROM EmployeeBranches eb
                INNER JOIN Branches b ON eb.BranchID = b.BranchID
                WHERE eb.EmployeeID = @EmployeeID";

                    using (SqlCommand command = new SqlCommand(query, newConnection))
                    {
                        command.Parameters.AddWithValue("@EmployeeID", employeeId);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                branches.Add(reader.GetString(0));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading employee branches: " + ex.Message);
            }

            return branches;
        }

        // יתר המתודות העזר לטעינת המשמרות, התפקידים והעדפות המשמרות
        #endregion
        public static void createSceduele(string username)
        {
            LoadDataForUser(username);
            Console.WriteLine("Creating new schedule at: " + DateTime.Now);

            pop.Chromoshomes.Clear();
        

            pop = initializeFirstPopulation(pop);
     
            MessageBox.Show("סידור עבודה נוצר בהצלחה!", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public static Population initializeFirstPopulation(Population pop)
        {
            Random random = new Random();

            #region InLine
            Chromosome initializeChoromosome()
            {
                List<Employee> employeesSortedByRate = sort_employees_by_rate(Employees);
                List<Employee> employeesSortedByavailabilty = sort_employees_by_availabilty(Employees);
                Dictionary<int, List<Employee>> employeesMappedByRequestedShifts = mappingEmployeesByRequestedShifts();
                employeesMappedByRequestedShifts.OrderBy(pair => pair.Key);
                Dictionary<string, List<Employee>> employeesMappedByRequestedRoles = mappingEmployeesByRole();

                #region InLine
                List<Employee> sort_employees_by_availabilty(List<Employee> employees)
                {
                    return employees.OrderBy(e => e.requestedShifts?.Count ?? 0).ToList();//מבטיח שי לא יקרוס אם לא הוגשו משמרות
                }

                List<Employee> sort_employees_by_rate(List<Employee> employees)
                {
                    return employees.OrderByDescending(e => e.Rate).ToList();
                }

                Dictionary<int, List<Employee>> mappingEmployeesByRequestedShifts()
                {
                    return Employees
                        .SelectMany(emp => emp.requestedShifts, (emp, shiftId) => new { shiftId, emp })
                        .GroupBy(entry => entry.shiftId)
                        .ToDictionary(group => group.Key, group => group.Select(entry => entry.emp).ToList());
                }

                Dictionary<string, List<Employee>> mappingEmployeesByRole()
                {
                    return Employees
                        .SelectMany(emp => emp.roles, (emp, role) => new { role, emp }) // יצירת צמדים של תפקיד + עובד
                        .GroupBy(entry => entry.role) // קיבוץ לפי תפקיד
                        .ToDictionary(group => group.Key, group => group.Select(entry => entry.emp).ToList()); // המרה למילון
                }

                List<int> UpdateOverlappingShifts(Employee employee, Shift assignedShift)
                {
                    #region inLine
                     Shift FindShiftById(int shiftId)
                    {
                        // עבור כל סניף, בדוק את כל המשמרות
                        foreach (Branch branch in Program.Branches)
                        {
                            foreach (Shift shift in branch.Shifts)
                            {
                                if (shift.Id == shiftId)
                                {
                                    return shift;
                                }
                            }
                        }

                        return null;
                    }
                    #endregion
                    if (employee == null || assignedShift == null)
                        return null;

                    // רשימה לשמירת המזהים של המשמרות שנוסיר
                    List<int> idsToRemove = new List<int>();

                    // הסרת משמרות שחופפות באותו יום ושעה (זמן)
                    foreach (int shiftId in employee.requestedShifts)
                    {
                        // מציאת המשמרת המלאה מתוך הרשימה הכללית של המשמרות
                        Shift shift = FindShiftById(shiftId);

                        // אם מצאנו את המשמרת ויש לה את אותו יום ואותה שעה כמו המשמרת המשובצת
                        if (shift != null &&
                            shift.day == assignedShift.day &&
                            shift.TimeSlot == assignedShift.TimeSlot)
                        {
                            // הוספת המזהה לרשימת המשמרות להסרה
                            idsToRemove.Add(shiftId);
                        }
                    }
                    return idsToRemove;
                     }

                    List<Shift> fill_brach_shifts(Branch br)
                {

                    List<Shift> shuffledShifts = br.Shifts.OrderBy(x => random.Next()).ToList();


                    foreach (Shift sh in shuffledShifts)
                    {
                        List<Employee> currenList = new List<Employee>();

                        foreach (var entry in sh.RequiredRoles)
                        {
                            List<Employee> employeesAvaliableForShift;
                            employeesMappedByRequestedShifts.TryGetValue(sh.Id, out employeesAvaliableForShift);

                            for (int i = 0; i < entry.Value; i++)
                            {
                                employeesMappedByRequestedRoles.TryGetValue(entry.Key, out List<Employee> employeesAvaliableForRole);

                                int currentListIdentifier = random.Next(1, 3);//1-employeesSortedByRate //2-employeesSortedByavailabilty

                                switch (currentListIdentifier)
                                {
                                    case (1):
                                        currenList = employeesSortedByRate;
                                        break;
                                    case (2):
                                        currenList = employeesSortedByavailabilty;
                                        break;
                                }

                                if (employeesAvaliableForShift != null)
                                {
                                    Employee selectedEmployee = currenList.FirstOrDefault(emp => employeesAvaliableForShift.Contains(emp));

                                    if (selectedEmployee != null)
                                    {
                               
                                        if (!sh.AssignedEmployees.ContainsKey(entry.Key))
                                        {
                                            sh.AssignedEmployees[entry.Key] = new List<Employee>();
                                        }
                                        sh.AssignedEmployees[entry.Key].Add(selectedEmployee); 
                                        List<int> idToRemove = UpdateOverlappingShifts(selectedEmployee, sh);
                                        foreach (int id in idToRemove)
                                        {
                                            if (employeesMappedByRequestedShifts.TryGetValue(id, out List<Employee> shiftToRemove))
                                            {
                                                shiftToRemove.Remove(selectedEmployee);

                                                // אם הרשימה ריקה אחרי ההסרה, הסר אותה מהמיפוי כולו
                                                if (shiftToRemove.Count == 0)
                                                {
                                                    employeesMappedByRequestedShifts.Remove(id);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    return br.Shifts;
                }
                #endregion

                Chromosome ch = new Chromosome();
                List<Branch> branchesCopy = new List<Branch>();
                foreach (Branch originalBranch in Branches)
                {

                    List<Shift> shiftsCopy = new List<Shift>();
                    foreach (Shift originalShift in originalBranch.Shifts)
                    {
                        Shift shiftCopy = new Shift
                        {
                            Id = originalShift.Id,
                            day = originalShift.day,
                            TimeSlot = originalShift.TimeSlot,
                            RequiredRoles = new Dictionary<string, int>(originalShift.RequiredRoles),
                            AssignedEmployees = new Dictionary<string, List<Employee>>()
                        };
                        shiftsCopy.Add(shiftCopy); 
                    }
                    
                    Branch branchCopy = new Branch
                    {
                        ID = originalBranch.ID,
                        Name = originalBranch.Name,
                        Shifts= shiftsCopy
                    };
                    branchesCopy.Add(branchCopy);


                }

                List<Branch> shuffledBranches = branchesCopy.OrderBy(x => random.Next()).ToList();

                foreach (Branch br in shuffledBranches)
                {
                    br.Shifts = fill_brach_shifts(br);
                    ch.Shifts.Add(br.Name, br.Shifts);
                }

                return ch;
            }
            #endregion

            for (int i = 0; i < 10; i++)
            {
                foreach (Employee emp in Employees)
                {
                    emp.requestedShifts.Clear();
                    foreach (int id in emp.backUprequestedShifts)
                    {
                        emp.requestedShifts.Add(id);
                    }
                }
                Chromosome c = new Chromosome();
                c = initializeChoromosome();
                pop.Chromoshomes.Add(c);
                c.Fitness=calaulateChoromosomeFitness(c);



            }
            return pop;
        }



        public static double calaulateChoromosomeFitness(Chromosome ch)
        {
            double currentChromosomeFitness = 0;
            #region inLine
            double calculatesShiftFitness(Shift shift)
            {
                double fitness = 0;

                // ודא שהמילון קיים
                if (shift.AssignedEmployees == null)
                {
                    shift.AssignedEmployees = new Dictionary<string, List<Employee>>();
                }

                // 1. ניקוד על שיבוץ נכון של תפקידים
                foreach (var roleEntry in shift.RequiredRoles)
                {
                    string role = roleEntry.Key;
                    int requiredCount = roleEntry.Value;

                    // בדוק אם התפקיד קיים במילון ויש מספיק עובדים
                    if (shift.AssignedEmployees.ContainsKey(role) &&
                        shift.AssignedEmployees[role] != null &&
                        shift.AssignedEmployees[role].Count >= requiredCount)
                    {
                        fitness += 10;
                    }
                    else
                    {
                        fitness -= 10;
                    }
                }

                // 2. ניקוד על התאמה למשמרות ולסניפים
                foreach (var roleEntry in shift.AssignedEmployees)
                {
                    string role = roleEntry.Key;
                    List<Employee> employees = roleEntry.Value;

                    if (employees != null)
                    {
                        foreach (var emp in employees)
                        {
                            if (emp != null && emp.backUprequestedShifts != null &&
                                emp.backUprequestedShifts.Contains(shift.Id))
                            {
                                fitness += 3;
                            }
                            else
                            {
                                fitness -= 5;
                            }
                        }
                    }
                }

                // 3. ניקוד על נוכחות עובד מנוסה (Mentor)
                bool hasMentor = false;
                foreach (var employeeList in shift.AssignedEmployees.Values)
                {
                    if (employeeList != null && employeeList.Any(emp => emp != null && emp.isMentor))
                    {
                        hasMentor = true;
                        break;
                    }
                }
                fitness += hasMentor ? 5 : -5;

                // 4. חישוב עלות שכר והפחתת ניקוד
                double salaryExpense = 0;
                foreach (var employeeList in shift.AssignedEmployees.Values)
                {
                    if (employeeList != null)
                    {
                        foreach (var emp in employeeList)
                        {
                            if (emp != null)
                            {
                                salaryExpense += (emp.HourlySalary * emp.AssignedHours) / 100.0;
                            }
                        }
                    }
                }
                fitness -= salaryExpense;

                return fitness;
            }
            #endregion

            // חישוב הפיטנס לכל המשמרות בכל הסניפים
            if (ch.Shifts != null)
            {
                foreach (var branchEntry in ch.Shifts)
                {
                    string branchName = branchEntry.Key;
                    List<Shift> branchShifts = branchEntry.Value;

                    if (branchShifts != null)
                    {
                        foreach (var shift in branchShifts)
                        {
                            if (shift != null)
                            {
                                // ודא שהמילון קיים לפני החישוב
                                if (shift.AssignedEmployees == null)
                                {
                                    shift.AssignedEmployees = new Dictionary<string, List<Employee>>();
                                }

                                // חשב את הפיטנס רק למשמרות שיש להן עובדים משובצים
                                if (shift.AssignedEmployees.Count > 0)
                                {
                                    currentChromosomeFitness += calculatesShiftFitness(shift);
                                }
                            }
                        }
                    }
                }
            }

            // בונוס/קנס עבור אילוצים ברמת הכרומוזום
            int unassignedShifts = 0;

            if (ch.Shifts != null)
            {
                foreach (var branchEntry in ch.Shifts)
                {
                    List<Shift> branchShifts = branchEntry.Value;

                    if (branchShifts != null)
                    {
                        foreach (var shift in branchShifts)
                        {
                            if (shift != null && shift.RequiredRoles != null)
                            {
                                bool allRolesCovered = true;

                                foreach (var roleEntry in shift.RequiredRoles)
                                {
                                    string role = roleEntry.Key;
                                    int requiredCount = roleEntry.Value;

                                    if (shift.AssignedEmployees == null ||
                                        !shift.AssignedEmployees.ContainsKey(role) ||
                                        shift.AssignedEmployees[role] == null ||
                                        shift.AssignedEmployees[role].Count < requiredCount)
                                    {
                                        allRolesCovered = false;
                                        break;
                                    }
                                }

                                if (!allRolesCovered)
                                {
                                    unassignedShifts++;
                                }
                            }
                        }
                    }
                }
            }

            currentChromosomeFitness -= unassignedShifts * 20; // קנס עבור כל משמרת לא מכוסה

            // אפשרות להוסיף מדדי פיטנס נוספים ברמת הכרומוזום
            // למשל, בדיקה שעובד לא עובד יותר מדי משמרות ברצף

            return currentChromosomeFitness;
        }
        
        public static Chromosome GetBestChromosome()
        {
            return pop.Chromoshomes.OrderByDescending(ch => ch.Fitness).FirstOrDefault();
        }

        
        static void Main()
        {
            Branches = DB.addBranches();
            Employees = DB.addEmployees();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.Run(new HomePage());
        }
    }
}