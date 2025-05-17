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
    // דף לעריכת פרטי עובד במערכת
    public partial class EditEmployeePage : Form
    {
        // העובד הנבחר לעריכה
        private Employee selectedEmployee;
        // שדות טקסט למידע על העובד
        private TextBox nameTextBox, idTextBox, rateTextBox, salaryTextBox;
        private TextBox phoneTextBox, emailTextBox;
        // רשימת תפקידים אפשריים לעובד
        private CheckedListBox rolesCheckedListBox; // שינוי מ-ComboBox ל-CheckedListBox
        // תיבת סימון האם העובד מנוסה
        private CheckBox isExperiencedCheckBox;
        // רשימות הסניפים והמשמרות
        private CheckedListBox branchesCheckedListBox;
        private CheckedListBox shiftsCheckedListBox;
        // כפתורי שמירה וביטול
        private Button saveButton, cancelButton;
        // אחסון המשמרות לפי סניף
        private Dictionary<string, List<ShiftDisplayInfo>> branchShifts;
        // שם המשתמש הנוכחי
        string currentUserName;
        // מופע של מחלקת העזר לבסיס הנתונים
        private static DataBaseHelper helper = new DataBaseHelper();

        // אתחול הרכיבים של הטופס
        // פרמטרים: אין
        // ערך מוחזר: אין
        private void InitializeComponent()
        {

            this.SuspendLayout();
            // 
            // EditEmployeePage
            // 
            this.ClientSize = new System.Drawing.Size(400, 750);
            this.Name = "EditEmployeePage";
            this.CenterToScreen();
            this.ResumeLayout(false);

        }

        // הגדרת ממשק המשתמש של הטופס
        // פרמטרים: אין
        // ערך מוחזר: אין
        private void SetupUI()
        {
            
            this.Text = "עריכת עובד";
            this.Size = new System.Drawing.Size(450, 800);
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;

            // מרכוז הכותרת
            Label titleLabel = new Label()
            {
                Text = "עריכת פרטי עובד",
                AutoSize = true,
                Font = new Font("Arial", 16, FontStyle.Bold),
                Location = new Point((this.ClientSize.Width - 200) / 2, 20),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // הגדרת מרחקים קבועים
            int labelX = 50;
            int controlX = 200;
            int verticalSpacing = 40;
            int currentY = 70;

            // שם העובד
            Label nameLabel = new Label()
            {
                Text = "שם העובד:",
                Location = new Point(labelX, currentY),
                AutoSize = true
            };
            nameTextBox = new TextBox()
            {
                Location = new Point(controlX, currentY),
                Width = 180
            };
            currentY += verticalSpacing;

            // טלפון
            Label phoneLabel = new Label()
            {
                Text = "טלפון:",
                Location = new Point(labelX, currentY),
                AutoSize = true
            };
            phoneTextBox = new TextBox()
            {
                Location = new Point(controlX, currentY),
                Width = 180
            };
            currentY += verticalSpacing;

            // אימייל
            Label emailLabel = new Label()
            {
                Text = "אימייל:",
                Location = new Point(labelX, currentY),
                AutoSize = true
            };
            emailTextBox = new TextBox()
            {
                Location = new Point(controlX, currentY),
                Width = 180
            };
            currentY += verticalSpacing;

            // תפקידים 
            Label roleLabel = new Label()
            {
                Text = "תפקידים:",
                Location = new Point(labelX, currentY),
                AutoSize = true
            };

            rolesCheckedListBox = new CheckedListBox()
            {
                Location = new Point(controlX, currentY),
                Width = 180,
                Height = 80,
                CheckOnClick = true
            };

            // הוספת התפקידים האפשריים
            List<string>roles = helper.getRoles();
            rolesCheckedListBox.Items.AddRange(roles.ToArray());

            currentY += rolesCheckedListBox.Height + 5;

            // שכר שעתי
            Label salaryLabel = new Label()
            {
                Text = "שכר שעתי:",
                Location = new Point(labelX, currentY),
                AutoSize = true
            };
            salaryTextBox = new TextBox()
            {
                Location = new Point(controlX, currentY),
                Width = 180
            };
            currentY += verticalSpacing;

            // ציון עובד
            Label rateLabel = new Label()
            {
                Text = "ציון עובד:",
                Location = new Point(labelX, currentY),
                AutoSize = true
            };
            rateTextBox = new TextBox()
            {
                Location = new Point(controlX, currentY),
                Width = 180
            };
            currentY += verticalSpacing;

            // האם מנוסה
            isExperiencedCheckBox = new CheckBox()
            {
                Text = "האם עובד מנוסה?",
                Location = new Point(controlX, currentY),
                AutoSize = true
            };
            currentY += verticalSpacing;

            //  סניפים
            Label branchesLabel = new Label()
            {
                Text = "סניפים:",
                Location = new Point(labelX, currentY),
                AutoSize = true
            };
            currentY += 20;
            branchesCheckedListBox = new CheckedListBox()
            {
                Location = new Point(labelX, currentY),
                Width = this.ClientSize.Width - 100,
                Height = 80,
                CheckOnClick = true
            };
            branchesCheckedListBox.ItemCheck += BranchesCheckedListBox_ItemCheck;
            currentY += 100;

            // = משמרות מועדפות
            Label shiftsLabel = new Label()
            {
                Text = "משמרות מועדפות:",
                Location = new Point(labelX, currentY),
                AutoSize = true
            };
            currentY += 20;
            shiftsCheckedListBox = new CheckedListBox()
            {
                Location = new Point(labelX, currentY),
                Width = this.ClientSize.Width - 100,
                Height = 120,
            };
            currentY += 140;

            // כפתורי שמירה וביטול
            int buttonWidth = 100;
            int buttonHeight = 40;
            int buttonSpacing = (this.ClientSize.Width - (2 * buttonWidth)) / 2;

            cancelButton = new Button()
            {
                Text = "ביטול",
                Size = new Size(buttonWidth, buttonHeight),
                Location = new Point(buttonSpacing, currentY)
            };
            cancelButton.Click += (sender, e) => { this.Close(); };

            saveButton = new Button()
            {
                Text = "שמור",
                Size = new Size(buttonWidth, buttonHeight),
                Location = new Point(buttonSpacing + buttonWidth + 20, currentY)
            };
            saveButton.Click += SaveEmployeeChanges;

            // הוספת כל הרכיבים לטופס
            this.Controls.Add(titleLabel);
            this.Controls.Add(nameLabel);
            this.Controls.Add(nameTextBox);
            this.Controls.Add(phoneLabel);
            this.Controls.Add(phoneTextBox);
            this.Controls.Add(emailLabel);
            this.Controls.Add(emailTextBox);
            this.Controls.Add(roleLabel);
            this.Controls.Add(rolesCheckedListBox);
            this.Controls.Add(salaryLabel);
            this.Controls.Add(salaryTextBox);
            this.Controls.Add(rateLabel);
            this.Controls.Add(rateTextBox);
            this.Controls.Add(isExperiencedCheckBox);
            this.Controls.Add(branchesLabel);
            this.Controls.Add(branchesCheckedListBox);
            this.Controls.Add(shiftsLabel);
            this.Controls.Add(shiftsCheckedListBox);
            this.Controls.Add(saveButton);
            this.Controls.Add(cancelButton);
        }

        // טעינת סניפים ומשמרות זמינים לעובד
        // פרמטרים: אין
        // ערך מוחזר: אין
        private void LoadBranchesAndShifts()
        {
            branchShifts = new Dictionary<string, List<ShiftDisplayInfo>>();
            List<Branch> branches = helper.LoadUserBranches(currentUserName);
            List<string> EmployeeBranches = helper.LoademployeeBranches(selectedEmployee);
            foreach (Branch br in branches)
            {
                branchesCheckedListBox.Items.Add(br.Name);
                branchShifts[br.Name] = helper.LoadBranchShiftsDetails(br.ID, br.Name);
                if (EmployeeBranches.Contains(br.Name))
                {
                    int index = branchesCheckedListBox.Items.IndexOf(br.Name);
                    if (index >= 0)
                    {
                        branchesCheckedListBox.SetItemChecked(index, true);
                    }
                }
            }
            HashSet<int> preferredShiftIds = helper.LoademployeePrefferdShifts(selectedEmployee);
            selectedEmployee.requestedShifts = preferredShiftIds;
            UpdateShiftsList();
        }

     

        // אירוע שמופעל כאשר מסמנים או מבטלים סימון של סניף
        // פרמטרים
        // sender - האובייקט שהפעיל את האירוע
        // e - נתוני האירוע
        // ערך מוחזר: אין
        private void BranchesCheckedListBox_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            // הרצה מושהית כדי לאפשר את עדכון הסימון לפני הפעולה
            this.BeginInvoke(new Action(() =>
            {
                UpdateShiftsList();
            }));
        }

        // עדכון רשימת המשמרות על פי הסניפים שנבחרו
        // פרמטרים: אין
        // ערך מוחזר: אין
        private void UpdateShiftsList()
        {
            shiftsCheckedListBox.Items.Clear();

            // איסוף כל המשמרות מהסניפים שנבחרו
            List<ShiftDisplayInfo> availableShifts = new List<ShiftDisplayInfo>();

            foreach (var checkedItem in branchesCheckedListBox.CheckedItems)
            {
                string branchName = checkedItem.ToString();
                if (branchShifts.ContainsKey(branchName))
                {
                    availableShifts.AddRange(branchShifts[branchName]);
                }
            }

            // מיון המשמרות לפי יום ושעה
            availableShifts.Sort((a, b) =>
            {
                // מיון לפי יום
                string[] days = { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
                int dayCompare = Array.IndexOf(days, a.DayOfWeek).CompareTo(Array.IndexOf(days, b.DayOfWeek));

                if (dayCompare != 0)
                    return dayCompare;

                // מיון לפי זמן
                string[] times = { "Morning", "Evening" };
                return Array.IndexOf(times, a.TimeSlot).CompareTo(Array.IndexOf(times, b.TimeSlot));
            });

            // הוספת המשמרות לרשימה ובדיקת אילו מהן מועדפות על העובד
            foreach (var shift in availableShifts)
            {
                int index = shiftsCheckedListBox.Items.Add(shift);
                if (selectedEmployee.requestedShifts.Contains(shift.ShiftID))
                {
                    shiftsCheckedListBox.SetItemChecked(index, true);
                }
            }
        }

        // טעינת נתוני העובד לטופס
        // פרמטרים: אין
        // ערך מוחזר: אין
        private void LoadEmployeeData()
        {
            nameTextBox.Text = selectedEmployee.Name;
            phoneTextBox.Text = helper.LoadEmployeePhone(selectedEmployee.ID);
            emailTextBox.Text = helper.LoadEmployeeEmail(selectedEmployee.ID);
            HashSet<string> roles = helper.LoadEmployeeRoles(selectedEmployee.ID);
            foreach (string roleName in roles)
            {
                int index = rolesCheckedListBox.Items.IndexOf(roleName);
                if (index >= 0)
                {
                    rolesCheckedListBox.SetItemChecked(index, true);
                }
            }
            // טעינת נתוני טלפון ואימייל מהדאטאבייס

            salaryTextBox.Text = selectedEmployee.HourlySalary.ToString();
            rateTextBox.Text = selectedEmployee.Rate.ToString();
            isExperiencedCheckBox.Checked = selectedEmployee.isMentor;
        }

        // שמירת השינויים לעובד
        // פרמטרים
        // sender - האובייקט שהפעיל את האירוע
        // e - נתוני האירוע
        // ערך מוחזר: אין
        private void SaveEmployeeChanges(object sender, EventArgs e)
        {
            // בדיקת שדות חובה
            if (string.IsNullOrWhiteSpace(nameTextBox.Text) ||
                rolesCheckedListBox.CheckedItems.Count == 0 || // בדיקה שנבחר לפחות תפקיד אחד
                string.IsNullOrWhiteSpace(salaryTextBox.Text) ||
                string.IsNullOrWhiteSpace(rateTextBox.Text))
            {
                MessageBox.Show("נא למלא את כל השדות הדרושים ולבחור לפחות תפקיד אחד.", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            // בדיקה שלפחות סניף אחד נבחר
            if (branchesCheckedListBox.CheckedItems.Count == 0)
            {
                MessageBox.Show("נא לבחור לפחות סניף אחד לשיבוץ העובד.", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // איסוף התפקידים הנבחרים
            List<string> selectedRoles = new List<string>();
            foreach (var item in rolesCheckedListBox.CheckedItems)
            {
                selectedRoles.Add(item.ToString());
            }

            // איסוף הסניפים הנבחרים
            List<string> selectedBranchNames = new List<string>();
            List<Branch> selectedBranches = new List<Branch>();
            foreach (var branch in branchesCheckedListBox.CheckedItems)
            {
                string branchName = branch.ToString();
                selectedBranchNames.Add(branchName);

                // קבלת האובייקט Branch המתאים
                Branch matchingBranch = helper.LoadUserBranches(currentUserName)
                    .FirstOrDefault(b => b.Name == branchName);

                if (matchingBranch != null)
                {
                    selectedBranches.Add(matchingBranch);
                }
            }

            // איסוף המשמרות המועדפות
            List<ShiftDisplayInfo> selectedShifts = new List<ShiftDisplayInfo>();
            foreach (ShiftDisplayInfo shift in shiftsCheckedListBox.CheckedItems)
            {
                selectedShifts.Add(shift);
            }

            // שמירת השינויים לעובד
            if (helper.SaveEmployeeToDataBase(
                selectedEmployee,
                nameTextBox.Text,
                phoneTextBox.Text,
                emailTextBox.Text,
                salaryTextBox.Text,
                rateTextBox.Text,
                isExperiencedCheckBox.Checked,
                selectedRoles,
                selectedBranches,
                selectedShifts))
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        // בנאי המחלקה - יוצר טופס עריכת עובד
        // פרמטרים
        // employee - העובד שיש לערוך
        // userName - שם המשתמש המחובר למערכת
        // ערך מוחזר: אין
        public EditEmployeePage(Employee employee, string userName)
        {
            selectedEmployee = employee;
            InitializeComponent();
            branchShifts = new Dictionary<string, List<ShiftDisplayInfo>>();
            SetupUI();
            currentUserName = userName;

            // מנטרל את האירוע זמנית
            branchesCheckedListBox.ItemCheck -= BranchesCheckedListBox_ItemCheck;

            // טעינת הסניפים והמשמרות
            LoadBranchesAndShifts();

            // טעינת נתוני העובד
            LoadEmployeeData();

            // חיבור האירוע מחדש אחרי שכל הנתונים טעונים
            branchesCheckedListBox.ItemCheck += BranchesCheckedListBox_ItemCheck;
        }
    }
}