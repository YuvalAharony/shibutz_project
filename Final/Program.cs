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
        public const int ChromosomesEachGene = 500;
        public const int Genes = 500;


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
            SELECT s.ShiftID,ts.TimeSlotName, s.DayOfWeek, st.TypeName, s.IsBusy
            FROM Shifts s
            INNER JOIN ShiftTypes st ON s.ShiftTypeID = st.ShiftTypeID
            INNER JOIN TimeSlots ts on ts.TimeSlotID=s.TimeSlotID
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
           

            pop.Chromoshomes.Clear();
        

            pop = initializeFirstPopulation(pop);
            pop.Chromoshomes = pop.Chromoshomes.OrderByDescending(x => x.Fitness).Take(ChromosomesEachGene).ToList();

            for (int i = 0; i < Genes; i++)
            {
                crossover(pop);
                Mutation(pop);
                // תיקון - השמה והפיכת הסדר כי אנחנו רוצים את הערכים הגבוהים
                pop.Chromoshomes = pop.Chromoshomes.OrderByDescending(x => x.Fitness).Take(ChromosomesEachGene).ToList();
            }
            MessageBox.Show("נוצר בהצלחה", "הצלחה", MessageBoxButtons.OK, MessageBoxIcon.Information);

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
                                    Employee selectedEmployee = currenList.FirstOrDefault(emp =>
                                    employeesAvaliableForShift.Contains(emp) && employeesAvaliableForRole.Contains(emp));


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

            for (int i = 0; i < ChromosomesEachGene; i++)
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

        public static void Mutation(Population pop)
        {
            Random random = new Random();
            List<Chromosome> newChromosomes = new List<Chromosome>();

            // עבור על כל הכרומוזומים באוכלוסייה
            foreach (Chromosome chromosome in pop.Chromoshomes)
            {
                // יצירת העתק עמוק של הכרומוזום לפני המוטציה
                Chromosome mutatedChromosome = DeepCopyChromosome(chromosome);
                bool wasMutated = false;

                // עבור על כל סניף בכרומוזום
                foreach (var branchEntry in mutatedChromosome.Shifts)
                {
                    string branchName = branchEntry.Key;
                    List<Shift> shifts = branchEntry.Value;

                    // בחר משמרת אקראית לשינוי
                    if (shifts != null && shifts.Count > 0)
                    {
                        int shiftIndex = random.Next(shifts.Count);
                        Shift shiftToMutate = shifts[shiftIndex];

                        // וודא שיש לפחות עובד אחד במשמרת
                        if (shiftToMutate.AssignedEmployees != null && shiftToMutate.AssignedEmployees.Count > 0)
                        {
                            // בחר תפקיד אקראי
                            var roles = shiftToMutate.AssignedEmployees.Keys.ToList();
                            if (roles.Count > 0)
                            {
                                string role = roles[random.Next(roles.Count)];

                                // וודא שיש לפחות עובד אחד בתפקיד הזה
                                if (shiftToMutate.AssignedEmployees[role] != null &&
                                    shiftToMutate.AssignedEmployees[role].Count > 0)
                                {
                                    // בחר עובד אקראי להחלפה
                                    int employeeIndex = random.Next(shiftToMutate.AssignedEmployees[role].Count);
                                    Employee employeeToReplace = shiftToMutate.AssignedEmployees[role][employeeIndex];

                                    // רשימת העובדים שכבר משובצים במשמרת הזו (בכל התפקידים)
                                    HashSet<Employee> employeesInShift = new HashSet<Employee>();
                                    foreach (var roleEntry in shiftToMutate.AssignedEmployees)
                                    {
                                        foreach (Employee emp in roleEntry.Value)
                                        {
                                            employeesInShift.Add(emp);
                                        }
                                    }

                                    // מצא עובד חלופי שיכול לבצע את התפקיד הזה ולא כבר משובץ במשמרת
                                    List<Employee> potentialReplacements = Program.Employees
                                        .Where(e => e.roles.Contains(role) &&
                                               !employeesInShift.Contains(e))
                                        .ToList();

                                    if (potentialReplacements.Count > 0)
                                    {
                                        // החלף את העובד בעובד אקראי מהרשימה של המחליפים הפוטנציאליים
                                        shiftToMutate.AssignedEmployees[role][employeeIndex] =
                                            potentialReplacements[random.Next(potentialReplacements.Count)];

                                        wasMutated = true;
                                    }
                                }
                            }
                        }
                    }
                }

                // אם בוצעה מוטציה, חשב את ערך הפיטנס החדש והוסף לאוכלוסייה
                if (wasMutated)
                {
                    mutatedChromosome.Fitness = Program.calaulateChoromosomeFitness(mutatedChromosome);
                    newChromosomes.Add(mutatedChromosome);
                }
            }

            // הוסף את הכרומוזומים החדשים לאוכלוסייה
            foreach (var newChromosome in newChromosomes)
            {
                pop.Chromoshomes.Add(newChromosome);
            }

            // מיין את האוכלוסייה לפי ערך הפיטנס וקח את החזקים ביותר
            pop.Chromoshomes = pop.Chromoshomes
                .OrderByDescending(ch => ch.Fitness)
                .Take(Program.ChromosomesEachGene)
                .ToList();
        }
        public static void crossover(Population pop)
        {
            Random random = new Random();
            List<Chromosome> newOffspring = new List<Chromosome>();

            // הגדלת מספר הצאצאים
            int desiredOffspringCount = Program.ChromosomesEachGene * 3 / 4; // 75% מהאוכלוסייה

            for (int i = 0; i < desiredOffspringCount; i++)
            {
                // בחירת הורים טובים יותר - לקיחת ה-10% העליונים
                var bestParents = pop.Chromoshomes
                    .OrderByDescending(ch => ch.Fitness)
                    .Take(pop.Chromoshomes.Count / 10)
                    .ToList();

                // בחירת הורים בסיכוי יחסי לפיטנס
                Chromosome parent1 = SelectParentByRouletteWheel(bestParents.Count > 0 ? bestParents : pop.Chromoshomes);
                Chromosome parent2 = SelectParentByRouletteWheel(bestParents.Count > 0 ? bestParents : pop.Chromoshomes);

                // וידוא הורים שונים
                int attempts = 0;
                while (parent1 == parent2 && attempts < 10)
                {
                    parent2 = SelectParentByRouletteWheel(bestParents.Count > 0 ? bestParents : pop.Chromoshomes);
                    attempts++;
                }

                // גיוון טכניקות הצלבה
                int crossoverType = random.Next(3);
                Chromosome offspring;

                switch (crossoverType)
                {
                    case 0:
                        offspring = PerformCrossover(parent1, parent2); // הצלבה רגילה
                        break;

                    case 1:
                        offspring = PerformUniformCrossover(parent1, parent2); // הצלבה אחידה
                        break;

                    default:
                        offspring = PerformMultiPointCrossover(parent1, parent2); // הצלבה רב-נקודתית
                        break;
                }

                // חישוב פיטנס והוספה
                offspring.Fitness = Program.calaulateChoromosomeFitness(offspring);
                newOffspring.Add(offspring);
            }

            // הוספת הצאצאים לאוכלוסייה
            foreach (var offspring in newOffspring)
            {
                pop.Chromoshomes.Add(offspring);
            }

            // מיון ושמירת הטובים ביותר
            pop.Chromoshomes = pop.Chromoshomes
                .OrderByDescending(ch => ch.Fitness)
                .Take(Program.ChromosomesEachGene)
                .ToList();
        }

        // הצלבה אחידה - שילוב אקראי של תכונות משני ההורים
        private static Chromosome PerformUniformCrossover(Chromosome parent1, Chromosome parent2)
        {
            Random random = new Random();
            Chromosome offspring = new Chromosome();
            offspring.Shifts = new Dictionary<string, List<Shift>>();

            // איחוד מפתחות הסניפים מההורים
            var allBranchNames = new HashSet<string>(parent1.Shifts.Keys.Concat(parent2.Shifts.Keys));

            foreach (string branchName in allBranchNames)
            {
                // אם הסניף קיים רק באחד ההורים, העתק אותו כמו שהוא
                if (!parent1.Shifts.ContainsKey(branchName))
                {
                    offspring.Shifts[branchName] = DeepCopyShifts(parent2.Shifts[branchName]);
                    continue;
                }

                if (!parent2.Shifts.ContainsKey(branchName))
                {
                    offspring.Shifts[branchName] = DeepCopyShifts(parent1.Shifts[branchName]);
                    continue;
                }

                // הסניף קיים בשני ההורים - בצע הצלבה אחידה ברמת המשמרת
                List<Shift> parent1Shifts = parent1.Shifts[branchName];
                List<Shift> parent2Shifts = parent2.Shifts[branchName];
                List<Shift> offspringShifts = new List<Shift>();

                // צלול עמוק יותר - בצע הצלבה ברמת המשמרות
                int shiftsCount = Math.Min(parent1Shifts.Count, parent2Shifts.Count);

                for (int i = 0; i < shiftsCount; i++)
                {
                    Shift shift1 = parent1Shifts[i];
                    Shift shift2 = parent2Shifts[i];

                    // וודא תאימות בין המשמרות (אותו יום, אותה שעה)
                    if (shift1.day == shift2.day && shift1.TimeSlot == shift2.TimeSlot)
                    {
                        // יצירת משמרת בסיסית
                        Shift offspringShift = new Shift
                        {
                            Id = shift1.Id,
                            branch = shift1.branch,
                            day = shift1.day,
                            TimeSlot = shift1.TimeSlot,
                            IsBusy = shift1.IsBusy,
                            EventType = shift1.EventType,
                            RequiredRoles = new Dictionary<string, int>(shift1.RequiredRoles),
                            AssignedEmployees = new Dictionary<string, List<Employee>>()
                        };

                        // איחוד תפקידים משני ההורים
                        var allRoles = new HashSet<string>(
                            shift1.AssignedEmployees.Keys.Concat(shift2.AssignedEmployees.Keys));

                        foreach (string role in allRoles)
                        {
                            offspringShift.AssignedEmployees[role] = new List<Employee>();

                            // העתקת עובדים מההורה הראשון או השני
                            if (shift1.AssignedEmployees.ContainsKey(role) &&
                                shift2.AssignedEmployees.ContainsKey(role))
                            {
                                var employees1 = shift1.AssignedEmployees[role];
                                var employees2 = shift2.AssignedEmployees[role];

                                // בחירה אקראית בין העובדים של שני ההורים
                                for (int j = 0; j < Math.Max(employees1.Count, employees2.Count); j++)
                                {
                                    if (random.Next(2) == 0 && j < employees1.Count)
                                    {
                                        if (!offspringShift.AssignedEmployees[role].Contains(employees1[j]))
                                            offspringShift.AssignedEmployees[role].Add(employees1[j]);
                                    }
                                    else if (j < employees2.Count)
                                    {
                                        if (!offspringShift.AssignedEmployees[role].Contains(employees2[j]))
                                            offspringShift.AssignedEmployees[role].Add(employees2[j]);
                                    }
                                }
                            }
                            else if (shift1.AssignedEmployees.ContainsKey(role))
                            {
                                offspringShift.AssignedEmployees[role] = new List<Employee>(shift1.AssignedEmployees[role]);
                            }
                            else if (shift2.AssignedEmployees.ContainsKey(role))
                            {
                                offspringShift.AssignedEmployees[role] = new List<Employee>(shift2.AssignedEmployees[role]);
                            }
                        }

                        offspringShifts.Add(offspringShift);
                    }
                    else
                    {
                        // המשמרות אינן תואמות - העתק אחת מהן באקראי
                        offspringShifts.Add(DeepCopyShift(random.Next(2) == 0 ? shift1 : shift2));
                    }
                }

                // טיפול במקרה שלאחד ההורים יש יותר משמרות
                if (parent1Shifts.Count > shiftsCount)
                {
                    for (int i = shiftsCount; i < parent1Shifts.Count; i++)
                    {
                        offspringShifts.Add(DeepCopyShift(parent1Shifts[i]));
                    }
                }
                else if (parent2Shifts.Count > shiftsCount)
                {
                    for (int i = shiftsCount; i < parent2Shifts.Count; i++)
                    {
                        offspringShifts.Add(DeepCopyShift(parent2Shifts[i]));
                    }
                }

                offspring.Shifts[branchName] = offspringShifts;
            }

            return offspring;
        }

        // הצלבה רב-נקודתית
        private static Chromosome PerformMultiPointCrossover(Chromosome parent1, Chromosome parent2)
        {
            Random random = new Random();
            Chromosome offspring = new Chromosome();
            offspring.Shifts = new Dictionary<string, List<Shift>>();

            // עבור על הסניפים המשותפים
            var commonBranches = parent1.Shifts.Keys.Intersect(parent2.Shifts.Keys).ToList();

            foreach (string branchName in commonBranches)
            {
                List<Shift> parent1Shifts = parent1.Shifts[branchName];
                List<Shift> parent2Shifts = parent2.Shifts[branchName];
                List<Shift> offspringShifts = new List<Shift>();

                int shiftsCount = Math.Min(parent1Shifts.Count, parent2Shifts.Count);

                // יצירת 1-3 נקודות חיתוך אקראיות
                int numCrossoverPoints = random.Next(1, Math.Min(4, shiftsCount));
                List<int> crossoverPoints = new List<int>();

                for (int i = 0; i < numCrossoverPoints; i++)
                {
                    int point = random.Next(1, shiftsCount);

                    if (!crossoverPoints.Contains(point))
                        crossoverPoints.Add(point);
                }

                crossoverPoints.Sort();

                // ביצוע הצלבה לפי נקודות החיתוך
                bool useParent1 = true;
                int currentIndex = 0;

                for (int i = 0; i < shiftsCount; i++)
                {
                    if (crossoverPoints.Contains(i))
                    {
                        useParent1 = !useParent1;
                    }

                    if (useParent1)
                    {
                        offspringShifts.Add(DeepCopyShift(parent1Shifts[i]));
                    }
                    else
                    {
                        offspringShifts.Add(DeepCopyShift(parent2Shifts[i]));
                    }
                }

                // טיפול במשמרות נוספות
                if (parent1Shifts.Count > shiftsCount)
                {
                    for (int i = shiftsCount; i < parent1Shifts.Count; i++)
                    {
                        offspringShifts.Add(DeepCopyShift(parent1Shifts[i]));
                    }
                }
                else if (parent2Shifts.Count > shiftsCount)
                {
                    for (int i = shiftsCount; i < parent2Shifts.Count; i++)
                    {
                        offspringShifts.Add(DeepCopyShift(parent2Shifts[i]));
                    }
                }

                offspring.Shifts[branchName] = offspringShifts;
            }

            // טיפול בסניפים ייחודיים
            foreach (string branchName in parent1.Shifts.Keys.Except(commonBranches))
            {
                offspring.Shifts[branchName] = DeepCopyShifts(parent1.Shifts[branchName]);
            }

            foreach (string branchName in parent2.Shifts.Keys.Except(commonBranches))
            {
                offspring.Shifts[branchName] = DeepCopyShifts(parent2.Shifts[branchName]);
            }

            return offspring;
        }

        // פונקציות עזר
        private static Shift DeepCopyShift(Shift originalShift)
        {
            Shift copy = new Shift
            {
                Id = originalShift.Id,
                branch = originalShift.branch,
                day = originalShift.day,
                TimeSlot = originalShift.TimeSlot,
                IsBusy = originalShift.IsBusy,
                EventType = originalShift.EventType,
                RequiredRoles = new Dictionary<string, int>(originalShift.RequiredRoles),
                AssignedEmployees = new Dictionary<string, List<Employee>>()
            };

            foreach (var roleEntry in originalShift.AssignedEmployees)
            {
                copy.AssignedEmployees[roleEntry.Key] = new List<Employee>(roleEntry.Value);
            }

            return copy;
        }

        private static List<Shift> DeepCopyShifts(List<Shift> shifts)
        {
            List<Shift> copies = new List<Shift>();

            foreach (var shift in shifts)
            {
                copies.Add(DeepCopyShift(shift));
            }

            return copies;
        }

        // פונקציית עזר ליצירת העתק עמוק של כרומוזום
        private static Chromosome DeepCopyChromosome(Chromosome original)
        {
            Chromosome copy = new Chromosome();
            copy.Fitness = original.Fitness;
            copy.Shifts = new Dictionary<string, List<Shift>>();

            foreach (var branchEntry in original.Shifts)
            {
                string branchName = branchEntry.Key;
                List<Shift> originalShifts = branchEntry.Value;
                List<Shift> copiedShifts = new List<Shift>();

                foreach (Shift originalShift in originalShifts)
                {
                    Shift shiftCopy = new Shift
                    {
                        Id = originalShift.Id,
                        branch = originalShift.branch,
                        day = originalShift.day,
                        TimeSlot = originalShift.TimeSlot,
                        IsBusy = originalShift.IsBusy,
                        EventType = originalShift.EventType,
                        RequiredRoles = new Dictionary<string, int>(originalShift.RequiredRoles),
                        AssignedEmployees = new Dictionary<string, List<Employee>>()
                    };

                    foreach (var roleEntry in originalShift.AssignedEmployees)
                    {
                        string role = roleEntry.Key;
                        List<Employee> employees = roleEntry.Value;

                        shiftCopy.AssignedEmployees[role] = new List<Employee>(employees);
                    }

                    copiedShifts.Add(shiftCopy);
                }

                copy.Shifts[branchName] = copiedShifts;
            }

            return copy;
        }

        // פונקציית עזר לבחירת הורה באמצעות סלקציית רולטה
        private static Chromosome SelectParentByRouletteWheel(List<Chromosome> chromosomes)
        {
            Random random = new Random();

            // חישוב סכום כל ערכי הפיטנס
            double totalFitness = 0;
            foreach (var chromosome in chromosomes)
            {
                // התייחסות לערכי פיטנס שליליים
                double adjustedFitness = chromosome.Fitness < 0 ? 0.1 : chromosome.Fitness;
                totalFitness += adjustedFitness;
            }

            // בחירת מיקום אקראי בגלגל הרולטה
            double randomPosition = random.NextDouble() * totalFitness;
            double currentPosition = 0;

            // מציאת הכרומוזום שנבחר
            foreach (var chromosome in chromosomes)
            {
                double adjustedFitness = chromosome.Fitness < 0 ? 0.1 : chromosome.Fitness;
                currentPosition += adjustedFitness;

                if (currentPosition >= randomPosition)
                {
                    return chromosome;
                }
            }

            // במקרה קיצוני, החזר את הכרומוזום האחרון
            return chromosomes[chromosomes.Count - 1];
        }

        // פונקציית עזר לביצוע crossover בין שני הורים
        private static Chromosome PerformCrossover(Chromosome parent1, Chromosome parent2)
        {
            Random random = new Random();
            Chromosome offspring = new Chromosome();
            offspring.Shifts = new Dictionary<string, List<Shift>>();

            // עבור על כל הסניפים בהורה הראשון
            foreach (var branchEntry in parent1.Shifts)
            {
                string branchName = branchEntry.Key;
                List<Shift> parent1Shifts = branchEntry.Value;

                // בדוק אם הסניף קיים גם בהורה השני
                if (parent2.Shifts.ContainsKey(branchName))
                {
                    List<Shift> parent2Shifts = parent2.Shifts[branchName];
                    List<Shift> offspringShifts = new List<Shift>();

                    // עבור על כל המשמרות בסניף
                    int shiftCount = parent1Shifts.Count;
                    for (int i = 0; i < shiftCount; i++)
                    {
                        // בחר נקודת crossover אקראית (נקודה שבה מחליפים בין ההורים)
                        bool useParent1 = random.Next(2) == 0;

                        Shift sourceShift = useParent1 ? parent1Shifts[i] : parent2Shifts[i];

                        // יצירת העתק של המשמרת הנבחרת
                        Shift offspringShift = new Shift
                        {
                            Id = sourceShift.Id,
                            branch = sourceShift.branch,
                            day = sourceShift.day,
                            TimeSlot = sourceShift.TimeSlot,
                            IsBusy = sourceShift.IsBusy,
                            EventType = sourceShift.EventType,
                            RequiredRoles = new Dictionary<string, int>(sourceShift.RequiredRoles),
                            AssignedEmployees = new Dictionary<string, List<Employee>>()
                        };

                        // העתקת העובדים המשובצים
                        foreach (var roleEntry in sourceShift.AssignedEmployees)
                        {
                            string role = roleEntry.Key;
                            List<Employee> employees = roleEntry.Value;

                            offspringShift.AssignedEmployees[role] = new List<Employee>(employees);
                        }

                        offspringShifts.Add(offspringShift);
                    }

                    offspring.Shifts[branchName] = offspringShifts;
                }
                else
                {
                    // אם הסניף לא קיים בהורה השני, העתק אותו מההורה הראשון
                    List<Shift> copiedShifts = new List<Shift>();

                    foreach (Shift originalShift in parent1Shifts)
                    {
                        Shift shiftCopy = new Shift
                        {
                            Id = originalShift.Id,
                            branch = originalShift.branch,
                            day = originalShift.day,
                            TimeSlot = originalShift.TimeSlot,
                            IsBusy = originalShift.IsBusy,
                            EventType = originalShift.EventType,
                            RequiredRoles = new Dictionary<string, int>(originalShift.RequiredRoles),
                            AssignedEmployees = new Dictionary<string, List<Employee>>()
                        };

                        foreach (var roleEntry in originalShift.AssignedEmployees)
                        {
                            string role = roleEntry.Key;
                            List<Employee> employees = roleEntry.Value;

                            shiftCopy.AssignedEmployees[role] = new List<Employee>(employees);
                        }

                        copiedShifts.Add(shiftCopy);
                    }

                    offspring.Shifts[branchName] = copiedShifts;
                }
            }

            // טיפול בסניפים שקיימים רק בהורה השני
            foreach (var branchEntry in parent2.Shifts)
            {
                string branchName = branchEntry.Key;

                // אם הסניף לא קיים בצאצא (כלומר, לא היה בהורה הראשון), הוסף אותו
                if (!offspring.Shifts.ContainsKey(branchName))
                {
                    List<Shift> parent2Shifts = branchEntry.Value;
                    List<Shift> copiedShifts = new List<Shift>();

                    foreach (Shift originalShift in parent2Shifts)
                    {
                        Shift shiftCopy = new Shift
                        {
                            Id = originalShift.Id,
                            branch = originalShift.branch,
                            day = originalShift.day,
                            TimeSlot = originalShift.TimeSlot,
                            IsBusy = originalShift.IsBusy,
                            EventType = originalShift.EventType,
                            RequiredRoles = new Dictionary<string, int>(originalShift.RequiredRoles),
                            AssignedEmployees = new Dictionary<string, List<Employee>>()
                        };

                        foreach (var roleEntry in originalShift.AssignedEmployees)
                        {
                            string role = roleEntry.Key;
                            List<Employee> employees = roleEntry.Value;

                            shiftCopy.AssignedEmployees[role] = new List<Employee>(employees);
                        }

                        copiedShifts.Add(shiftCopy);
                    }

                    offspring.Shifts[branchName] = copiedShifts;
                }
            }

            return offspring;
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

                               
                                    currentChromosomeFitness += calculatesShiftFitness(shift);
                                
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