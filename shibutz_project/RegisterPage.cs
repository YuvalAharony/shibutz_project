using System;
using System.Windows.Forms;

namespace EmployeeSchedulingApp
{
    public partial class RegisterPage : Form
    {
        public RegisterPage()
        {
            SetupUI();
        }

        private void SetupUI()
        {
            this.Text = "הרשמה למערכת";
            this.Size = new System.Drawing.Size(400, 350);

            Label titleLabel = new Label()
            {
                Text = "צור חשבון חדש",
                AutoSize = true,
                Font = new System.Drawing.Font("Arial", 14, System.Drawing.FontStyle.Bold),
                Location = new System.Drawing.Point(120, 20)
            };

            Label userLabel = new Label() { Text = "שם משתמש:", Location = new System.Drawing.Point(50, 70) };
            TextBox userTextBox = new TextBox() { Location = new System.Drawing.Point(150, 70), Width = 180 };

            Label passLabel = new Label() { Text = "סיסמה:", Location = new System.Drawing.Point(50, 110) };
            TextBox passTextBox = new TextBox() { Location = new System.Drawing.Point(150, 110), Width = 180, PasswordChar = '*' };

            Label confirmPassLabel = new Label() { Text = "אימות סיסמה:", Location = new System.Drawing.Point(50, 150) };
            TextBox confirmPassTextBox = new TextBox() { Location = new System.Drawing.Point(150, 150), Width = 180, PasswordChar = '*' };

            Button registerButton = new Button()
            {
                Text = "הרשם",
                Size = new System.Drawing.Size(100, 40),
                Location = new System.Drawing.Point(150, 200)
            };
            registerButton.Click += (sender, e) => { PerformRegistration(userTextBox.Text, passTextBox.Text, confirmPassTextBox.Text); };

            this.Controls.Add(titleLabel);
            this.Controls.Add(userLabel);
            this.Controls.Add(userTextBox);
            this.Controls.Add(passLabel);
            this.Controls.Add(passTextBox);
            this.Controls.Add(confirmPassLabel);
            this.Controls.Add(confirmPassTextBox);
            this.Controls.Add(registerButton);
        }

        private void PerformRegistration(string username, string password, string confirmPassword)
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

            MessageBox.Show("הרשמה הושלמה בהצלחה!", "הצלחה", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Close(); // סוגר את עמוד ההרשמה
        }
    }
}