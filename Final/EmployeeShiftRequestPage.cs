using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Final
{
    public class EmployeeShiftRequestPage : Form
    {
        private static string connectionString = "Data Source=(localdb)\\mssqllocaldb;Initial Catalog=EmployeeScheduling;Integrated Security=True";

        private Employee currentEmployee;
        private DataGridView shiftsDataGridView;
        private ComboBox branchComboBox;
        private Label welcomeLabel;
        private Label summaryLabel;
        private Button saveButton;
        private Dictionary<int, Shift> shiftsMap = new Dictionary<int, Shift>();
        private List<string> employeeBranches = new List<string>();

        public EmployeeShiftRequestPage(Employee employee)
        {
            currentEmployee = employee;
            InitializeComponent();
            LoadEmployeeData();
        }

        private void InitializeComponent()
        {
            this.Text = "הגשת משמרות מועדפות";
            this.Size = new System.Drawing.Size(800, 600);
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;

            // כותרת ברוכים הבאים
            welcomeLabel = new Label
            {
                Text = $"שלום {currentEmployee.Name}",
                Font = new System.Drawing.Font("Arial", 14, System.Drawing.FontStyle.Bold),
                AutoSize = true,
                Location = new System.Drawing.Point(300, 20)
            };

            // תיאור הטופס
            Label descriptionLabel = new Label
            {
                Text = "בחר את המשמרות המועדפות עליך",
                AutoSize = true,
                Location = new System.Drawing.Point(300, 50)
            };

          

            // בחירת סניף
            Label branchLabel = new Label
            {
                Text = "בחר סניף:",
                AutoSize = true,
                Location = new System.Drawing.Point(50, 90)
            };

            branchComboBox = new ComboBox
            {
                Location = new System.Drawing.Point(150, 90),
                Width = 200,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            branchComboBox.SelectedIndexChanged += BranchComboBox_SelectedIndexChanged;

            // טבלת משמרות
            shiftsDataGridView = new DataGridView
            {
                Location = new System.Drawing.Point(50, 130),
                Size = new System.Drawing.Size(700, 350),
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = true,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            shiftsDataGridView.Columns.Add("ShiftID", "מזהה");
            shiftsDataGridView.Columns.Add("Day", "יום");
            shiftsDataGridView.Columns.Add("TimeSlot", "שעות");
            shiftsDataGridView.Columns.Add("Type", "סוג משמרת");
            shiftsDataGridView.Columns.Add("Selected", "נבחר");

            // Hide the ShiftID column
            shiftsDataGridView.Columns["ShiftID"].Visible = false;

            shiftsDataGridView.CellClick += ShiftsDataGridView_CellClick;

            // כפתור שמירה
            saveButton = new Button
            {
                Text = "שמור משמרות מועדפות",
                Size = new System.Drawing.Size(150, 40),
                Location = new System.Drawing.Point(600, 500)
            };
            saveButton.Click += SaveButton_Click;

            // כפתור סגירה
            Button closeButton = new Button
            {
                Text = "סגור",
                Size = new System.Drawing.Size(150, 40),
                Location = new System.Drawing.Point(440, 500)
            };
            closeButton.Click += (sender, e) => this.Close();

            // הוספת הרכיבים לטופס
            this.Controls.Add(welcomeLabel);
            this.Controls.Add(descriptionLabel);
            this.Controls.Add(branchLabel);
            this.Controls.Add(branchComboBox);
            this.Controls.Add(summaryLabel);
            this.Controls.Add(shiftsDataGridView);
            this.Controls.Add(saveButton);
            this.Controls.Add(closeButton);
        }

        private void LoadEmployeeData()
        {
            // טעינת הסניפים של העובד
            LoadEmployeeBranches();

            // טעינת ההעדפות הקיימות של העובד
            LoadEmployeePreferences();

            // טעינת הנתונים הנוספים של העובד, אם צריך
            UpdateEmployeeInfo();

        
        }

   

        private void LoadEmployeeBranches()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = @"
                        SELECT b.BranchID, b.Name
                        FROM Branches b
                        INNER JOIN EmployeeBranches eb ON b.BranchID = eb.BranchID
                        WHERE eb.EmployeeID = @EmployeeID
                        ORDER BY b.Name";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@EmployeeID", currentEmployee.ID);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            branchComboBox.Items.Clear();
                            employeeBranches.Clear();

                            while (reader.Read())
                            {
                                string branchName = reader.GetString(1);
                                employeeBranches.Add(branchName);
                                branchComboBox.Items.Add(branchName);
                            }

                            if (branchComboBox.Items.Count > 0)
                            {
                                branchComboBox.SelectedIndex = 0;
                            }
                            else
                            {
                                MessageBox.Show("לא נמצאו סניפים משויכים לעובד זה", "הודעה",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"שגיאה בטעינת סניפים: {ex.Message}", "שגיאה",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadEmployeePreferences()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = @"
                        SELECT eps.ShiftID, b.Name AS BranchName
                        FROM EmployeePreferredShifts eps
                        JOIN Shifts s ON eps.ShiftID = s.ShiftID
                        JOIN Branches b ON s.BranchID = b.BranchID
                        WHERE eps.EmployeeID = @EmployeeID";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@EmployeeID", currentEmployee.ID);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            currentEmployee.requestedShifts = new System.Collections.Generic.HashSet<int>();

                            // מילון לשמירת מספר המשמרות לפי סניף
                            Dictionary<string, int> branchShiftCounts = new Dictionary<string, int>();

                            while (reader.Read())
                            {
                                int shiftId = reader.GetInt32(0);
                                string branchName = reader.GetString(1);

                                currentEmployee.requestedShifts.Add(shiftId);

                                // הוספה לספירת המשמרות לפי סניף
                                if (branchShiftCounts.ContainsKey(branchName))
                                    branchShiftCounts[branchName]++;
                                else
                                    branchShiftCounts[branchName] = 1;
                            }

                            // הוספת תיאור לכל סניף עם מספר המשמרות
                            for (int i = 0; i < branchComboBox.Items.Count; i++)
                            {
                                string branchName = branchComboBox.Items[i].ToString();
                                if (branchShiftCounts.ContainsKey(branchName))
                                {
                                    // לא משנים את התוכן המקורי של הרשימה, אבל מציגים את המידע בצורת טקסט
                                    branchComboBox.Items[i] = $"{branchName} ({branchShiftCounts[branchName]} משמרות)";
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"שגיאה בטעינת העדפות משמרות: {ex.Message}", "שגיאה",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateEmployeeInfo()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = @"
                        SELECT r.RoleName 
                        FROM EmployeeRoles er
                        JOIN Roles r ON er.RoleID = r.RoleID
                        WHERE er.EmployeeID = @EmployeeID";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@EmployeeID", currentEmployee.ID);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            currentEmployee.roles = new HashSet<string>();

                            while (reader.Read())
                            {
                                string role = reader.GetString(0);
                                currentEmployee.roles.Add(role);
                            }
                        }
                    }
                }

                // עדכון התווית של ברוכים הבאים עם מידע נוסף
                string roles = currentEmployee.roles.Count > 0 ?
                    string.Join(", ", currentEmployee.roles) : "לא הוגדר";

                welcomeLabel.Text = $"שלום {currentEmployee.Name} | תפקיד: {roles}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"שגיאה בטעינת מידע נוסף: {ex.Message}", "שגיאה",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BranchComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (branchComboBox.SelectedIndex >= 0)
            {
                string selectedBranchText = branchComboBox.SelectedItem.ToString();

                // הסרת המידע על מספר המשמרות אם קיים
                string selectedBranch = selectedBranchText;
                if (selectedBranchText.Contains(" ("))
                {
                    selectedBranch = selectedBranchText.Substring(0, selectedBranchText.IndexOf(" ("));
                }

                LoadShiftsForBranch(selectedBranch);
            }
        }

        private void LoadShiftsForBranch(string branchName)
        {
            shiftsDataGridView.Rows.Clear();
            shiftsMap.Clear();

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // שיניתי את סדר המיון בשאילתה - קודם לפי יום בשבוע, ואז לפי זמן (בוקר/ערב)
                    string query = @"
                        SELECT s.ShiftID, s.DayOfWeek, ts.TimeSlotName, st.TypeName,
                               CASE WHEN eps.ShiftID IS NOT NULL THEN 1 ELSE 0 END AS IsSelected
                        FROM Shifts s
                        INNER JOIN Branches b ON s.BranchID = b.BranchID
                        INNER JOIN TimeSlots ts ON s.TimeSlotID = ts.TimeSlotID
                        INNER JOIN ShiftTypes st ON s.ShiftTypeID = st.ShiftTypeID
                        LEFT JOIN EmployeePreferredShifts eps ON s.ShiftID = eps.ShiftID AND eps.EmployeeID = @EmployeeID
                        WHERE b.Name = @BranchName
                        ORDER BY 
                            CASE s.DayOfWeek 
                                WHEN 'Sunday' THEN 1
                                WHEN 'Monday' THEN 2
                                WHEN 'Tuesday' THEN 3
                                WHEN 'Wednesday' THEN 4
                                WHEN 'Thursday' THEN 5
                                WHEN 'Friday' THEN 6
                                WHEN 'Saturday' THEN 7
                            END,
                            CASE ts.TimeSlotName
                                WHEN 'Morning' THEN 1
                                WHEN 'Evening' THEN 2
                                ELSE 3
                            END";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@BranchName", branchName);
                        command.Parameters.AddWithValue("@EmployeeID", currentEmployee.ID);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            // רשימה לשמירת כל המשמרות לפני הוספתן לטבלה
                            List<object[]> shifts = new List<object[]>();

                            while (reader.Read())
                            {
                                int shiftId = reader.GetInt32(0);
                                string dayOfWeek = reader.GetString(1);
                                string timeSlot = reader.GetString(2);
                                string shiftType = reader.GetString(3);
                                bool isSelected = reader.GetInt32(4) == 1;

                                // שמירת המשמרת במפה לשימוש מהיר
                                Shift shift = new Shift
                                {
                                    Id = shiftId,
                                    branch = branchName,
                                    day = dayOfWeek,
                                    TimeSlot = timeSlot,
                                    EventType = shiftType
                                };

                                shiftsMap[shiftId] = shift;

                                // הוספה לרשימה
                                shifts.Add(new object[] {
                                    shiftId,
                                    dayOfWeek, // שימוש בשם היום באנגלית
                                    timeSlot,
                                    shiftType,
                                    isSelected ? "✓" : ""
                                });
                            }

                            // הוספת כל המשמרות לטבלה, ממוינות כפי שהוחזרו מהשאילתה
                            foreach (var shiftData in shifts)
                            {
                                shiftsDataGridView.Rows.Add(shiftData);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"שגיאה בטעינת משמרות: {ex.Message}", "שגיאה",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShiftsDataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // דילוג אם נלחץ כותרת עמודה
            if (e.RowIndex < 0)
                return;

            // החלפת הסימון
            DataGridViewRow row = shiftsDataGridView.Rows[e.RowIndex];
            bool isCurrentlySelected = !string.IsNullOrEmpty(row.Cells["Selected"].Value?.ToString());

            // עדכון התא המסומן
            row.Cells["Selected"].Value = isCurrentlySelected ? "" : "✓";
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            // איסוף כל המשמרות המסומנות
            HashSet<int> selectedShiftIds = new HashSet<int>();
            foreach (DataGridViewRow row in shiftsDataGridView.Rows)
            {
                if (!string.IsNullOrEmpty(row.Cells["Selected"].Value?.ToString()))
                {
                    int shiftId = Convert.ToInt32(row.Cells["ShiftID"].Value);
                    selectedShiftIds.Add(shiftId);
                }
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // התחלת טרנזקציה לפעולות אטומיות
                    using (SqlTransaction transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            // הסרת המידע על מספר המשמרות מהשם שנבחר אם קיים
                            string selectedBranchText = branchComboBox.SelectedItem.ToString();
                            string selectedBranch = selectedBranchText;
                            if (selectedBranchText.Contains(" ("))
                            {
                                selectedBranch = selectedBranchText.Substring(0, selectedBranchText.IndexOf(" ("));
                            }

                            // מחיקת ההעדפות הקיימות לסניף הנוכחי
                            string deleteQuery = @"
                                DELETE FROM EmployeePreferredShifts 
                                WHERE EmployeeID = @EmployeeID 
                                AND ShiftID IN (
                                    SELECT s.ShiftID 
                                    FROM Shifts s 
                                    INNER JOIN Branches b ON s.BranchID = b.BranchID 
                                    WHERE b.Name = @BranchName
                                )";

                            using (SqlCommand command = new SqlCommand(deleteQuery, connection, transaction))
                            {
                                command.Parameters.AddWithValue("@EmployeeID", currentEmployee.ID);
                                command.Parameters.AddWithValue("@BranchName", selectedBranch);
                                command.ExecuteNonQuery();
                            }

                            // הוספת ההעדפות החדשות
                            if (selectedShiftIds.Count > 0)
                            {
                                string insertQuery = "INSERT INTO EmployeePreferredShifts (EmployeeID, ShiftID) VALUES (@EmployeeID, @ShiftID)";

                                foreach (int shiftId in selectedShiftIds)
                                {
                                    using (SqlCommand command = new SqlCommand(insertQuery, connection, transaction))
                                    {
                                        command.Parameters.AddWithValue("@EmployeeID", currentEmployee.ID);
                                        command.Parameters.AddWithValue("@ShiftID", shiftId);
                                        command.ExecuteNonQuery();
                                    }
                                }
                            }

                            // אישור הטרנזקציה אם הכל הצליח
                            transaction.Commit();

                            // עדכון העדפות העובד בזיכרון
                            LoadEmployeePreferences();

                            // טעינה מחדש של המשמרות לאחר השמירה
                            LoadShiftsForBranch(selectedBranch);

                            // עדכון התווית של הסניף הנוכחי עם מספר המשמרות החדש
                            int currentIndex = branchComboBox.SelectedIndex;
                            if (currentIndex >= 0)
                            {
                                string updatedBranchName = $"{selectedBranch} ({selectedShiftIds.Count} משמרות)";
                                branchComboBox.Items[currentIndex] = updatedBranchName;
                                branchComboBox.SelectedIndex = currentIndex;
                            }

                            // הודעת הצלחה
                            MessageBox.Show($"נשמרו {selectedShiftIds.Count} העדפות משמרות עבור סניף {selectedBranch}!", "הצלחה",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        catch (Exception ex)
                        {
                            // ביטול הטרנזקציה במקרה של שגיאה
                            transaction.Rollback();
                            throw ex;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"שגיאה בשמירת העדפות המשמרות: {ex.Message}", "שגיאה",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // פונקציה זו אינה נחוצה עוד ולכן נוכל להסירה
        // private string TranslateDayToHebrew(string englishDay)
        // {
        //     switch (englishDay)
        //     {
        //         case "Sunday": return "ראשון";
        //         case "Monday": return "שני";
        //         case "Tuesday": return "שלישי";
        //         case "Wednesday": return "רביעי";
        //         case "Thursday": return "חמישי";
        //         case "Friday": return "שישי";
        //         case "Saturday": return "שבת";
        //         default: return englishDay;
        //     }
        // }
    }
}