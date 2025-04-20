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
        //פתיחת החיבור לבסיס הנתונים
        private static readonly string connectionString = "Data Source=(localdb)\\mssqllocaldb;Initial Catalog=EmployeeScheduling;Integrated Security=True";

        //הכרזה על משתנים גלובליים לתוכנית
        #region Data
        public static  Random random = new Random();
        public static List<Employee> Employees;
        public static List<Branch> Branches;
        public const int ChromosomesEachGene = 1000;
        public const int Genes = 500;
        public static Population pop;
        #endregion

        //פעולות הטוענות את כל הנתונים מהדאטא בייס של המשתמש המחובר
        #region SQL
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
                    Branches = LoadUserBranches(username, connection);
                    Employees = LoadUserEmployees(username, connection);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"שגיאה בטעינת נתוני המשתמש: {ex.Message}", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Employees = new List<Employee>();
                Branches = new List<Branch>();
            }
        }

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

            foreach (Branch branch in branches)
            {
                branch.Shifts = LoadBranchShifts(branch.ID, connection);
            }

            return branches;
        }

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

        private static List<Shift> LoadBranchShifts(int branchId, SqlConnection connection)
        {
            List<Shift> shifts = new List<Shift>();

            try
            {
                string query = @"
            SELECT s.ShiftID, ts.TimeSlotName, s.DayOfWeek, st.TypeName, s.IsBusy
            FROM Shifts s
            INNER JOIN ShiftTypes st ON s.ShiftTypeID = st.ShiftTypeID
            INNER JOIN TimeSlots ts on ts.TimeSlotID = s.TimeSlotID
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

                            Dictionary<string, int> requiredRoles = LoadShiftRequiredRoles(shiftId, connection);

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

        private static Dictionary<string, int> LoadShiftRequiredRoles(int shiftId, SqlConnection connection)
        {
            Dictionary<string, int> requiredRoles = new Dictionary<string, int>();

            try
            {
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

        private static List<string> LoadEmployeeRoles(int employeeId, SqlConnection connection)
        {
            List<string> roles = new List<string>();

            try
            {
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

        private static HashSet<int> LoadEmployeePreferredShifts(int employeeId, SqlConnection connection)
        {
            HashSet<int> preferredShifts = new HashSet<int>();

            try
            {
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

        private static List<string> LoadEmployeeBranches(int employeeId, SqlConnection connection)
        {
            List<string> branches = new List<string>();

            try
            {
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
        #endregion

        //אלגוריתם ראשי למציאת סידור עבודה אופטימלי ע"י אלגוריתם גנטי
        public static void createSceduele(string username)
        {
            //יצירת אוכלוסייה חדשה בכל פעם שמפעילים את האלגוריתם
            pop = new Population(new List<Chromosome>(), ChromosomesEachGene);
            //טעינת כל הנתונים של המשתמש המחובר
            LoadDataForUser(username);
            //יצירת אוכלוסייה ראשונית- שלב 1 באלגוריתם הגנטי 
            pop = initializeFirstPopulation(pop);
            //לולאה הרצה לפי מספר הדורות הנקבע ויוצרת דור חדש של צאצאים
            //for (int i = 0; i < Genes; i++)
            //{
            //    //שיפור הכרומוזומים ע"י הכלאה בין זוגות כרומוזומים
            //    crossover(pop);
            //    //שיפור הכרומוזומים ע"י מוטציות בין זוגות כרומוזומים
            //    Mutation(pop);
            //    //מיון האוכלוסייה החדשה ושמירת מספר מסוים(קבוע שהוחלט)
            //    //של הכרומוזומים הטובים ביותר שנוצרו בכל הדורות עד כה
            //    pop.Chromoshomes = pop.Chromoshomes.OrderByDescending(x => x.Fitness).Take(ChromosomesEachGene).ToList();
            //}
            //הדפסת הודעה למשתמש בסיום האלגוריתם שסידור העבודה נוצר בהצלחה
            MessageBox.Show("נוצר בהצלחה", "הצלחה", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        //פונקציה היוצרת אוכלוסייה ראשונית- שלב ראשון באלגוריתם הגנטי
        public static Population initializeFirstPopulation(Population pop)
        {
            for (int i = 0; i < ChromosomesEachGene; i++)
            {
                //שחזור המשמרות של כל עובד בשביל יצירת הכרומוזום הבא
                restoreEmployeesRequestedShifts();
                //אתחול כרומוזום חדש
                Chromosome c = new Chromosome();
                //יצירת הכרומוזום
                c = initializeChoromosome();
                //הוספת הכרומוזום לאוכלוסייה
                pop.Chromoshomes.Add(c);
                //קביעת ציון הכושר של הכרומוזום
                c.Fitness = calaulateChoromosomeFitness(c);
            }
            return pop;
        }

        #region InitializeFirstPopulation Helper Functions
        //פונקציה שמשחזרת את המשמרות המבוקשות של כל עובד
        public static void restoreEmployeesRequestedShifts()
        {
            
            foreach (Employee emp in Employees)//מעבר על רשימת העובדים
            {
                emp.requestedShifts.Clear();//ניקוי המשמרות המבוקשות(כדיי שלא יהיו כפילויות)
                foreach (int id in emp.backUprequestedShifts)//מעבר על גיבוי המשמרות המבוקשות
                {
                    emp.requestedShifts.Add(id);//הוספת המשמרות מהגיבוי למשמרות המבוקשות
                }
            }
        }
        //פונקציה שמטרתה ליצור כרווזום חדש
        public static Chromosome initializeChoromosome()
        {
            List<Employee> employeesSortedByRate = sort_employees_by_rate(Employees);//מיון העובדים לפי הציון שלהם
            List<Employee> employeesSortedByavailabilty = sort_employees_by_availabilty(Employees);//מיון העובדים לפי הזמינות שלהם
            Dictionary<int, List<Employee>> employeesMappedByRequestedShifts = mappingEmployeesByRequestedShifts();//מיפוי העובדים לפי המשמרות המבוקשות
            Dictionary<string, List<Employee>> employeesMappedByRequestedRoles = mappingEmployeesByRole();//מיפןי העובדים לפי תפקידים
            //אתחול כרומוזום חדש
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
                    Shifts = shiftsCopy
                };
                branchesCopy.Add(branchCopy);
            }

            List<Branch> shuffledBranches = Branches.OrderBy(x => random.Next()).ToList();

            foreach (Branch br in shuffledBranches)
            {
                br.Shifts = fill_brach_shifts(br);
                ch.Shifts.Add(br.Name, br.Shifts);
            }

            return ch;
        }

        public static List<Employee> sort_employees_by_availabilty(List<Employee> employees)
        {
            return employees.OrderBy(e => e.requestedShifts?.Count ?? 0).ToList();
        }

        public static List<Employee> sort_employees_by_rate(List<Employee> employees)
        {
            return employees.OrderByDescending(e => e.Rate).ToList();
        }

        public static Dictionary<int, List<Employee>> mappingEmployeesByRequestedShifts()
        {
            return Employees
                .SelectMany(emp => emp.requestedShifts, (emp, shiftId) => new { shiftId, emp })
                .GroupBy(entry => entry.shiftId)
                .ToDictionary(group => group.Key, group => group.Select(entry => entry.emp).ToList());
        }

        public static Dictionary<string, List<Employee>> mappingEmployeesByRole()
        {
            return Employees
                .SelectMany(emp => emp.roles, (emp, role) => new { role, emp })
                .GroupBy(entry => entry.role)
                .ToDictionary(group => group.Key, group => group.Select(entry => entry.emp).ToList());
        }

        public static List<int> UpdateOverlappingShifts(Employee employee, Shift assignedShift)
        {
            #region inLine
            Shift FindShiftById(int shiftId)
            {
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

            List<int> idsToRemove = new List<int>();

            foreach (int shiftId in employee.requestedShifts)
            {
                Shift shift = FindShiftById(shiftId);
                if (shift != null &&
                    shift.day == assignedShift.day &&
                    shift.TimeSlot == assignedShift.TimeSlot)
                {
                    idsToRemove.Add(shiftId);
                }
            }
            return idsToRemove;
        }

        public static List<Shift> fill_brach_shifts(Branch br)
        {
            Dictionary<int, List<Employee>> employeesMappedByRequestedShifts = mappingEmployeesByRequestedShifts();
            Dictionary<string, List<Employee>> employeesMappedByRequestedRoles = mappingEmployeesByRole();
            List<Employee> employeesSortedByRate = sort_employees_by_rate(Employees);
            List<Employee> employeesSortedByavailabilty = sort_employees_by_availabilty(Employees);
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

                        int currentListIdentifier = random.Next(1, 3);
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

        #region Crossover


        public static void crossover(Population pop)
        {
            Random random = new Random();
            List<Chromosome> newOffspring = new List<Chromosome>();

            // יצירת צאצאים נוספים להגברת הגיוון
            int desiredOffspringCount = Program.ChromosomesEachGene * 3 / 4;

            // מיון כרומוזומים לפי Fitness
            var sortedChromosomes = pop.Chromoshomes.OrderByDescending(ch => ch.Fitness).ToList();

            // שמירת המיטבי לצורך אליטיזם
            Chromosome bestChromosome = null;
            if (sortedChromosomes.Count > 0)
                bestChromosome = DeepCopyChromosome(sortedChromosomes[0]);

            for (int i = 0; i < desiredOffspringCount; i++)
            {
                // בחירת הורים – שימוש בבחירת טורניר
                Chromosome parent1 = SelectParentByTournament(pop.Chromoshomes, random);
                Chromosome parent2 = SelectParentByTournament(pop.Chromoshomes, random);

                // ודא שההורים שונים
                int attempts = 0;
                while (parent1 == parent2 && attempts < 3 && pop.Chromoshomes.Count > 1)
                {
                    parent2 = SelectParentByTournament(pop.Chromoshomes, random);
                    attempts++;
                }

                // שימוש בסוגי crossover שונים לקידום הגיוון
                Chromosome offspring;
                int crossoverType = random.Next(3);

                switch (crossoverType)
                {
                    case 0:
                        offspring = PerformUniformCrossover(parent1, parent2, random);
                        break;
                    case 1:
                        offspring = PerformMultiPointCrossover(parent1, parent2, random);
                        break;
                    default:
                        offspring = PerformCrossover(parent1, parent2, random);
                        break;
                }

                // חישוב Fitness לצאצא החדש
                offspring.Fitness = Program.calaulateChoromosomeFitness(offspring);
                newOffspring.Add(offspring);
            }

            // הוספת הצאצאים החדשים לאוכלוסייה
            foreach (var offspring in newOffspring)
            {
                pop.Chromoshomes.Add(offspring);
            }

            // שמירה על הכרומוזומים הטובים לדור הבא
            pop.Chromoshomes = pop.Chromoshomes
                .OrderByDescending(ch => ch.Fitness)
                .Take(Program.ChromosomesEachGene - 1) // משאירים מקום לאליט
                .ToList();

            // הוספת הכרומוזום הטוב חזרה (אליטיזם)
            if (bestChromosome != null)
                pop.Chromoshomes.Add(bestChromosome);
        }

        private static Chromosome SelectParentByTournament(List<Chromosome> chromosomes, Random random)
        {
            // בחירת טורניר עם 3 מועמדים אקראיים
            Chromosome best = null;
            double bestFitness = double.MinValue;

            for (int i = 0; i < 3; i++)
            {
                if (chromosomes.Count == 0)
                    return null;

                int idx = random.Next(chromosomes.Count);
                Chromosome candidate = chromosomes[idx];

                if (best == null || candidate.Fitness > bestFitness)
                {
                    best = candidate;
                    bestFitness = candidate.Fitness;
                }
            }

            return best;
        }

        private static Chromosome PerformUniformCrossover(Chromosome parent1, Chromosome parent2, Random random)
        {
            Chromosome offspring = new Chromosome();
            offspring.Shifts = new Dictionary<string, List<Shift>>();

            // מעקב אחרי שיוכי עובדים למשמרות מסוימות
            Dictionary<Employee, HashSet<string>> employeeAssignments = new Dictionary<Employee, HashSet<string>>();

            // איסוף כל שמות הסניפים משני ההורים
            var allBranchNames = new HashSet<string>(parent1.Shifts.Keys.Concat(parent2.Shifts.Keys));

            foreach (string branchName in allBranchNames)
            {
                // אם סניף קיים רק באחד ההורים, מעתיקים אותו ישירות
                if (!parent1.Shifts.ContainsKey(branchName))
                {
                    var shiftsCopy = DeepCopyShifts(parent2.Shifts[branchName]);
                    offspring.Shifts[branchName] = shiftsCopy;

                    // עדכון מעקב שיוך עובדים
                    RecordEmployeeAssignments(shiftsCopy, employeeAssignments);
                    continue;
                }

                if (!parent2.Shifts.ContainsKey(branchName))
                {
                    var shiftsCopy = DeepCopyShifts(parent1.Shifts[branchName]);
                    offspring.Shifts[branchName] = shiftsCopy;

                    RecordEmployeeAssignments(shiftsCopy, employeeAssignments);
                    continue;
                }

                // אם הסניף קיים בשני ההורים – ביצוע uniform crossover ברמת המשמרת
                List<Shift> parent1Shifts = parent1.Shifts[branchName];
                List<Shift> parent2Shifts = parent2.Shifts[branchName];
                List<Shift> offspringShifts = new List<Shift>();

                // מספר המשמרות המינימלי בין שני ההורים
                int shiftsCount = Math.Min(parent1Shifts.Count, parent2Shifts.Count);

                for (int i = 0; i < shiftsCount; i++)
                {
                    Shift shift1 = parent1Shifts[i];
                    Shift shift2 = parent2Shifts[i];

                    // וידוא שמשמרות מתאימות לאותו יום ושעת משמרת
                    if (shift1.day == shift2.day && shift1.TimeSlot == shift2.TimeSlot)
                    {
                        // יצירת משמרת חדשה עם מאפיינים מ־parent1
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

                        // איסוף כל התפקידים משני ההורים
                        var allRoles = new HashSet<string>(
                            shift1.AssignedEmployees.Keys.Concat(shift2.AssignedEmployees.Keys));

                        // עבור כל תפקיד, בוחרים עובדים מהורה אחד עם מקריות מסוימת
                        foreach (string role in allRoles)
                        {
                            offspringShift.AssignedEmployees[role] = new List<Employee>();

                            // אם התפקיד קיים בשני ההורים
                            if (shift1.AssignedEmployees.ContainsKey(role) &&
                                shift2.AssignedEmployees.ContainsKey(role))
                            {
                                var employees1 = shift1.AssignedEmployees[role];
                                var employees2 = shift2.AssignedEmployees[role];

                                // לקיחת המספר הגדול יותר של עובדים
                                int maxEmployees = Math.Max(employees1.Count, employees2.Count);

                                // עבור כל עמדה, בוחרים מהורה 1 או 2
                                for (int j = 0; j < maxEmployees; j++)
                                {
                                    bool takeFromParent1 = random.Next(2) == 0;
                                    string shiftKey = $"{offspringShift.day}_{offspringShift.TimeSlot}";

                                    if ((takeFromParent1 && j < employees1.Count) ||
                                        (!takeFromParent1 && j >= employees2.Count && j < employees1.Count))
                                    {
                                        Employee emp = employees1[j];
                                        if (!IsEmployeeAlreadyAssigned(emp, shiftKey, employeeAssignments) &&
                                            !offspringShift.AssignedEmployees[role].Contains(emp))
                                        {
                                            offspringShift.AssignedEmployees[role].Add(emp);
                                            if (!employeeAssignments.ContainsKey(emp))
                                                employeeAssignments[emp] = new HashSet<string>();
                                            employeeAssignments[emp].Add(shiftKey);
                                        }
                                    }
                                    else if ((!takeFromParent1 && j < employees2.Count) ||
                                             (takeFromParent1 && j >= employees1.Count && j < employees2.Count))
                                    {
                                        Employee emp = employees2[j];
                                        if (!IsEmployeeAlreadyAssigned(emp, shiftKey, employeeAssignments) &&
                                            !offspringShift.AssignedEmployees[role].Contains(emp))
                                        {
                                            offspringShift.AssignedEmployees[role].Add(emp);
                                            if (!employeeAssignments.ContainsKey(emp))
                                                employeeAssignments[emp] = new HashSet<string>();
                                            employeeAssignments[emp].Add(shiftKey);
                                        }
                                    }
                                }
                            }
                            else if (shift1.AssignedEmployees.ContainsKey(role))
                            {
                                string shiftKey = $"{offspringShift.day}_{offspringShift.TimeSlot}";
                                foreach (Employee emp in shift1.AssignedEmployees[role])
                                {
                                    if (!IsEmployeeAlreadyAssigned(emp, shiftKey, employeeAssignments))
                                    {
                                        offspringShift.AssignedEmployees[role].Add(emp);
                                        if (!employeeAssignments.ContainsKey(emp))
                                            employeeAssignments[emp] = new HashSet<string>();
                                        employeeAssignments[emp].Add(shiftKey);
                                    }
                                }
                            }
                            else if (shift2.AssignedEmployees.ContainsKey(role))
                            {
                                string shiftKey = $"{offspringShift.day}_{offspringShift.TimeSlot}";
                                foreach (Employee emp in shift2.AssignedEmployees[role])
                                {
                                    if (!IsEmployeeAlreadyAssigned(emp, shiftKey, employeeAssignments))
                                    {
                                        offspringShift.AssignedEmployees[role].Add(emp);
                                        if (!employeeAssignments.ContainsKey(emp))
                                            employeeAssignments[emp] = new HashSet<string>();
                                        employeeAssignments[emp].Add(shiftKey);
                                    }
                                }
                            }
                        }
                        offspringShifts.Add(offspringShift);
                    }
                    else
                    {
                        // אם המשמרות לא תואמות – בוחרים אחת באופן אקראי
                        Shift selectedShift = DeepCopyShift(random.Next(2) == 0 ? shift1 : shift2);
                        string shiftKey = $"{selectedShift.day}_{selectedShift.TimeSlot}";
                        RemoveOverlappingEmployees(selectedShift, shiftKey, employeeAssignments);
                        offspringShifts.Add(selectedShift);
                    }
                }

                // אם להורה אחד יש משמרות נוספות – מוסיפים גם אותן
                if (parent1Shifts.Count > shiftsCount)
                {
                    for (int i = shiftsCount; i < parent1Shifts.Count; i++)
                    {
                        Shift extraShift = DeepCopyShift(parent1Shifts[i]);
                        string shiftKey = $"{extraShift.day}_{extraShift.TimeSlot}";
                        RemoveOverlappingEmployees(extraShift, shiftKey, employeeAssignments);
                        offspringShifts.Add(extraShift);
                    }
                }
                else if (parent2Shifts.Count > shiftsCount)
                {
                    for (int i = shiftsCount; i < parent2Shifts.Count; i++)
                    {
                        Shift extraShift = DeepCopyShift(parent2Shifts[i]);
                        string shiftKey = $"{extraShift.day}_{extraShift.TimeSlot}";
                        RemoveOverlappingEmployees(extraShift, shiftKey, employeeAssignments);
                        offspringShifts.Add(extraShift);
                    }
                }
                offspring.Shifts[branchName] = offspringShifts;
            }
            return offspring;
        }

        // בדיקה האם עובד כבר משויך למשמרת באותו יום/שעה
        private static bool IsEmployeeAlreadyAssigned(Employee employee, string shiftKey,
            Dictionary<Employee, HashSet<string>> employeeAssignments)
        {
            return employeeAssignments.ContainsKey(employee) &&
                   employeeAssignments[employee].Contains(shiftKey);
        }

        // רישום שיוכי העובדים ממשמרות לרשומת המעקב
        private static void RecordEmployeeAssignments(List<Shift> shifts,
            Dictionary<Employee, HashSet<string>> employeeAssignments)
        {
            foreach (Shift shift in shifts)
            {
                string shiftKey = $"{shift.day}_{shift.TimeSlot}";
                foreach (var roleEntry in shift.AssignedEmployees)
                {
                    foreach (Employee employee in roleEntry.Value)
                    {
                        if (!employeeAssignments.ContainsKey(employee))
                            employeeAssignments[employee] = new HashSet<string>();
                        employeeAssignments[employee].Add(shiftKey);
                    }
                }
            }
        }

        // הסרת עובדים המשויכים למשמרת כפולה (חפיפה)
        private static void RemoveOverlappingEmployees(Shift shift, string shiftKey,
            Dictionary<Employee, HashSet<string>> employeeAssignments)
        {
            foreach (var roleEntry in shift.AssignedEmployees.ToList())
            {
                string role = roleEntry.Key;
                List<Employee> employees = roleEntry.Value;
                List<Employee> keptEmployees = new List<Employee>();

                foreach (Employee employee in employees)
                {
                    if (!IsEmployeeAlreadyAssigned(employee, shiftKey, employeeAssignments))
                    {
                        keptEmployees.Add(employee);
                        if (!employeeAssignments.ContainsKey(employee))
                            employeeAssignments[employee] = new HashSet<string>();
                        employeeAssignments[employee].Add(shiftKey);
                    }
                }
                shift.AssignedEmployees[role] = keptEmployees;
            }
        }

        private static Chromosome PerformMultiPointCrossover(Chromosome parent1, Chromosome parent2, Random random)
        {
            Chromosome offspring = new Chromosome();
            offspring.Shifts = new Dictionary<string, List<Shift>>();

            // מעקב אחרי שיוכי עובדים למשמרות מסוימות
            Dictionary<Employee, HashSet<string>> employeeAssignments = new Dictionary<Employee, HashSet<string>>();

            // סניפים משותפים לשני ההורים
            var commonBranches = parent1.Shifts.Keys.Intersect(parent2.Shifts.Keys).ToList();

            foreach (string branchName in commonBranches)
            {
                List<Shift> parent1Shifts = parent1.Shifts[branchName];
                List<Shift> parent2Shifts = parent2.Shifts[branchName];
                List<Shift> offspringShifts = new List<Shift>();

                int shiftsCount = Math.Min(parent1Shifts.Count, parent2Shifts.Count);

                if (shiftsCount <= 1)
                {
                    if (shiftsCount == 1)
                    {
                        Shift selectedShift = DeepCopyShift(random.Next(2) == 0 ? parent1Shifts[0] : parent2Shifts[0]);
                        string shiftKey = $"{selectedShift.day}_{selectedShift.TimeSlot}";
                        RemoveOverlappingEmployees(selectedShift, shiftKey, employeeAssignments);
                        offspringShifts.Add(selectedShift);
                    }
                    offspring.Shifts[branchName] = offspringShifts;
                    continue;
                }

                // הגדרת נקודות החלפה
                int numCrossoverPoints = Math.Min(shiftsCount - 1, random.Next(1, 4));
                List<int> crossoverPoints = new List<int>();

                for (int i = 0; i < numCrossoverPoints; i++)
                {
                    int point = random.Next(1, shiftsCount);
                    if (!crossoverPoints.Contains(point))
                        crossoverPoints.Add(point);
                }
                crossoverPoints.Sort();

                bool useParent1 = true;

                for (int i = 0; i < shiftsCount; i++)
                {
                    if (crossoverPoints.Contains(i))
                    {
                        useParent1 = !useParent1;
                    }
                    Shift selectedShift = DeepCopyShift(useParent1 ? parent1Shifts[i] : parent2Shifts[i]);
                    string shiftKey = $"{selectedShift.day}_{selectedShift.TimeSlot}";
                    RemoveOverlappingEmployees(selectedShift, shiftKey, employeeAssignments);
                    offspringShifts.Add(selectedShift);
                }

                // טיפול במשמרות נוספות
                if (parent1Shifts.Count > shiftsCount)
                {
                    for (int i = shiftsCount; i < parent1Shifts.Count; i++)
                    {
                        Shift extraShift = DeepCopyShift(parent1Shifts[i]);
                        string shiftKey = $"{extraShift.day}_{extraShift.TimeSlot}";
                        RemoveOverlappingEmployees(extraShift, shiftKey, employeeAssignments);
                        offspringShifts.Add(extraShift);
                    }
                }
                else if (parent2Shifts.Count > shiftsCount)
                {
                    for (int i = shiftsCount; i < parent2Shifts.Count; i++)
                    {
                        Shift extraShift = DeepCopyShift(parent2Shifts[i]);
                        string shiftKey = $"{extraShift.day}_{extraShift.TimeSlot}";
                        RemoveOverlappingEmployees(extraShift, shiftKey, employeeAssignments);
                        offspringShifts.Add(extraShift);
                    }
                }
                offspring.Shifts[branchName] = offspringShifts;
            }

            // הוספת סניפים הייחודיים לכל הורה
            foreach (string branchName in parent1.Shifts.Keys.Except(commonBranches))
            {
                var shiftsCopy = DeepCopyShifts(parent1.Shifts[branchName]);
                offspring.Shifts[branchName] = shiftsCopy;
                RecordEmployeeAssignments(shiftsCopy, employeeAssignments);
            }
            foreach (string branchName in parent2.Shifts.Keys.Except(commonBranches))
            {
                var shiftsCopy = DeepCopyShifts(parent2.Shifts[branchName]);
                offspring.Shifts[branchName] = shiftsCopy;
                RecordEmployeeAssignments(shiftsCopy, employeeAssignments);
            }

            return offspring;
        }

        private static Chromosome PerformCrossover(Chromosome parent1, Chromosome parent2, Random random)
        {
            Chromosome offspring = new Chromosome();
            offspring.Shifts = new Dictionary<string, List<Shift>>();

            // מעקב אחרי שיוכי עובדים למשמרות מסוימות
            Dictionary<Employee, HashSet<string>> employeeAssignments = new Dictionary<Employee, HashSet<string>>();

            // איסוף רשימת כל הסניפים מהשני הורים
            var allBranchNames = new HashSet<string>(parent1.Shifts.Keys.Concat(parent2.Shifts.Keys));

            foreach (string branchName in allBranchNames)
            {
                // טיפול במקרה שהסניף קיים רק באחד ההורים
                if (!parent1.Shifts.ContainsKey(branchName))
                {
                    var shiftsCopy = DeepCopyShifts(parent2.Shifts[branchName]);

                    // טיפול בחפיפות
                    foreach (Shift shift in shiftsCopy)
                    {
                        string shiftKey = $"{shift.day}_{shift.TimeSlot}";
                        RemoveOverlappingEmployees(shift, shiftKey, employeeAssignments);
                    }

                    offspring.Shifts[branchName] = shiftsCopy;
                    continue;
                }

                if (!parent2.Shifts.ContainsKey(branchName))
                {
                    var shiftsCopy = DeepCopyShifts(parent1.Shifts[branchName]);

                    // טיפול בחפיפות
                    foreach (Shift shift in shiftsCopy)
                    {
                        string shiftKey = $"{shift.day}_{shift.TimeSlot}";
                        RemoveOverlappingEmployees(shift, shiftKey, employeeAssignments);
                    }

                    offspring.Shifts[branchName] = shiftsCopy;
                    continue;
                }

                // אם הסניף קיים בשני ההורים
                List<Shift> offspringShifts = new List<Shift>();
                List<Shift> parent1Shifts = parent1.Shifts[branchName];
                List<Shift> parent2Shifts = parent2.Shifts[branchName];

                // איחוד רשימת המשמרות מההורים
                HashSet<Tuple<string, string>> allShiftSlots = new HashSet<Tuple<string, string>>();

                foreach (Shift shift in parent1Shifts)
                {
                    allShiftSlots.Add(new Tuple<string, string>(shift.day, shift.TimeSlot));
                }

                foreach (Shift shift in parent2Shifts)
                {
                    allShiftSlots.Add(new Tuple<string, string>(shift.day, shift.TimeSlot));
                }

                // עבור כל משמרת אפשרית, בחר אותה מאחד ההורים
                foreach (var shiftSlot in allShiftSlots)
                {
                    string day = shiftSlot.Item1;
                    string timeSlot = shiftSlot.Item2;
                    string shiftKey = $"{day}_{timeSlot}";

                    Shift shift1 = parent1Shifts.FirstOrDefault(s => s.day == day && s.TimeSlot == timeSlot);
                    Shift shift2 = parent2Shifts.FirstOrDefault(s => s.day == day && s.TimeSlot == timeSlot);

                    if (shift1 != null && shift2 != null)
                    {
                        // אם קיימת בשני ההורים, בחר באקראי
                        Shift selectedShift = DeepCopyShift(random.Next(2) == 0 ? shift1 : shift2);
                        RemoveOverlappingEmployees(selectedShift, shiftKey, employeeAssignments);
                        offspringShifts.Add(selectedShift);
                    }
                    else if (shift1 != null)
                    {
                        // אם קיימת רק בהורה הראשון
                        Shift selectedShift = DeepCopyShift(shift1);
                        RemoveOverlappingEmployees(selectedShift, shiftKey, employeeAssignments);
                        offspringShifts.Add(selectedShift);
                    }
                    else if (shift2 != null)
                    {
                        // אם קיימת רק בהורה השני
                        Shift selectedShift = DeepCopyShift(shift2);
                        RemoveOverlappingEmployees(selectedShift, shiftKey, employeeAssignments);
                        offspringShifts.Add(selectedShift);
                    }
                }

                offspring.Shifts[branchName] = offspringShifts;
            }

            return offspring;
        }

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
        #endregion

        #region Mutation

        public static void Mutation(Population pop)
        {
            Random random = new Random();
            List<Chromosome> newChromosomes = new List<Chromosome>();

            // מעקב אחרי עובדים שמשויכים למשמרות למניעת חפיפות
            Dictionary<Employee, HashSet<string>> globalEmployeeAssignments = new Dictionary<Employee, HashSet<string>>();

            foreach (Chromosome chromosome in pop.Chromoshomes)
            {
                // יצירת העתק עמוק של הכרומוזום לצורך מוטציה
                Chromosome mutatedChromosome = DeepCopyChromosome(chromosome);
                bool wasMutated = false;
                double originalFitness = chromosome.Fitness;

                // ניקוי המעקב עבור כל כרומוזום חדש
                globalEmployeeAssignments.Clear();

                // בניית המעקב הקיים של שיוכי עובדים כדי לזהות חפיפות
                foreach (var branchEntry in mutatedChromosome.Shifts)
                {
                    foreach (Shift shift in branchEntry.Value)
                    {
                        foreach (var roleEntry in shift.AssignedEmployees)
                        {
                            foreach (Employee emp in roleEntry.Value)
                            {
                                string shiftKey = $"{shift.day}_{shift.TimeSlot}";
                                if (!globalEmployeeAssignments.ContainsKey(emp))
                                    globalEmployeeAssignments[emp] = new HashSet<string>();
                                globalEmployeeAssignments[emp].Add(shiftKey);
                            }
                        }
                    }
                }

                // ניסיונות מוטציה מרובים - הגדלנו את מספר הניסיונות
                for (int attempt = 0; attempt < 10; attempt++)
                {
                    // בחירת סניף אקראי לביצוע מוטציה
                    if (mutatedChromosome.Shifts.Count == 0)
                        continue;

                    var branchKeys = mutatedChromosome.Shifts.Keys.ToList();
                    string branchName = branchKeys[random.Next(branchKeys.Count)];
                    List<Shift> shifts = mutatedChromosome.Shifts[branchName];

                    if (shifts == null || shifts.Count == 0)
                        continue;

                    // בחירת משמרת אקראית למוטציה
                    int shiftIndex = random.Next(shifts.Count);
                    Shift shiftToMutate = shifts[shiftIndex];
                    string shiftKey = $"{shiftToMutate.day}_{shiftToMutate.TimeSlot}";

                    if (shiftToMutate.AssignedEmployees == null)
                        shiftToMutate.AssignedEmployees = new Dictionary<string, List<Employee>>();

                    // Strategy 1: מילוי משרות ריקות (Focus on empty slots first)
                    bool appliedStrategy1 = TryFillEmptyPositions(shiftToMutate, shiftKey, globalEmployeeAssignments);
                    if (appliedStrategy1)
                    {
                        wasMutated = true;
                        continue;
                    }

                    // Strategy 2: שיפור שיבוץ על ידי החלפת עובדים בעלי דירוג נמוך בעובדים בעלי דירוג גבוה
                    bool appliedStrategy2 = TryUpgradeEmployees(shiftToMutate, shiftKey, globalEmployeeAssignments);
                    if (appliedStrategy2)
                    {
                        wasMutated = true;
                        continue;
                    }

                    // Strategy 3: מנסה לשבץ עובדים שביקשו משמרת זו במקום עובדים שלא ביקשו
                    bool appliedStrategy3 = TryMatchPreferredShifts(shiftToMutate, shiftKey, globalEmployeeAssignments);
                    if (appliedStrategy3)
                    {
                        wasMutated = true;
                        continue;
                    }

                    // Strategy 4: הוספת עובד מנטור אם אין כזה במשמרת
                    bool appliedStrategy4 = TryAddMentor(shiftToMutate, shiftKey, globalEmployeeAssignments);
                    if (appliedStrategy4)
                    {
                        wasMutated = true;
                        continue;
                    }

                    // Strategy 5: ניסיון להפחית עלויות על ידי החלפת עובדים יקרים בעובדים זולים יותר
                    bool appliedStrategy5 = TryReduceCosts(shiftToMutate, shiftKey, globalEmployeeAssignments);
                    if (appliedStrategy5)
                    {
                        wasMutated = true;
                        continue;
                    }
                }

                if (wasMutated)
                {
                    // חישוב ציון הכושר החדש
                    mutatedChromosome.Fitness = Program.calaulateChoromosomeFitness(mutatedChromosome);

                    // רק אם הכרומוזום שופר (או לא נפגע) נוסיף אותו לאוכלוסייה
                    if (mutatedChromosome.Fitness >= originalFitness)
                    {
                        newChromosomes.Add(mutatedChromosome);
                    }
                }
            }

            // הוספת הכרומוזומים החדשים לאוכלוסייה
            foreach (var newChromosome in newChromosomes)
            {
                pop.Chromoshomes.Add(newChromosome);
            }

            // שמירה על הכרומוזומים הטובים בלבד
            pop.Chromoshomes = pop.Chromoshomes
                .OrderByDescending(ch => ch.Fitness)
                .Take(Program.ChromosomesEachGene)
                .ToList();
        }

        // ניסיון למלא משרות ריקות
        private static bool TryFillEmptyPositions(Shift shiftToMutate, string shiftKey,
            Dictionary<Employee, HashSet<string>> globalEmployeeAssignments)
        {
            // בדיקה האם קיימות משרות שלא מולאו
            foreach (var roleReq in shiftToMutate.RequiredRoles)
            {
                string role = roleReq.Key;
                int required = roleReq.Value;

                if (!shiftToMutate.AssignedEmployees.ContainsKey(role))
                    shiftToMutate.AssignedEmployees[role] = new List<Employee>();

                if (shiftToMutate.AssignedEmployees[role].Count < required)
                {
                    // מציאת עובדים שכבר במשמרת
                    HashSet<Employee> employeesInShift = GetEmployeesInShift(shiftToMutate);

                    // מציאת עובדים זמינים לתפקיד, עם עדיפות למי שביקש את המשמרת
                    List<Employee> preferredEmployees = Program.Employees
                        .Where(e => e.roles.Contains(role) &&
                               e.requestedShifts.Contains(shiftToMutate.Id) &&
                               !employeesInShift.Contains(e) &&
                               (!globalEmployeeAssignments.ContainsKey(e) ||
                                !globalEmployeeAssignments[e].Contains(shiftKey)))
                        .OrderByDescending(e => e.Rate)
                        .ToList();

                    // אם יש עובדים מתאימים שביקשו את המשמרת
                    if (preferredEmployees.Count > 0)
                    {
                        // הוספת עובד מועדף למשרה הריקה
                        Employee selectedEmployee = preferredEmployees[0];
                        shiftToMutate.AssignedEmployees[role].Add(selectedEmployee);

                        // עדכון המעקב
                        if (!globalEmployeeAssignments.ContainsKey(selectedEmployee))
                            globalEmployeeAssignments[selectedEmployee] = new HashSet<string>();
                        globalEmployeeAssignments[selectedEmployee].Add(shiftKey);

                        return true;
                    }
                    else
                    {
                        // אם אין עובדים שביקשו את המשמרת, ננסה לשבץ כל עובד מתאים
                        List<Employee> availableEmployees = Program.Employees
                            .Where(e => e.roles.Contains(role) &&
                                  !employeesInShift.Contains(e) &&
                                  (!globalEmployeeAssignments.ContainsKey(e) ||
                                   !globalEmployeeAssignments[e].Contains(shiftKey)))
                            .OrderByDescending(e => e.Rate)
                            .ToList();

                        if (availableEmployees.Count > 0)
                        {
                            // הוספת עובד מתאים למשרה הריקה
                            Employee selectedEmployee = availableEmployees[0];
                            shiftToMutate.AssignedEmployees[role].Add(selectedEmployee);

                            // עדכון המעקב
                            if (!globalEmployeeAssignments.ContainsKey(selectedEmployee))
                                globalEmployeeAssignments[selectedEmployee] = new HashSet<string>();
                            globalEmployeeAssignments[selectedEmployee].Add(shiftKey);

                            return true;
                        }
                    }
                }
            }
            return false;
        }

        // ניסיון לשפר את הרכב העובדים על ידי החלפת עובדים בעלי דירוג נמוך בעובדים בעלי דירוג גבוה
        private static bool TryUpgradeEmployees(Shift shiftToMutate, string shiftKey,
            Dictionary<Employee, HashSet<string>> globalEmployeeAssignments)
        {
            if (shiftToMutate.AssignedEmployees.Count == 0)
                return false;

            // בחירת תפקיד אקראי שיש בו עובדים
            var rolesWithEmployees = shiftToMutate.AssignedEmployees
                .Where(kv => kv.Value != null && kv.Value.Count > 0)
                .Select(kv => kv.Key)
                .ToList();

            if (rolesWithEmployees.Count == 0)
                return false;

            Random random = new Random();
            string role = rolesWithEmployees[random.Next(rolesWithEmployees.Count)];
            List<Employee> employees = shiftToMutate.AssignedEmployees[role];

            if (employees.Count > 0)
            {
                // מיון העובדים לפי דירוג (מהנמוך לגבוה) כדי להחליף את הנמוך ביותר
                employees.Sort((a, b) => a.Rate.CompareTo(b.Rate));
                Employee currentEmployee = employees[0]; // העובד עם הדירוג הנמוך ביותר

                // מציאת עובדים שכבר במשמרת
                HashSet<Employee> employeesInShift = GetEmployeesInShift(shiftToMutate);

                // מציאת עובדים פוטנציאליים להחלפה עם דירוג גבוה יותר
                List<Employee> potentialReplacements = Program.Employees
                    .Where(e => e.roles.Contains(role) &&
                           e.Rate > currentEmployee.Rate &&
                           !employeesInShift.Contains(e) &&
                           (!globalEmployeeAssignments.ContainsKey(e) ||
                            !globalEmployeeAssignments[e].Contains(shiftKey)))
                    .OrderByDescending(e => e.Rate)
                    .ToList();

                if (potentialReplacements.Count > 0)
                {
                    // הסרת העובד הנוכחי מהמעקב
                    if (globalEmployeeAssignments.ContainsKey(currentEmployee))
                        globalEmployeeAssignments[currentEmployee].Remove(shiftKey);

                    // החלפה בעובד טוב יותר
                    Employee replacement = potentialReplacements[0];
                    int employeeIndex = employees.IndexOf(currentEmployee);
                    shiftToMutate.AssignedEmployees[role][employeeIndex] = replacement;

                    if (!globalEmployeeAssignments.ContainsKey(replacement))
                        globalEmployeeAssignments[replacement] = new HashSet<string>();
                    globalEmployeeAssignments[replacement].Add(shiftKey);

                    return true;
                }
            }
            return false;
        }

        // ניסיון להתאים עובדים למשמרות שהם ביקשו
        private static bool TryMatchPreferredShifts(Shift shiftToMutate, string shiftKey,
            Dictionary<Employee, HashSet<string>> globalEmployeeAssignments)
        {
            if (shiftToMutate.AssignedEmployees.Count == 0)
                return false;

            foreach (var roleEntry in shiftToMutate.AssignedEmployees)
            {
                string role = roleEntry.Key;
                List<Employee> employees = roleEntry.Value;

                // מיון העובדים לפי אלו שלא ביקשו את המשמרת
                var nonPreferredEmployees = employees
                    .Where(e => !e.requestedShifts.Contains(shiftToMutate.Id))
                    .ToList();

                if (nonPreferredEmployees.Count > 0)
                {
                    Employee currentEmployee = nonPreferredEmployees[0];

                    // מציאת עובדים שכבר במשמרת
                    HashSet<Employee> employeesInShift = GetEmployeesInShift(shiftToMutate);
                    employeesInShift.Remove(currentEmployee); // מסירים את העובד הנוכחי שנחליף

                    // מציאת עובדים שכן ביקשו את המשמרת
                    List<Employee> preferredEmployees = Program.Employees
                        .Where(e => e.roles.Contains(role) &&
                               e.requestedShifts.Contains(shiftToMutate.Id) &&
                               !employeesInShift.Contains(e) &&
                               (!globalEmployeeAssignments.ContainsKey(e) ||
                                !globalEmployeeAssignments[e].Contains(shiftKey)))
                        .OrderByDescending(e => e.Rate)
                        .ToList();

                    if (preferredEmployees.Count > 0)
                    {
                        // הסרת העובד הנוכחי מהמעקב
                        if (globalEmployeeAssignments.ContainsKey(currentEmployee))
                            globalEmployeeAssignments[currentEmployee].Remove(shiftKey);

                        // החלפה בעובד שביקש את המשמרת
                        Employee replacement = preferredEmployees[0];
                        int employeeIndex = employees.IndexOf(currentEmployee);
                        shiftToMutate.AssignedEmployees[role][employeeIndex] = replacement;

                        if (!globalEmployeeAssignments.ContainsKey(replacement))
                            globalEmployeeAssignments[replacement] = new HashSet<string>();
                        globalEmployeeAssignments[replacement].Add(shiftKey);

                        return true;
                    }
                }
            }
            return false;
        }

        // ניסיון להוסיף עובד מנטור אם אין כזה במשמרת
        private static bool TryAddMentor(Shift shiftToMutate, string shiftKey,
            Dictionary<Employee, HashSet<string>> globalEmployeeAssignments)
        {
            // בדיקה אם כבר יש מנטור במשמרת
            bool hasMentor = false;
            foreach (var employeeList in shiftToMutate.AssignedEmployees.Values)
            {
                if (employeeList.Any(e => e.isMentor))
                {
                    hasMentor = true;
                    break;
                }
            }

            // אם אין מנטור, ננסה להוסיף אחד
            if (!hasMentor)
            {
                // עובר על כל התפקידים כדי לנסות להחליף עובד רגיל במנטור
                foreach (var roleEntry in shiftToMutate.AssignedEmployees)
                {
                    string role = roleEntry.Key;
                    List<Employee> employees = roleEntry.Value;

                    if (employees.Count > 0)
                    {
                        // בוחר עובד אקראי שאינו מנטור
                        Random random = new Random();
                        var nonMentors = employees.Where(e => !e.isMentor).ToList();

                        if (nonMentors.Count > 0)
                        {
                            Employee currentEmployee = nonMentors[random.Next(nonMentors.Count)];

                            // מציאת עובדים שכבר במשמרת
                            HashSet<Employee> employeesInShift = GetEmployeesInShift(shiftToMutate);
                            employeesInShift.Remove(currentEmployee); // מסירים את העובד הנוכחי שנחליף

                            // חיפוש מנטור מתאים
                            List<Employee> availableMentors = Program.Employees
                                .Where(e => e.roles.Contains(role) &&
                                       e.isMentor &&
                                       !employeesInShift.Contains(e) &&
                                       (!globalEmployeeAssignments.ContainsKey(e) ||
                                        !globalEmployeeAssignments[e].Contains(shiftKey)))
                                .OrderByDescending(e => e.Rate)
                                .ToList();

                            if (availableMentors.Count > 0)
                            {
                                // הסרת העובד הנוכחי מהמעקב
                                if (globalEmployeeAssignments.ContainsKey(currentEmployee))
                                    globalEmployeeAssignments[currentEmployee].Remove(shiftKey);

                                // החלפה במנטור
                                Employee mentor = availableMentors[0];
                                int employeeIndex = employees.IndexOf(currentEmployee);
                                shiftToMutate.AssignedEmployees[role][employeeIndex] = mentor;

                                if (!globalEmployeeAssignments.ContainsKey(mentor))
                                    globalEmployeeAssignments[mentor] = new HashSet<string>();
                                globalEmployeeAssignments[mentor].Add(shiftKey);

                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        // ניסיון להפחית עלויות על ידי החלפת עובדים יקרים בעובדים זולים יותר
        private static bool TryReduceCosts(Shift shiftToMutate, string shiftKey,
            Dictionary<Employee, HashSet<string>> globalEmployeeAssignments)
        {
            if (shiftToMutate.AssignedEmployees.Count == 0)
                return false;

            // בחירת תפקיד אקראי שיש בו עובדים
            Random random = new Random();
            var rolesWithEmployees = shiftToMutate.AssignedEmployees
                .Where(kv => kv.Value != null && kv.Value.Count > 0)
                .Select(kv => kv.Key)
                .ToList();

            if (rolesWithEmployees.Count == 0)
                return false;

            string role = rolesWithEmployees[random.Next(rolesWithEmployees.Count)];
            List<Employee> employees = shiftToMutate.AssignedEmployees[role];

            if (employees.Count > 0)
            {
                // מיון העובדים לפי שכר (מהגבוה לנמוך)
                employees.Sort((a, b) => b.HourlySalary.CompareTo(a.HourlySalary));
                Employee expensiveEmployee = employees[0]; // העובד עם השכר הגבוה ביותר

                // רק אם העובד מרוויח מעל למינימום מסוים - שווה להחליף אותו
                if (expensiveEmployee.HourlySalary > 50)
                {
                    // מציאת עובדים שכבר במשמרת
                    HashSet<Employee> employeesInShift = GetEmployeesInShift(shiftToMutate);
                    employeesInShift.Remove(expensiveEmployee); // מסירים את העובד היקר שנחליף

                    // חיפוש עובדים זולים יותר
                    List<Employee> cheaperEmployees = Program.Employees
                        .Where(e => e.roles.Contains(role) &&
                               e.HourlySalary < expensiveEmployee.HourlySalary &&
                               e.Rate >= expensiveEmployee.Rate * 0.8 && // לא יותר מדי נמוך בדירוג
                               !employeesInShift.Contains(e) &&
                               (!globalEmployeeAssignments.ContainsKey(e) ||
                                !globalEmployeeAssignments[e].Contains(shiftKey)))
                        .OrderBy(e => e.HourlySalary)
                        .ToList();

                    if (cheaperEmployees.Count > 0)
                    {
                        // הסרת העובד היקר מהמעקב
                        if (globalEmployeeAssignments.ContainsKey(expensiveEmployee))
                            globalEmployeeAssignments[expensiveEmployee].Remove(shiftKey);

                        // החלפה בעובד זול יותר
                        Employee replacement = cheaperEmployees[0];
                        int employeeIndex = employees.IndexOf(expensiveEmployee);
                        shiftToMutate.AssignedEmployees[role][employeeIndex] = replacement;

                        if (!globalEmployeeAssignments.ContainsKey(replacement))
                            globalEmployeeAssignments[replacement] = new HashSet<string>();
                        globalEmployeeAssignments[replacement].Add(shiftKey);

                        return true;
                    }
                }
            }
            return false;
        }

        // Helper method to get all employees currently in a shift
        private static HashSet<Employee> GetEmployeesInShift(Shift shift)
        {
            HashSet<Employee> employeesInShift = new HashSet<Employee>();
            foreach (var roleEntry in shift.AssignedEmployees)
            {
                foreach (Employee emp in roleEntry.Value)
                {
                    employeesInShift.Add(emp);
                }
            }
            return employeesInShift;
        }

        // Helper function to find shifts that would overlap with an assigned shift
        private static List<int> FindOverlappingShifts(Employee employee, Shift assignedShift)
        {
            if (employee == null || assignedShift == null)
                return new List<int>();

            List<int> overlappingShiftIds = new List<int>();

            foreach (int shiftId in employee.requestedShifts)
            {
                // Find the shift in the branches
                Shift shift = null;
                foreach (Branch branch in Program.Branches)
                {
                    shift = branch.Shifts.FirstOrDefault(s => s.Id == shiftId);
                    if (shift != null)
                        break;
                }

                // Check if it's an overlapping shift (same day and time slot)
                if (shift != null && shift.day == assignedShift.day && shift.TimeSlot == assignedShift.TimeSlot)
                {
                    overlappingShiftIds.Add(shiftId);
                }
            }

            return overlappingShiftIds;
        }

        #endregion

        public static double calaulateChoromosomeFitness(Chromosome ch)
        {
            double currentChromosomeFitness = 0;
            double calculatesShiftFitness(Shift shift)
            {
                double fitness = 0;

                if (shift.AssignedEmployees == null)
                {
                    shift.AssignedEmployees = new Dictionary<string, List<Employee>>();
                }

                foreach (var roleEntry in shift.RequiredRoles)
                {
                    string role = roleEntry.Key;
                    int requiredCount = roleEntry.Value;

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

            currentChromosomeFitness -= unassignedShifts * 20;
            return currentChromosomeFitness;
        }

        public static Chromosome GetBestChromosome()
        {
            return pop.Chromoshomes.OrderBy(ch => ch.Fitness).FirstOrDefault();
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
