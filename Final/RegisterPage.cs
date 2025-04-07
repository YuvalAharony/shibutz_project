using System;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Data.SqlClient;


namespace EmployeeSchedulingApp
{
    public partial class RegisterPage : Form
    {
        private static string connectionString = "Data Source=(localdb)\\mssqllocaldb;Initial Catalog=EmployeeScheduling;Integrated Security=True";

        public RegisterPage()
        {
            SetupUI();
        }

        private void SetupUI()
        {
            this.Text = "הרשמה למערכת";
            this.Size = new System.Drawing.Size(400, 450); // הגדלת החלון
            this.RightToLeft = RightToLeft.Yes; // תמיכה בעברית מימין לשמאל

            Label titleLabel = new Label()
            {
                Text = "צור חשבון חדש",
                AutoSize = true,
                Font = new System.Drawing.Font("Arial", 14, System.Drawing.FontStyle.Bold),
                Location = new System.Drawing.Point(120, 20)
            };

            // שם מלא
            Label fullNameLabel = new Label() { Text = "שם מלא:", Location = new System.Drawing.Point(50, 70) };
            TextBox fullNameTextBox = new TextBox() { Location = new System.Drawing.Point(150, 70), Width = 180 };

            // שם משתמש
            Label userLabel = new Label() { Text = "שם משתמש:", Location = new System.Drawing.Point(50, 110) };
            TextBox userTextBox = new TextBox() { Location = new System.Drawing.Point(150, 110), Width = 180 };

            // מייל
            Label emailLabel = new Label() { Text = "אימייל:", Location = new System.Drawing.Point(50, 150) };
            TextBox emailTextBox = new TextBox() { Location = new System.Drawing.Point(150, 150), Width = 180 };

            // סיסמה
            Label passLabel = new Label() { Text = "סיסמה:", Location = new System.Drawing.Point(50, 190) };
            TextBox passTextBox = new TextBox() { Location = new System.Drawing.Point(150, 190), Width = 180, PasswordChar = '*' };

            // אימות סיסמה
            Label confirmPassLabel = new Label() { Text = "אימות סיסמה:", Location = new System.Drawing.Point(50, 230) };
            TextBox confirmPassTextBox = new TextBox() { Location = new System.Drawing.Point(150, 230), Width = 180, PasswordChar = '*' };

            // כפתור הרשמה
            Button registerButton = new Button()
            {
                Text = "הרשם",
                Size = new System.Drawing.Size(100, 40),
                Location = new System.Drawing.Point(150, 280)
            };

            // כפתור ביטול
            Button cancelButton = new Button()
            {
                Text = "ביטול",
                Size = new System.Drawing.Size(100, 40),
                Location = new System.Drawing.Point(40, 280)
            };

            // עדכון האירוע של כפתור ההרשמה כדי לכלול את השדות החדשים
            registerButton.Click += (sender, e) => {
                PerformRegistration(
                    userTextBox.Text,
                    passTextBox.Text,
                    confirmPassTextBox.Text,
                    fullNameTextBox.Text,
                    emailTextBox.Text
                );
            };

            cancelButton.Click += (sender, e) => {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            };

            // הוספת כל הרכיבים לטופס
            this.Controls.Add(titleLabel);
            this.Controls.Add(fullNameLabel);
            this.Controls.Add(fullNameTextBox);
            this.Controls.Add(userLabel);
            this.Controls.Add(userTextBox);
            this.Controls.Add(emailLabel);
            this.Controls.Add(emailTextBox);
            this.Controls.Add(passLabel);
            this.Controls.Add(passTextBox);
            this.Controls.Add(confirmPassLabel);
            this.Controls.Add(confirmPassTextBox);
            this.Controls.Add(registerButton);
            this.Controls.Add(cancelButton);
        }
        private void PerformRegistration(string username, string password, string confirmPassword, string fullName, string email)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(confirmPassword))
            {
                MessageBox.Show("נא למלא את כל השדות.", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (password != confirmPassword)
            {
                MessageBox.Show("הסיסמאות אינן תואמות.", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // שימוש בפרמטרים מונע SQL Injection
                string query = @"INSERT INTO Users (Username, Password, FullName, Email, IsActive)
                        VALUES (@Username, @Password, @FullName, @Email, @IsActive);";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // הוספת הפרמטרים
                    command.Parameters.AddWithValue("@Username", username);
                    command.Parameters.AddWithValue("@Password", password); // הצפנת הסיסמה
                    command.Parameters.AddWithValue("@FullName", fullName);
                    command.Parameters.AddWithValue("@Email", email);
                    command.Parameters.AddWithValue("@IsActive", 1);

                    // ביצוע השאילתה
                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("המשתמש נוסף בהצלחה!", "הצלחה", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("הוספת המשתמש נכשלה", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }

            MessageBox.Show("הרשמה הושלמה בהצלחה!", "הצלחה", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Close();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // RegisterPage
            // 
            this.ClientSize = new System.Drawing.Size(282, 253);
            this.Name = "RegisterPage";
            this.ResumeLayout(false);

        }

      
    }
}