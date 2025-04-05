using Final;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

public static class DataBaseHelper
{
    private static string connectionString = "Data Source=YOUR_SERVER_NAME;Initial Catalog=EmployeeScheduling;Integrated Security=True";

    public static void LoadData()
    {
        // טען את הנתונים הנדרשים ישירות לרשימות במחלקת Program
        Program.Employees = LoadEmployees();
        Program.Branches = LoadBranches();
    }

    private static List<Employee> LoadEmployees()
    {
        List<Employee> employees = new List<Employee>();

        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            connection.Open();

            string query = "SELECT * FROM Employees";
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    Employee employee = new Employee
                    {
                        ID = Convert.ToInt32(reader["EmployeeID"]),
                        Name = reader["Name"].ToString(),
                        Roles = new List<string>(),
                        requestedShifts = new HashSet<int>(),
                        backUprequestedShifts =new HashSet<int>(),
                        Rate = reader["Rate"] != DBNull.Value ? Convert.ToInt32(reader["Rate"]) : 0,
                        HourlySalary = (int)Convert.ToDecimal(reader["HourlySalary"]),
                        AssignedHours = reader["AssignedHours"] != DBNull.Value ? Convert.ToInt32(reader["AssignedHours"]) : 0,
                        isMentor = Convert.ToBoolean(reader["IsMentor"]),
                        Branches=null,
                        
                        
                    };

                    employees.Add(employee);
                    
                }
            }

            // טען את התפקידים של כל עובד
            foreach (Employee employee in employees)
            {
                string rolesQuery = @"
                    SELECT r.RoleName 
                    FROM EmployeeRoles er
                    JOIN Roles r ON er.RoleID = r.RoleID
                    WHERE er.EmployeeID = @EmployeeID";

                employee.Roles = new List<string>();

                using (SqlCommand command = new SqlCommand(rolesQuery, connection))
                {
                    command.Parameters.AddWithValue("@EmployeeID", employee.ID);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            employee.Roles.Add(reader["RoleName"].ToString());
                        }
                    }
                }
            }
        }

        return employees;
    }

    private static List<Branch> LoadBranches()
    {
        // קוד דומה לטעינת סניפים
        // ...

        return new List<Branch>();
    }
}