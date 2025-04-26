using System;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Drawing;


namespace EmployeeSchedulingApp
{
    public partial class RegisterPage : Form
    {
        private static string connectionString = "Data Source=(localdb)\\mssqllocaldb;Initial Catalog=EmployeeScheduling;Integrated Security=True";
        private static DataBaseHelper helper = new DataBaseHelper();


        public RegisterPage()
        {
            SetupUI();
        }

        private void SetupUI()
        {
            this.Text = "הרשמה למערכת";
            this.Size = new System.Drawing.Size(400, 450);

            // 1) הפיכת כל ה-Form ל-RTL
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;

            // כותרת קבועה ומרוכזת
            Label titleLabel = new Label()
            {
                Text = "צור חשבון חדש",
                AutoSize = false,
                Font = new System.Drawing.Font("Arial", 14, System.Drawing.FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Width = this.ClientSize.Width,
                Location = new System.Drawing.Point(0, 20)
            };

            int labelX = 50, inputX = 150, gapY = 40, currentY = 70;

            // שם מלא
            Label fullNameLabel = new Label() { Text = "שם מלא:", Location = new Point(labelX, currentY), AutoSize = true };
            TextBox fullNameTextBox = new TextBox() { Location = new Point(inputX, currentY), Width = 180 };
            fullNameTextBox.TextAlign = HorizontalAlignment.Right;

            currentY += gapY;
            // שם משתמש
            Label userLabel = new Label() { Text = "שם משתמש:", Location = new Point(labelX, currentY), AutoSize = true };
            TextBox userTextBox = new TextBox() { Location = new Point(inputX, currentY), Width = 180 };
            userTextBox.TextAlign = HorizontalAlignment.Right;

            currentY += gapY;
            // אימייל
            Label emailLabel = new Label() { Text = "אימייל:", Location = new Point(labelX, currentY), AutoSize = true };
            TextBox emailTextBox = new TextBox() { Location = new Point(inputX, currentY), Width = 180 };
            emailTextBox.TextAlign = HorizontalAlignment.Right;

            currentY += gapY;
            // סיסמה
            Label passLabel = new Label() { Text = "סיסמה:", Location = new Point(labelX, currentY), AutoSize = true };
            TextBox passTextBox = new TextBox() { Location = new Point(inputX, currentY), Width = 180, PasswordChar = '*' };
            passTextBox.TextAlign = HorizontalAlignment.Right;

            currentY += gapY;
            // אימות סיסמה
            Label confirmPassLabel = new Label() { Text = "אימות סיסמה:", Location = new Point(labelX, currentY), AutoSize = true };
            TextBox confirmPassTextBox = new TextBox() { Location = new Point(inputX, currentY), Width = 180, PasswordChar = '*' };
            confirmPassTextBox.TextAlign = HorizontalAlignment.Right;

            currentY += gapY + 10;
            // כפתורי פעולה
            Button registerButton = new Button()
            {
                Text = "הרשם",
                Size = new Size(100, 40),
                Location = new Point(inputX, currentY),
                RightToLeft = RightToLeft.Yes
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

            Button cancelButton = new Button()
            {
                Text = "ביטול",
                Size = new Size(100, 40),
                Location = new Point(labelX, currentY),
                RightToLeft = RightToLeft.Yes
            };
            cancelButton.Click += (s, e) => {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            };

            // הוספה ל-Form
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


    }
}