//using Final;
//using System;
//using System.Collections.Generic;
//using System.Data;
//using System.Data.SqlClient;

//public static class DataBaseHelper
//{
//    private static string connectionString = "Data Source=YOUR_SERVER_NAME;Initial Catalog=EmployeeScheduling;Integrated Security=True";

//    public static void LoadData()
//    {
//        // טען את הנתונים הנדרשים ישירות לרשימות במחלקת Program
//        Program.Employees = LoadEmployees();
//        Program.Branches = LoadBranches();
//    }

//    private static List<Employee> LoadEmployees()
//    {
//        List<Employee> employees = new List<Employee>();

//        using (SqlConnection connection = new SqlConnection(connectionString))
//        {
//            connection.Open();

//            string query = "SELECT * FROM Employees";
//            using (SqlCommand command = new SqlCommand(query, connection))
//            {
//                using (SqlDataReader reader = command.ExecuteReader())
//                {
//                    Employee employee = new Employee
//                    (
//                        Convert.ToInt32(reader["EmployeeID"]),
//                        reader["Name"].ToString(),
//                        new List<string>(),
//                        new HashSet<int>(),
//                        reader["Rate"] != DBNull.Value ? Convert.ToInt32(reader["Rate"]) : 0,
//                        Convert.ToInt32(reader["HourlySalary"]),
//                        Convert.ToInt32(reader["AssignedHours"]),
//                        Convert.ToBoolean(reader["IsMentor"]),
//                        null


//                    );

//                    employees.Add(employee);
                    
//                }
//            }

//            // טען את התפקידים של כל עובד
//            foreach (Employee employee in employees)
//            {
//                string rolesQuery = @"
//                    SELECT r.RoleName 
//                    FROM EmployeeRoles er
//                    JOIN Roles r ON er.RoleID = r.RoleID
//                    WHERE er.EmployeeID = @EmployeeID";

//                employee.roles = new List<string>();

//                using (SqlCommand command = new SqlCommand(rolesQuery, connection))
//                {
//                    command.Parameters.AddWithValue("@EmployeeID", employee.ID);
//                    using (SqlDataReader reader = command.ExecuteReader())
//                    {
//                        while (reader.Read())
//                        {
//                            employee.roles.Add(reader["RoleName"].ToString());
//                        }
//                    }
//                }
//            }
//        }

//        return employees;
//    }

//    private static List<Branch> LoadBranches()
//    {
//        // קוד דומה לטעינת סניפים
//        // ...

//        return new List<Branch>();
//    }
//}