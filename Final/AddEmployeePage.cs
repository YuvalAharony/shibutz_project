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
    public partial class AddEmployeePage : Form
    {
        private CheckedListBox branchesCheckedListBox;
        private string currentUserName;
        private static string connectionString = "Data Source=(localdb)\\mssqllocaldb;Initial Catalog=EmployeeScheduling;Integrated Security=True";
        private Dictionary<string, List<ShiftDisplayInfo>> branchShifts; // מילון לשמירת המשמרות לפי סניף
        private static DataBaseHelper helper = new DataBaseHelper();

        public AddEmployeePage(String userName)
        {
            InitializeComponent();
            currentUserName = userName;
            SetupUI();
            LoadAvaliableBranches();


        }


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

            Label passwordLabel = new Label() { Text = "סיסמה:", Location = new System.Drawing.Point(labelX, currentY) };
            TextBox passwordTextBox = new TextBox()
            {
                Location = new System.Drawing.Point(controlX, currentY),
                Width = controlWidth,
                Name = "passwordTextBox",
                PasswordChar = '*'
            };
            currentY += gap;

            // תפקידים (CheckedListBox)
            Label roleLabel = new Label() { Text = "תפקידים:", Location = new System.Drawing.Point(labelX, currentY) };

            // יצירת CheckedListBox לתפקידים
            CheckedListBox rolesCheckedListBox = new CheckedListBox()
            {
                Location = new System.Drawing.Point(controlX, currentY),
                Width = controlWidth,
                Height = 80, // גובה מתאים להצגת מספר פריטים
                CheckOnClick = true,
                Name = "rolesCheckedListBox"
            };

            // הוספת התפקידים האפשריים
            string[] roles = { "Waiter", "Chef", "Bartender", "Manager" };
            rolesCheckedListBox.Items.AddRange(roles);

            currentY += rolesCheckedListBox.Height + 5; // התאמת המיקום האנכי


            // שכר שעתי
            Label salaryLabel = new Label() { Text = "שכר שעתי:", Location = new System.Drawing.Point(labelX, currentY) };
            TextBox salaryTextBox = new TextBox() { Location = new System.Drawing.Point(controlX, currentY), Width = controlWidth, Name = "salaryTextBox" };
            currentY += gap;

            // ציון עובד
            Label rateLabel = new Label() { Text = "ציון עובד:", Location = new System.Drawing.Point(labelX, currentY) };
            TextBox rateTextBox = new TextBox() { Location = new System.Drawing.Point(controlX, currentY), Width = controlWidth, Name = "rateTextBox" };
            currentY += gap;

            // האם מנוסה (CheckBox)
            CheckBox isExperiencedCheckBox = new CheckBox()
            {
                Text = "האם עובד מנוסה?",
                Location = new System.Drawing.Point(controlX, currentY),
                Name = "isExperiencedCheckBox"
            };
            currentY += gap;

            // בחירת סניפים - שימוש בשדה שכבר הוגדר במחלקה
            Label branchesLabel = new Label() { Text = "בחר סניפים:", Location = new System.Drawing.Point(labelX, currentY) };
            currentY += 20; // מרווח קטן לפני הרשימה

            // אתחול branchesCheckedListBox שכבר הוגדר כשדה של המחלקה
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
            saveButton.Click += (sender, e) => {
                // יצירת רשימת התפקידים שנבחרו
                HashSet<string> selectedRoles = new HashSet<string>();
                foreach (var item in rolesCheckedListBox.CheckedItems)
                {
                    selectedRoles.Add(item.ToString());
                }


                if(
                helper.AddEmployee(
                    idTextBox.Text,
                    nameTextBox.Text,
                    phoneTextBox.Text,
                    emailTextBox.Text,
                    rateTextBox.Text,
                    selectedRoles,
                    salaryTextBox.Text,
                    isExperiencedCheckBox.Checked,
                    passwordTextBox.Text,
                    branchesCheckedListBox
                ))
                {
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
               
            };

            // כפתור ביטול
            Button cancelButton = new Button()
            {
                Text = "ביטול",
                Size = new System.Drawing.Size(100, 40),
                Location = new System.Drawing.Point(labelX, currentY),
                Name = "cancelButton"
            };
            cancelButton.Click += (sender, e) => { this.Close(); };

            // הוספת כל הפקדים לטופס
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