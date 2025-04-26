using Final;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Final
{
    public partial class AddEmployeePage : Form
    {
        private CheckedListBox branchesCheckedListBox;
        private CheckedListBox shiftsCheckedListBox;  // רשימה חדשה לבחירת משמרות
        private string currentUserName;
        private static string connectionString = "Data Source=(localdb)\\mssqllocaldb;Initial Catalog=EmployeeScheduling;Integrated Security=True";
        private Dictionary<string, List<ShiftDisplayInfo>> branchShifts; // מילון לשמירת המשמרות לפי סניף
        private static DataBaseHelper helper = new DataBaseHelper();

        public AddEmployeePage(String userName)
        {
            InitializeComponent();
            currentUserName = userName;
            branchShifts = new Dictionary<string, List<ShiftDisplayInfo>>();
            SetupUI();
            LoadAvailableBranches();
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

                SaveEmployee(
                    idTextBox.Text,
                    nameTextBox.Text,
                    phoneTextBox.Text,
                    emailTextBox.Text,
                    rateTextBox.Text,
                    selectedRoles,
                    salaryTextBox.Text,
                    isExperiencedCheckBox.Checked,
                    passwordTextBox.Text
                );
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

        // פונקציה לטעינת הסניפים הזמינים למשתמש הנוכחי
        private void LoadAvailableBranches()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = @"
                        SELECT b.BranchID, b.Name 
                        FROM Branches b
                        INNER JOIN UserBranches ub ON b.BranchID = ub.BranchID
                        INNER JOIN Users u ON ub.UserID = u.UserID
                        WHERE u.Username = @Username";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Username", currentUserName);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            branchesCheckedListBox.Items.Clear();

                            while (reader.Read())
                            {
                                string branchName = reader.GetString(1);
                                branchesCheckedListBox.Items.Add(branchName);

                                // טעינת המשמרות של הסניף
                                LoadBranchShifts(reader.GetInt32(0), branchName);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("אירעה שגיאה בטעינת הסניפים: " + ex.Message, "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // פונקציה לטעינת המשמרות של סניף מסוים
        private void LoadBranchShifts(int branchId, string branchName)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = @"
                        SELECT s.ShiftID, ts.TimeSlotName, s.DayOfWeek, st.TypeName
                        FROM Shifts s
                        INNER JOIN ShiftTypes st ON s.ShiftTypeID = st.ShiftTypeID
                        INNER JOIN TimeSlots ts ON s.TimeSlotID=ts.TimeSlotID 
                        WHERE s.BranchID = @BranchID";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@BranchID", branchId);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            List<ShiftDisplayInfo> shifts = new List<ShiftDisplayInfo>();

                            while (reader.Read())
                            {
                                shifts.Add(new ShiftDisplayInfo
                                {
                                    ShiftID = reader.GetInt32(0),
                                    BranchName = branchName,
                                    TimeSlot = reader.GetString(1),
                                    DayOfWeek = reader.GetString(2),
                                    ShiftType = reader.GetString(3)
                                });
                            }

                            branchShifts[branchName] = shifts;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("אירעה שגיאה בטעינת המשמרות: " + ex.Message, "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // אירוע שמופעל כאשר מסמנים או מבטלים סימון של סניף

        private void SaveEmployee(
        string employeeId,
        string name,
        string phone,
        string email,
        string rate,
        HashSet<string> roles,
        string salary,
        bool isExperienced,
        string password)
        {
            // בדיקת תקינות שדות
            if (string.IsNullOrWhiteSpace(name) ||
              roles.Count == 0 ||  // בדיקה שנבחר לפחות תפקיד אחד
              string.IsNullOrWhiteSpace(salary) ||
              string.IsNullOrWhiteSpace(rate) ||
              string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("נא למלא את כל השדות הדרושים ולבחור לפחות תפקיד אחד.", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int newEmployeeId = 0;
            bool useCustomId = false;

            if (!string.IsNullOrWhiteSpace(employeeId))
            {
                if (!int.TryParse(employeeId, out newEmployeeId))
                {
                    MessageBox.Show("מזהה העובד חייב להיות מספר.", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // בדיקה אם המזהה כבר קיים במערכת
                using (SqlConnection checkConnection = new SqlConnection(connectionString))
                {
                    checkConnection.Open();
                    string checkQuery = "SELECT COUNT(*) FROM Employees WHERE EmployeeID = @EmployeeID";
                    using (SqlCommand checkCommand = new SqlCommand(checkQuery, checkConnection))
                    {
                        checkCommand.Parameters.AddWithValue("@EmployeeID", newEmployeeId);
                        int count = (int)checkCommand.ExecuteScalar();

                        if (count > 0)
                        {
                            MessageBox.Show("מזהה העובד כבר קיים במערכת.", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }
                }

                useCustomId = true;
            }

            // איסוף הסניפים שנבחרו
            List<string> branchList = new List<string>();
            foreach (var item in branchesCheckedListBox.CheckedItems)
            {
                branchList.Add(item.ToString());
            }

            if (branchList.Count == 0)
            {
                MessageBox.Show("נא לבחור לפחות סניף אחד.", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string insertEmployeeQuery;

                    if (useCustomId)
                    {
                        // שימוש במזהה מותאם אישית
                        insertEmployeeQuery = @"
                SET IDENTITY_INSERT Employees ON;
                INSERT INTO Employees (EmployeeID, Name, Phone, Email, HourlySalary, Rate, IsMentor, Password)
                VALUES (@EmployeeID, @Name, @Phone, @Email, @HourlySalary, @Rate, @IsMentor, @Password);
                SET IDENTITY_INSERT Employees OFF;
                SELECT @EmployeeID;";
                    }
                    else
                    {
                        // שימוש במזהה אוטומטי
                        insertEmployeeQuery = @"
                INSERT INTO Employees (Name, Phone, Email, HourlySalary, Rate, IsMentor, Password)
                VALUES (@Name, @Phone, @Email, @HourlySalary, @Rate, @IsMentor, @Password);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";
                    }

                    using (SqlCommand command = new SqlCommand(insertEmployeeQuery, connection))
                    {
                        if (useCustomId)
                        {
                            command.Parameters.AddWithValue("@EmployeeID", newEmployeeId);
                        }

                        command.Parameters.AddWithValue("@Name", name);
                        command.Parameters.AddWithValue("@Phone", string.IsNullOrEmpty(phone) ? (object)DBNull.Value : phone);
                        command.Parameters.AddWithValue("@Email", string.IsNullOrEmpty(email) ? (object)DBNull.Value : email);
                        command.Parameters.AddWithValue("@HourlySalary", Convert.ToDecimal(salary));
                        command.Parameters.AddWithValue("@Rate", Convert.ToInt32(rate));
                        command.Parameters.AddWithValue("@IsMentor", isExperienced);
                        command.Parameters.AddWithValue("@AssignedHours", 7); // ערך ברירת מחדל
                        command.Parameters.AddWithValue("@Password", password);

                        // קבלת ה-ID של העובד החדש
                        newEmployeeId = (int)command.ExecuteScalar();
                    }

                    // הוספת התפקידים של העובד לטבלת EmployeeRoles
                    if (roles != null && roles.Count > 0)
                    {
                        foreach (string roleName in roles)
                        {
                            // בדיקה אם התפקיד קיים בטבלת Roles, אם לא - הוספתו
                            int roleId;
                            string checkRoleQuery = "SELECT RoleID FROM Roles WHERE RoleName = @RoleName";
                            using (SqlCommand command = new SqlCommand(checkRoleQuery, connection))
                            {
                                command.Parameters.AddWithValue("@RoleName", roleName);
                                object result = command.ExecuteScalar();

                                if (result == null) // התפקיד לא קיים
                                {
                                    string insertRoleQuery = "INSERT INTO Roles (RoleName) VALUES (@RoleName); SELECT CAST(SCOPE_IDENTITY() AS INT)";
                                    using (SqlCommand insertCommand = new SqlCommand(insertRoleQuery, connection))
                                    {
                                        insertCommand.Parameters.AddWithValue("@RoleName", roleName);
                                        roleId = (int)insertCommand.ExecuteScalar();
                                    }
                                }
                                else
                                {
                                    roleId = (int)result;
                                }
                            }

                            // קישור העובד לתפקיד
                            string insertEmployeeRoleQuery = "INSERT INTO EmployeeRoles (EmployeeID, RoleID) VALUES (@EmployeeID, @RoleID)";
                            using (SqlCommand command = new SqlCommand(insertEmployeeRoleQuery, connection))
                            {
                                command.Parameters.AddWithValue("@EmployeeID", newEmployeeId);
                                command.Parameters.AddWithValue("@RoleID", roleId);
                                command.ExecuteNonQuery();
                            }
                        }
                    }

                    // קישור העובד לסניפים שנבחרו
                    foreach (string branchName in branchList)
                    {
                        // קבלת ה-ID של הסניף לפי שמו
                        int branchId;
                        string getBranchIdQuery = "SELECT BranchID FROM Branches WHERE Name = @BranchName";
                        using (SqlCommand command = new SqlCommand(getBranchIdQuery, connection))
                        {
                            command.Parameters.AddWithValue("@BranchName", branchName);
                            object result = command.ExecuteScalar();

                            if (result != null) // נמצא סניף עם השם הזה
                            {
                                branchId = (int)result;

                                // קישור העובד לסניף
                                string insertEmployeeBranchQuery = "INSERT INTO EmployeeBranches (EmployeeID, BranchID) VALUES (@EmployeeID, @BranchID)";
                                using (SqlCommand insertCommand = new SqlCommand(insertEmployeeBranchQuery, connection))
                                {
                                    insertCommand.Parameters.AddWithValue("@EmployeeID", newEmployeeId);
                                    insertCommand.Parameters.AddWithValue("@BranchID", branchId);
                                    insertCommand.ExecuteNonQuery();
                                }
                            }
                        }
                    }

                    // הוספה לרשימת העובדים בזיכרון
                    Employee newEmployee = new Employee(
                        newEmployeeId,
                        name,
                        roles,  // העברת רשימת התפקידים
                        null,
                        Convert.ToInt32(rate),
                        Convert.ToInt32(salary),
                        isExperienced,
                        branchList
                    );
                    Program.Employees.Add(newEmployee);

                    MessageBox.Show($"העובד {name} נוסף בהצלחה!", "הצלחה", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"אירעה שגיאה בהוספת העובד: {ex.Message}", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}