using System;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace EmployeeSchedulingApp
{
    // דף התחברות למערכת ניהול המשמרות
    public partial class LoginPage : Form
    {
        // מחרוזת חיבור לבסיס הנתונים
        private static string connectionString = "Data Source=(localdb)\\mssqllocaldb;Initial Catalog=EmployeeScheduling;Integrated Security=True";
        // מופע של מחלקת העזר לבסיס הנתונים
        private static DataBaseHelper helper = new DataBaseHelper();

        // בנאי המחלקה - יוצר את דף ההתחברות
        // פרמטרים: אין
        // ערך מוחזר: אין
        public LoginPage()
        {
            SetupUI();
        }

        // הגדרת ממשק המשתמש של דף ההתחברות
        // פרמטרים: אין
        // ערך מוחזר: אין
        private void SetupUI()
        {
            this.Text = "התחברות למערכת";
            this.Size = new System.Drawing.Size(400, 300);
            this.RightToLeft = RightToLeft.Yes;
            this.CenterToScreen();
            this.RightToLeftLayout = true;

            // כותרת
            Label titleLabel = new Label()
            {
                Text = "התחבר למערכת",
                AutoSize = false,
                Font = new System.Drawing.Font("Arial", 14, System.Drawing.FontStyle.Bold),
                Width = this.ClientSize.Width,
                Location = new System.Drawing.Point(150, 20)
            };

            int labelX = 50, inputX = 150, gapY = 40;
            int currentY = 70;

            // שם משתמש
            Label userLabel = new Label()
            {
                Text = "שם משתמש:",
                Location = new System.Drawing.Point(labelX, currentY),
                AutoSize = true
            };
            TextBox userTextBox = new TextBox()
            {
                Location = new System.Drawing.Point(inputX, currentY),
                Width = 180,
                TextAlign = HorizontalAlignment.Right
            };
            currentY += gapY;

            // סיסמה
            Label passLabel = new Label()
            {
                Text = "סיסמה:",
                Location = new System.Drawing.Point(labelX, currentY),
                AutoSize = true
            };
            TextBox passTextBox = new TextBox()
            {
                Location = new System.Drawing.Point(inputX, currentY),
                Width = 180,
                PasswordChar = '*',
                TextAlign = HorizontalAlignment.Right
            };
            currentY += gapY + 10;

            // כפתור התחבר
            Button loginButton = new Button()
            {
                Text = "התחבר",
                Size = new System.Drawing.Size(100, 40),
                Location = new System.Drawing.Point(inputX, currentY),
                RightToLeft = RightToLeft.Yes
            };
            loginButton.Click += (sender, e) => {
                if (helper.PerformLogin(userTextBox.Text, passTextBox.Text))
                    this.Close();
            };

            // הוספת כל הרכיבים לטופס
            this.Controls.Add(titleLabel);
            this.Controls.Add(userLabel);
            this.Controls.Add(userTextBox);
            this.Controls.Add(passLabel);
            this.Controls.Add(passTextBox);
            this.Controls.Add(loginButton);
        }
    }
}