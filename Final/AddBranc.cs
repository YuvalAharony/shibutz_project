using Final;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace EmployeeSchedulingApp
{
    // דף להוספת סניף חדש למערכת
    public partial class AddBranchPage : Form
    {
        // שם המשתמש הנוכחי
        private string currentUserName;

        // מופע של מחלקת העזר לבסיס הנתונים
        private static DataBaseHelper helper = new DataBaseHelper();

        // בנאי של המחלקה - יוצר טופס הוספת סניף חדש 
        // פרמטרים
        // userName - שם המשתמש המחובר למערכת 
        // ערך מוחזר: אין
        public AddBranchPage(string userName = null)
        {
            InitializeComponent();
            currentUserName = userName;
            SetupUI();
        }

        // מאתחל את הרכיבים הבסיסיים של הטופס
        // פרמטרים: אין
        // ערך מוחזר: אין
        private void InitializeComponent()
        {
            this.SuspendLayout();

            // הגדרות בסיסיות לטופס הוספת סניף
            this.ClientSize = new System.Drawing.Size(400, 450);
            this.Name = "AddBranchPage";
            this.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.Text = "הוספת סניף חדש";
            this.CenterToScreen();
            this.ResumeLayout(false);
        }

        // מגדיר את ממשק המשתמש של הטופס
        // פרמטרים: אין
        // ערך מוחזר: אין
        private void SetupUI()
        {
            this.BackColor = Color.Tan;

            // יצירת כותרת
            Label titleLabel = new Label
            {
                Text = "הוספת סניף חדש",
                Font = new Font("Arial", 14, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(120, 20)
            };
            this.Controls.Add(titleLabel);

            // יצירת תווית ושדה טקסט לשם הסניף
            Label nameLabel = new Label
            {
                Text = "שם הסניף:",
                Location = new Point(50, 70),
                AutoSize = true
            };

            TextBox nameTextBox = new TextBox
            {
                Name = "nameTextBox",
                Location = new Point(150, 70),
                Width = 180
            };

            this.Controls.Add(nameLabel);
            this.Controls.Add(nameTextBox);

            // יצירה והוספת כפתור שמירה
            Button saveButton = new Button
            {
                Text = "שמור",
                Size = new Size(100, 40),
                Location = new Point(220, 120),
                BackColor = Color.White,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat
            };
            saveButton.Click += SaveButton_Click;
            this.Controls.Add(saveButton);

            // יצירה והוספת כפתור ביטול
            Button cancelButton = new Button
            {
                Text = "ביטול",
                Size = new Size(100, 40),
                Location = new Point(80, 120),
                BackColor = Color.White,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat
            };
            cancelButton.Click += (sender, e) => { this.Close(); };
            this.Controls.Add(cancelButton);
        }

        // מטפל באירוע לחיצה על כפתור "שמור" - מאמת את הקלט ושומר את הסניף החדש
        // פרמטרים
        // sender - האובייקט שהפעיל את האירוע
        // e - נתוני האירוע
        // ערך מוחזר: אין
        private void SaveButton_Click(object sender, EventArgs e)
        {
            // קבלת ערך הטקסט משדה שם הסניף
            TextBox nameTextBox = (TextBox)this.Controls["nameTextBox"];
            string branchName = nameTextBox.Text.Trim();

            // בדיקת תקינות - ודא שהוזן שם סניף
            if (string.IsNullOrEmpty(branchName))
            {
                MessageBox.Show("נא להזין שם סניף", "שגיאה",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // ניסיון להוספת הסניף לבסיס הנתונים
            if (helper.AddBranch(branchName, currentUserName))
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }
    }
}