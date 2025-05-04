using Final;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace Final
{
    // דף להוספת עובד חדש למערכת
    public partial class AddEmployeePage : Form
    {
        // רשימת הסניפים לבחירה
        private CheckedListBox branchesCheckedListBox;

        // שם המשתמש הנוכחי
        private string currentUserName;

        // מחרוזת חיבור לבסיס הנתונים
        private static string connectionString = "Data Source=(localdb)\\mssqllocaldb;Initial Catalog=EmployeeScheduling;Integrated Security=True";

        // מילון לשמירת המשמרות לפי סניף
        private Dictionary<string, List<ShiftDisplayInfo>> branchShifts;

        // מופע של מחלקת העזר לבסיס הנתונים
        private static DataBaseHelper helper = new DataBaseHelper();

        // בנאי של המחלקה - יוצר טופס הוספת עובד חדש
        // פרמטרים
        // userName - שם המשתמש המחובר למערכת
        // ערך מוחזר: אין
        public AddEmployeePage(String userName)
        {
            InitializeComponent();
            currentUserName = userName;
            SetupUI();
            LoadAvaliableBranches();
        }

        // מגדיר את ממשק המשתמש של הטופס
        // פרמטרים: אין
        // ערך מוחזר: אין
        private void SetupUI()
        {
            this.Text = "הוספת עובד חדש";
            this.Size = new System.Drawing.Size(400, 600); // הקטנת הגובה כי אין משמרות

            Label titleLabel = new Label()
            {
                Text = "הוספת עובד חדש",
                AutoSize = true,
                Font = new System.Drawing.Font("Arial", 14, System.Drawing.FontStyle.Bold),
                Location = new System.Drawing.Point(120, 20)
            };

            // מיקומים מסודרים עם מרווחים עקביים
            int startY = 70;
            int gap = 40; // מרווח אחיד
            int currentY = startY;
            int labelX = 50;
            int controlX = 150;
            int controlWidth = 180;

            // מזהה העובד
            Label idLabel = new Label() { Text = "מזהה עובד:", Location = new System.Drawing.Point(labelX, currentY) };
            TextBox idTextBox = new TextBox() { Location = new System.Drawing.Point(controlX, currentY), Width = controlWidth, Name = "idTextBox" };
            currentY += gap;

            // שם העובד
            Label nameLabel = new Label() { Text = "שם העובד:", Location = new System.Drawing.Point(labelX, currentY) };
            TextBox nameTextBox = new TextBox() { Location = new System.Drawing.Point(controlX, currentY), Width = controlWidth, Name = "nameTextBox" };
            currentY += gap;

            // טלפון
            Label phoneLabel = new Label() { Text = "טלפון:", Location = new System.Drawing.Point(labelX, currentY) };
            TextBox phoneTextBox = new TextBox() { Location = new System.Drawing.Point(controlX, currentY), Width = controlWidth, Name = "phoneTextBox" };
            currentY += gap;

            // אימייל
            Label emailLabel = new Label() { Text = "אימייל:", Location = new System.Drawing.Point(labelX, currentY) };
            TextBox emailTextBox = new TextBox() { Location = new System.Drawing.Point(controlX, currentY), Width = controlWidth, Name = "emailTextBox" };
            currentY += gap;

            // סיסמא
            Label passwordLabel = new Label() { Text = "סיסמה:", Location = new System.Drawing.Point(labelX, currentY) };
            TextBox passwordTextBox = new TextBox()
            {
                Location = new System.Drawing.Point(controlX, currentY),
                Width = controlWidth,
                Name = "passwordTextBox",
                PasswordChar = '*'
            };
            currentY += gap;

            // תפקידים 
            Label roleLabel = new Label() { Text = "תפקידים:", Location = new System.Drawing.Point(labelX, currentY) };
            CheckedListBox rolesCheckedListBox = new CheckedListBox()
            {
                Location = new System.Drawing.Point(controlX, currentY),
                Width = controlWidth,
                Height = 80, // גובה מתאים להצגת מספר פריטים
                CheckOnClick = true,
                Name = "rolesCheckedListBox"
            };

            // הוספת התפקידים האפשריים
            List<string> roles = helper.getRoles();
            rolesCheckedListBox.Items.AddRange(roles.ToArray());

            currentY += rolesCheckedListBox.Height + 5; // התאמת המיקום 

            // שכר שעתי
            Label salaryLabel = new Label() { Text = "שכר שעתי:", Location = new System.Drawing.Point(labelX, currentY) };
            TextBox salaryTextBox = new TextBox() { Location = new System.Drawing.Point(controlX, currentY), Width = controlWidth, Name = "salaryTextBox" };
            currentY += gap;

            // ציון עובד
            Label rateLabel = new Label() { Text = "ציון עובד:", Location = new System.Drawing.Point(labelX, currentY) };
            TextBox rateTextBox = new TextBox() { Location = new System.Drawing.Point(controlX, currentY), Width = controlWidth, Name = "rateTextBox" };
            currentY += gap;

            // האם עובד מנוסה 
            CheckBox isExperiencedCheckBox = new CheckBox()
            {
                Text = "האם עובד מנוסה?",
                Location = new System.Drawing.Point(controlX, currentY),
                Name = "isExperiencedCheckBox",
                AutoSize = true

            };
            currentY += gap;

            // בחירת סניפים 
            Label branchesLabel = new Label() { Text = "בחר סניפים:", Location = new System.Drawing.Point(labelX, currentY) };
            currentY += 20; 
            branchesCheckedListBox = new CheckedListBox()
            {
                Location = new System.Drawing.Point(labelX, currentY),
                Width = 280,
                Height = 80,
                CheckOnClick = true,
                Name = "branchesCheckedListBox"
            };
            currentY += branchesCheckedListBox.Height + 20;

            // כפתור שמירה
            Button saveButton = new Button()
            {
                Text = "שמור",
                Size = new System.Drawing.Size(100, 40),
                Location = new System.Drawing.Point(controlX, currentY),
                Name = "saveButton"
            };
            saveButton.Click += SaveButton_Click;

            // כפתור ביטול
            Button cancelButton = new Button()
            {
                Text = "ביטול",
                Size = new System.Drawing.Size(100, 40),
                Location = new System.Drawing.Point(labelX, currentY),
                Name = "cancelButton"
            };
            cancelButton.Click += (sender, e) => { this.Close(); };

            // הוספת כל הרכיבים לטופס
            this.Controls.Add(idLabel);
            this.Controls.Add(idTextBox);
            this.Controls.Add(titleLabel);
            this.Controls.Add(nameLabel);
            this.Controls.Add(nameTextBox);
            this.Controls.Add(phoneLabel);
            this.Controls.Add(phoneTextBox);
            this.Controls.Add(emailLabel);
            this.Controls.Add(emailTextBox);
            this.Controls.Add(roleLabel);
            this.Controls.Add(salaryLabel);
            this.Controls.Add(salaryTextBox);
            this.Controls.Add(rateLabel);
            this.Controls.Add(rateTextBox);
            this.Controls.Add(isExperiencedCheckBox);
            this.Controls.Add(branchesLabel);
            this.Controls.Add(branchesCheckedListBox);
            this.Controls.Add(saveButton);
            this.Controls.Add(cancelButton);
            this.Controls.Add(passwordLabel);
            this.Controls.Add(passwordTextBox);
            this.Controls.Add(roleLabel);
            this.Controls.Add(rolesCheckedListBox);
        }

        // "מטפל באירוע לחיצה על כפתור "שמור
        // פרמטרים
        // sender - האובייקט שהפעיל את האירוע
        // e - נתוני האירוע
        // ערך מוחזר: אין
        private void SaveButton_Click(object sender, EventArgs e)
        {
            // יצירת רשימת התפקידים שנבחרו
            HashSet<string> selectedRoles = GetSelectedRoles();

            // ניסיון להוספת העובד לבסיס הנתונים
            TextBox idTextBox = (TextBox)this.Controls["idTextBox"];
            TextBox nameTextBox = (TextBox)this.Controls["nameTextBox"];
            TextBox phoneTextBox = (TextBox)this.Controls["phoneTextBox"];
            TextBox emailTextBox = (TextBox)this.Controls["emailTextBox"];
            TextBox rateTextBox = (TextBox)this.Controls["rateTextBox"];
            TextBox salaryTextBox = (TextBox)this.Controls["salaryTextBox"];
            TextBox passwordTextBox = (TextBox)this.Controls["passwordTextBox"];
            CheckBox isExperiencedCheckBox = (CheckBox)this.Controls["isExperiencedCheckBox"];

            if (helper.AddEmployee(
                idTextBox.Text,
                nameTextBox.Text,
                phoneTextBox.Text,
                emailTextBox.Text,
                rateTextBox.Text,
                selectedRoles,
                salaryTextBox.Text,
                isExperiencedCheckBox.Checked,
                passwordTextBox.Text,
                branchesCheckedListBox))
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        // אוסף את התפקידים שנבחרו ברשימה
        // פרמטרים: אין
        // ערך מוחזר: אוסף התפקידים שנבחרו
        private HashSet<string> GetSelectedRoles()
        {
            CheckedListBox rolesCheckedListBox = (CheckedListBox)this.Controls["rolesCheckedListBox"];
            HashSet<string> selectedRoles = new HashSet<string>();

            foreach (var item in rolesCheckedListBox.CheckedItems)
            {
                selectedRoles.Add(item.ToString());
            }

            return selectedRoles;
        }

        // טוען את הסניפים הזמינים למשתמש
        // פרמטרים: אין
        // ערך מוחזר: אין
        private void LoadAvaliableBranches()
        {
            List<Branch> branches = helper.LoadUserBranches(currentUserName);
            foreach (Branch br in branches)
            {
                branchesCheckedListBox.Items.Add(br.Name);
            }
        }
    }
}