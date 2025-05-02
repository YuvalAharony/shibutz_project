using System;
using Final;
using System.Windows.Forms;

namespace EmployeeSchedulingApp
{
    public partial class HomePage : Form
    {
        public HomePage()
        {
            SetupUI();
        }

        private void SetupUI()
        {
            this.Text = "ברוך הבא למערכת ניהול המשמרות";
            this.Size = new System.Drawing.Size(800, 500);

            Label welcomeLabel = new Label()
            {
                Text = "ברוך הבא למערכת ניהול המשמרות!",
                AutoSize = true,
                Font = new System.Drawing.Font("Arial", 16, System.Drawing.FontStyle.Bold),
                Location = new System.Drawing.Point(250, 50)
            };

            Button loginButton = new Button()
            {
                Text = "התחברות",
                Size = new System.Drawing.Size(200, 50),
                Location = new System.Drawing.Point(300, 150)
            };
            loginButton.Click += (sender, e) => { OpenLoginPage(); };

            Button registerButton = new Button()
            {
                Text = "הרשמה",
                Size = new System.Drawing.Size(200, 50),
                Location = new System.Drawing.Point(300, 220)
            };
            registerButton.Click += (sender, e) => { OpenRegisterPage(); };

            this.Controls.Add(welcomeLabel);
            this.Controls.Add(loginButton);
            this.Controls.Add(registerButton);
        }

        private void OpenLoginPage()
        {
            LoginPage login = new LoginPage();
            login.Show();
            this.Hide();
        }

        private void OpenRegisterPage()
        {
            RegisterPage register = new RegisterPage();
            register.Show();
            this.Hide();
        }

       
    }
}