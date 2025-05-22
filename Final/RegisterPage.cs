using System;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Drawing;

namespace EmployeeSchedulingApp
{
    // דף הרשמה למערכת ניהול המשמרות
    public partial class RegisterPage : Form
    {
        // מופע של מחלקת העזר לבסיס הנתונים
        private static DataBaseHelper helper = new DataBaseHelper();

        // בנאי המחלקה - יוצר את דף ההרשמה
        // פרמטרים: אין
        // ערך מוחזר: אין
        public RegisterPage()
        {
            SetupUI();
        }

        // הגדרת ממשק המשתמש של דף ההרשמה
        // פרמטרים: אין
        // ערך מוחזר: אין

        private void SetupUI()
        {
            this.BackColor = Color.Tan;
            this.CenterToScreen();
            this.Text = "הרשמה למערכת";
            this.Size = new System.Drawing.Size(400, 450);


            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;

            // כותרת  
            Label titleLabel = new Label()
            {
                Text = "צור חשבון חדש",
                AutoSize = false,
                Font = new System.Drawing.Font("Arial", 14, System.Drawing.FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Width = this.ClientSize.Width,
                Location = new System.Drawing.Point(0, 20)
            };

            

            // שם מלא
            Label fullNameLabel = new Label() { Text = "שם מלא:", Location = new Point(50, 70), AutoSize = true };
            TextBox fullNameTextBox = new TextBox() { Location = new Point(150, 70), Width = 180 };
            fullNameTextBox.TextAlign = HorizontalAlignment.Right;

            // שם משתמש
            Label userLabel = new Label() { Text = "שם משתמש:", Location = new Point(50, 110), AutoSize = true };
            TextBox userTextBox = new TextBox() { Location = new Point(150, 110), Width = 180 };
            userTextBox.TextAlign = HorizontalAlignment.Right;

            // אימייל
            Label emailLabel = new Label() { Text = "אימייל:", Location = new Point(50, 150), AutoSize = true };
            TextBox emailTextBox = new TextBox() { Location = new Point(150, 150), Width = 180 };
            emailTextBox.TextAlign = HorizontalAlignment.Right;

            // סיסמה
            Label passLabel = new Label() { Text = "סיסמה:", Location = new Point(50, 190), AutoSize = true };
            TextBox passTextBox = new TextBox() { Location = new Point(150, 190), Width = 180, PasswordChar = '*' };
            passTextBox.TextAlign = HorizontalAlignment.Right;

            // אימות סיסמה
            Label confirmPassLabel = new Label() { Text = "אימות סיסמה:", Location = new Point(50, 230), AutoSize = true };
            TextBox confirmPassTextBox = new TextBox() { Location = new Point(150, 230), Width = 180, PasswordChar = '*' };
            confirmPassTextBox.TextAlign = HorizontalAlignment.Right;

            // כפתורי פעולה
            Button registerButton = new Button()
            {
                Text = "הרשמה",
                Size = new Size(100, 40),
                Location = new Point(150, 280),
                RightToLeft = RightToLeft.Yes,
                BackColor = Color.White,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat
            };
            registerButton.Click += (s, e) => {
                if (helper.PerformRegistration(
                      userTextBox.Text,
                      passTextBox.Text,
                      confirmPassTextBox.Text,
                      fullNameTextBox.Text,
                      emailTextBox.Text
                  ))
                {
                    this.Close();
                }
            };

          
       

            // הוספת רכיבים
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
        }

        // אתחול הרכיבים של הטופס
        // פרמטרים: אין
        // ערך מוחזר: אין

        private void InitializeComponent()
        {
            this.SuspendLayout();

            this.ClientSize = new System.Drawing.Size(282, 253);
            this.Name = "RegisterPage";
            this.ResumeLayout(false);

        }
    }
}