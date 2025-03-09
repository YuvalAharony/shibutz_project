using EmployeeSchedulingApp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using System.Xml.Linq;

namespace shibutz_project 
{



    internal class Program
    {
        #region Data
        public static List<Employee> Employees = new List<Employee>();
        public static List<Branch> Branches = new List<Branch>();
        
        public static  DB myDB = new DB();
       
        Population pop { get; set; }
        public const int ChromosomesEachGene = 10;
        #endregion

        public void initializeFirstPopulation()
        {
            #region InLine
            Chromosome initializeChoromosome()
            {
                #region InLine
                List<Employee> sort_employees_by_availabilty(List<Employee> employees)
                {
                    return employees.OrderBy(e => e.requestedShifts?.Count ?? 0).ToList();//מבטיח שי לא יקרוס אם לא הוגשו משמרות

                }
                List<Employee> sort_employees_by_rate(List<Employee> employees)
                {
                    return employees.OrderByDescending(e => e.Rate).ToList();
                }
                Dictionary<int,List<Employee>> mappingEmployeesByRequestedShifts()
                {
                      return Program.Employees
                            .SelectMany(emp => emp.requestedShifts, (emp, shiftId) => new { shiftId, emp })
                            .GroupBy(entry => entry.shiftId)
                            .ToDictionary(group => group.Key, group => group.Select(entry => entry.emp).ToList());
                    

                }
                Dictionary<string, List<Employee>> mappingEmployeesByRole()
                {
                    return Program.Employees
                        .SelectMany(emp => emp.Roles, (emp, role) => new { role, emp }) // יצירת צמדים של תפקיד + עובד
                        .GroupBy(entry => entry.role) // קיבוץ לפי תפקיד
                        .ToDictionary(group => group.Key, group => group.Select(entry => entry.emp).ToList()); // המרה למילון
                }

                List<Shift> fill_brach_shifts(Branch br)
                {   
                    Random random = new Random();
                    List<Employee> employeesSortedByRate = sort_employees_by_rate(Employees);
                    List<Employee> employeesSortedByavailabilty = sort_employees_by_availabilty(Employees);
                    Dictionary<int, List<Employee>> employeesMappedByRequestedShifts=mappingEmployeesByRequestedShifts();
                    Dictionary<string, List<Employee>> employeesMappedByRequestedRoles = mappingEmployeesByRole();
                    foreach (Shift sh in br.Shifts)
                    {
                        List<Employee> currenList=new List<Employee>();
                        int currentListIdentifier=random.Next(1,2);//1-employeesSortedByRate //2-employeesSortedByavailabilty

                        foreach (var entry in sh.RequiredRoles)
                        {
                            List<Employee> employeesAvaliableForShift;
                            employeesMappedByRequestedShifts.TryGetValue(sh.Id, out employeesAvaliableForShift);
                            for (int i = 0; i < entry.Value; i++)
                            {
                                employeesMappedByRequestedRoles.TryGetValue(entry.Key, out List<Employee> employeesAvaliableForRole);
                                switch (currentListIdentifier)
                                {
                                    case (1):
                                        currenList = employeesSortedByRate;
                                        break;
                                    case (2):
                                        currenList = employeesSortedByavailabilty;
                                        break;
                                }
                                
                            }
                            

                        }


                    }

                    return null;
                }
               
                #endregion
                
                Chromosome ch = new Chromosome();
                foreach (Branch br in Branches)
                {
                    ch.Shifts.Add(br, fill_brach_shifts(br));
                }
                return ch;
            }
            #endregion
            for (int i = 0; i < ChromosomesEachGene; i++)
            {
                pop.Chromoshomes.Add(initializeChoromosome());
            }
        }
        public void calaulateChoromosomeFitness(Chromosome ch)
        {
            double currentChromosomeFitness = 0;
            #region inLine
            double calculatesBranchFitness(Branch br)
            {
                double currentBranchFitness = 0;
                #region inLine
                double calculatesShiftFitness(Shift shift)
                {
                   
                        double fitness = 0;

                        // 1. ניקוד על שיבוץ נכון של תפקידים (חישוב ישיר עם LINQ)
                        fitness += shift.RequiredRoles.Sum(role =>
                            (shift.AssignedEmployees.Count(empId => Employees.Any(e => e.ID == empId && e.Roles.Contains(role.Key))) >= role.Value ? 10 : -10)
                        );

                        // 2. ניקוד על התאמה למשמרות ולסניפים (ללא תנאים חיצוניים)
                        fitness += shift.AssignedEmployees.Sum(empId =>
                        {
                            var emp = Employees.FirstOrDefault(e => e.ID == empId);
                            return emp == null ? 0 :
                                (emp.requestedShifts.Contains(shift.Id) ? 3 : -5);
                        });

                        // 3. ניקוד על נוכחות עובד מנוסה (Mentor) - שימוש ב- `Any()` במקום `if`
                        fitness += shift.AssignedEmployees.Any(empId => Employees.Any(e => e.ID == empId && e.isMentor)) ? 5 : -5;

                        // 4. חישוב עלות שכר והפחתת ניקוד (חישוב ישיר עם LINQ)
                        fitness -= shift.  AssignedEmployees.Sum(empId =>
                            Employees.FirstOrDefault(e => e.ID == empId)?.HourlySalary * Employees.FirstOrDefault(e => e.ID == empId)?.AssignedHours ?? 0) / 100.0;

                        return fitness;
                    }

                
                #endregion
                foreach (Shift sh in br.Shifts)
                {
                    currentBranchFitness += calculatesShiftFitness(sh);
                }
                return currentBranchFitness;
            }
            #endregion
            foreach (Branch br in ch.Shifts.Keys)
            {
                currentChromosomeFitness += calculatesBranchFitness(br);
            }
            ch.Fitness = currentChromosomeFitness;
        }







         static void Main()
        {
            Branches=DB.addBranches();
            Employees=DB.addEmployees();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);


            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.Run(new HomePage()); // מפעיל את המסך הראשי












        }
    }


}