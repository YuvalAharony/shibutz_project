using EmployeeSchedulingApp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Final
{
    public class ViewShiftDetailPage : Form
    {
        private Shift shift;
        private Branch branch;
        private DataGridView rolesGridView;

        public ViewShiftDetailPage(Shift shift, Branch branch)
        {
            this.shift = shift;
            this.branch = branch;
            InitializeUI();
            // Add this line to the constructor or InitializeComponent/SetupUI method
            LoadRolesAndEmployees();
        }

        private void InitializeUI()
        {
            // Set basic form properties
            this.Text = $"פרטי משמרת - {branch.Name} - {shift.day} {shift.TimeSlot}";
            this.Size = new System.Drawing.Size(600, 400);
            this.RightToLeft = RightToLeft.Yes; // For Hebrew support

            // Create title label
            Label titleLabel = new Label()
            {
                Text = $"עובדים ותפקידים במשמרת: {branch.Name} - {shift.day} {shift.TimeSlot}",
                AutoSize = true,
                Font = new System.Drawing.Font("Arial", 14, System.Drawing.FontStyle.Bold),
                Location = new System.Drawing.Point(20, 20)
            };
            this.Controls.Add(titleLabel);

            // Create grid for roles and assigned employees
            rolesGridView = new DataGridView()
            {
                Location = new System.Drawing.Point(20, 60),
                Size = new System.Drawing.Size(540, 280),
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.Fixed3D,
                RightToLeft = RightToLeft.Yes
            };

            // Add columns to the grid
            rolesGridView.Columns.Add("Role", "תפקיד");
            rolesGridView.Columns.Add("Required", "נדרש");
            rolesGridView.Columns.Add("Employee", "עובד משובץ");
            rolesGridView.Columns.Add("Rate", "דירוג");
            rolesGridView.Columns.Add("IsMentor", "חונך");

            this.Controls.Add(rolesGridView);
        }

        private void LoadRolesAndEmployees()
        {
            rolesGridView.Rows.Clear();

            // נסה למצוא את המשמרת ב-Chromosome הטוב ביותר
            Shift bestShift = null;
            Chromosome bestChromosome = Program.GetBestChromosome();

            if (bestChromosome != null)
            {
                foreach (var branchEntry in bestChromosome.Shifts)
                {
                    foreach (var s in branchEntry.Value)
                    {
                        if (s.Id == shift.Id)
                        {
                            bestShift = s;
                            break;
                        }
                    }
                    if (bestShift != null) break;
                }
            }

            // אם מצאנו את המשמרת בכרומוזום הטוב ביותר, השתמש בה במקום במשמרת המקורית
            if (bestShift != null && bestShift.AssignedEmployees != null && bestShift.AssignedEmployees.Count > 0)
            {
                // הקוד המקורי שלך להצגת עובדים, אבל משתמש ב-bestShift במקום shift
                foreach (var roleEntry in bestShift.RequiredRoles)
                {
                    string roleName = roleEntry.Key;
                    int requiredCount = roleEntry.Value;

                    List<Employee> assignedEmployees = new List<Employee>();
                    if (bestShift.AssignedEmployees.ContainsKey(roleName))
                    {
                        assignedEmployees = bestShift.AssignedEmployees[roleName];
                    }

                    // הוסף שורה עבור כל עובד משובץ
                    for (int i = 0; i < Math.Min(requiredCount, assignedEmployees.Count); i++)
                    {
                        Employee emp = assignedEmployees[i];
                        rolesGridView.Rows.Add(
                            roleName,
                            requiredCount,
                            emp.Name,
                            emp.Rate.ToString(),
                            emp.isMentor ? "כן" : "לא"
                        );
                    }

                    // הוסף שורות "NULL" עבור עמדות לא מאוישות
                    for (int i = assignedEmployees.Count; i < requiredCount; i++)
                    {
                        rolesGridView.Rows.Add(
                            roleName,
                            requiredCount,
                            "NULL",
                            "-",
                            "-"
                        );
                    }
                }

                return;
            }

            // אם לא מצאנו את המשמרת בכרומוזום הטוב ביותר, המשך עם הקוד המקורי
            // (כל הקוד המקורי שלך להצגת משמרות ריקות)
            foreach (var roleEntry in shift.RequiredRoles)
            {
                string roleName = roleEntry.Key;
                int requiredCount = roleEntry.Value;

                for (int i = 0; i < requiredCount; i++)
                {
                    rolesGridView.Rows.Add(
                        roleName,
                        requiredCount,
                        "NULL",
                        "-",
                        "-"
                    );
                }
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // ViewShiftDetailPage
            // 
            this.ClientSize = new System.Drawing.Size(282, 253);
            this.Name = "ViewShiftDetailPage";
            this.ResumeLayout(false);

        }

        
    }
}