using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Final
{
    public class EmployeeLoginPage : Form
    {
        private static string connectionString = "Data Source=(localdb)\\mssqllocaldb;Initial Catalog=EmployeeScheduling;Integrated Security=True";
        private TextBox employeeIdTextBox;
        private TextBox passwordTextBox;

        public EmployeeLoginPage()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "כניסת עובד";
            this.Size = new System.Drawing.Size(400, 300);
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;

            Label titleLabel = new Label
            {
                Text = "כניסת עובד למערכת",
                Font = new System.Drawing.Font("Arial", 14, System.Drawing.FontStyle.Bold),
                AutoSize = true,
                Location = new System.Drawing.Point(120, 20)
            };

            Label idLabel = new Label
            {
                Text = "מספר זיהוי:",
                Location = new System.Drawing.Point(50, 70),
                AutoSize = true
            };

            employeeIdTextBox = new TextBox
            {
                Location = new System.Drawing.Point(150, 70),
                Width = 180
            };

            Label passwordLabel = new Label
            {
                Text = "סיסמה:",
                Location = new System.Drawing.Point(50, 110),
                AutoSize = true
            };

            passwordTextBox = new TextBox
            {
                Location = new System.Drawing.Point(150, 110),
                Width = 180,
                PasswordChar = '*'
            };

            Button loginButton = new Button
            {
                Text = "התחברות",
                Size = new System.Drawing.Size(100, 40),
                Location = new System.Drawing.Point(150, 160)
            };
            loginButton.Click += LoginButton_Click;

            Button backButton = new Button
            {
                Text = "חזרה",
                Size = new System.Drawing.Size(100, 40),
                Location = new System.Drawing.Point(40, 160)
            };
            backButton.Click += (sender, e) => { this.Close(); };

            // הוספת רכיבים לטופס
            this.Controls.Add(titleLabel);
            this.Controls.Add(idLabel);
            this.Controls.Add(employeeIdTextBox);
            this.Controls.Add(passwordLabel);
            this.Controls.Add(passwordTextBox);
            this.Controls.Add(loginButton);
            this.Controls.Add(backButton);
        }

        private void LoginButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(employeeIdTextBox.Text))
            {
                MessageBox.Show("אנא הזן מספר זיהוי", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!int.TryParse(employeeIdTextBox.Text, out int employeeId))
            {
                MessageBox.Show("מספר זיהוי חייב להיות מספר", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(passwordTextBox.Text))
            {
                MessageBox.Show("אנא הזן סיסמה", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string password = passwordTextBox.Text;

            // בדיקת זהות העובד והסיסמה
            Employee employee = ValidateEmployee(employeeId, password);

            if (employee != null)
            {
                MessageBox.Show($"ברוך הבא, {employee.Name}!", "התחברות מוצלחת", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // פתיחת הטופס להגשת משמרות עם העובד הספציפי
                EmployeeShiftRequestPage shiftRequestPage = new EmployeeShiftRequestPage(employee);
                shiftRequestPage.ShowDialog();

                this.Close();
            }
            else
            {
                MessageBox.Show("פרטי ההתחברות שגויים או שהעובד לא קיים במערכת", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Employee ValidateEmployee(int employeeId, string password)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = @"
                        SELECT e.EmployeeID, e.Name, e.Rate, e.HourlySalary, e.AssignedHours, e.IsMentor 
                        FROM Employees e
                        WHERE e.EmployeeID = @EmployeeID AND e.Password = @Password";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@EmployeeID", employeeId);
                        command.Parameters.AddWithValue("@Password", password);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string name = reader.GetString(1);
                                int rate = reader.GetInt32(2);
                                int salary = Convert.ToInt32(reader.GetDecimal(3));
                                int hours = reader.GetInt32(4);
                                bool isMentor = reader.GetBoolean(5);

                                // יצירת אובייקט העובד עם המידע הנחוץ
                                Employee employee = new Employee(
                                    employeeId, name, new HashSet<string>(),
                                    new System.Collections.Generic.HashSet<int>(),
                                    rate, salary, isMentor, null
                                );

                                return employee;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"שגיאה בהתחברות: {ex.Message}", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return null;
        }
    }
}