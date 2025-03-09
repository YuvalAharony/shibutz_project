using System;
using System.Windows.Forms;

namespace EmployeeSchedulingApp
{
    public partial class LoginPage : Form
    {
        public LoginPage()
        {
            SetupUI();
        }

        private void SetupUI()
        {
            this.Text = "התחברות למערכת";
            this.Size = new System.Drawing.Size(400, 300);

            Label titleLabel = new Label()
            {
                Text = "התחבר למערכת",
                AutoSize = true,
                Font = new System.Drawing.Font("Arial", 14, System.Drawing.FontStyle.Bold),
                Location = new System.Drawing.Point(120, 20)
            };

            Label userLabel = new Label() { Text = "שם משתמש:", Location = new System.Drawing.Point(50, 70) };
            TextBox userTextBox = new TextBox() { Location = new System.Drawing.Point(150, 70), Width = 180 };

            Label passLabel = new Label() { Text = "סיסמה:", Location = new System.Drawing.Point(50, 110) };
            TextBox passTextBox = new TextBox() { Location = new System.Drawing.Point(150, 110), Width = 180, PasswordChar = '*' };

            Button loginButton = new Button()
            {
                Text = "התחבר",
                Size = new System.Drawing.Size(100, 40),
                Location = new System.Drawing.Point(150, 160)
            };
            loginButton.Click += (sender, e) => { PerformLogin(userTextBox.Text, passTextBox.Text); };

            this.Controls.Add(titleLabel);
            this.Controls.Add(userLabel);
            this.Controls.Add(userTextBox);
            this.Controls.Add(passLabel);
            this.Controls.Add(passTextBox);
            this.Controls.Add(loginButton);
        }

        private void PerformLogin(string username, string password)
        {
            if (username == "1" && password == "1") // דוגמה לבדיקה בסיסית
            {
                MessageBox.Show("התחברות מוצלחת!");
                MainPage main = new MainPage();
                main.Show();
                this.Hide();
            }
            else
            {
                MessageBox.Show("שם משתמש או סיסמה שגויים.", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoginPage_Load(object sender, EventArgs e)
        {

        }
    }
}