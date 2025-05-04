using EmployeeSchedulingApp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Final
{
    // דף לצפייה בפרטי משמרת ועובדים משובצים
    public class ViewShiftDetailPage : Form
    {
        // המשמרת להצגה
        private Shift shift;
        // הסניף אליו שייכת המשמרת
        private Branch branch;
        // טבלה להצגת התפקידים והעובדים במשמרת
        private DataGridView rolesGridView;

        // בנאי המחלקה - יוצר טופס לצפייה בפרטי משמרת
        // פרמטרים
        // shift - המשמרת להצגה
        // branch - הסניף אליו שייכת המשמרת
        // ערך מוחזר: אין
        public ViewShiftDetailPage(Shift shift, Branch branch)
        {
            this.shift = shift;
            this.branch = branch;
            SetupUI();
            LoadRolesAndEmployees();
        }

        // פונקציה להגדרת ממשק המשתמש
        // פרמטרים: אין
        // ערך מוחזר: אין
        private void SetupUI()
        {
            // הגדרת כותרת החלון
            this.Text = $"פרטי משמרת - {branch.Name} - {shift.day} {shift.TimeSlot}";
            // קביעת גודל החלון
            this.Size = new System.Drawing.Size(600, 400);
            // הגדרת כיוון טקסט מימין לשמאל
            this.RightToLeft = RightToLeft.Yes;

            // יצירת כותרת ראשית
            Label titleLabel = new Label()
            {
                Text = $"עובדים ותפקידים במשמרת: {branch.Name} - {shift.day} {shift.TimeSlot}",
                AutoSize = true,
                Font = new System.Drawing.Font("Arial", 14, System.Drawing.FontStyle.Bold),
                Location = new System.Drawing.Point(20, 20)
            };
            // הוספת התווית לחלון
            this.Controls.Add(titleLabel);

            // יצירת טבלת תפקידים
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

            // הוספת עמודות לטבלה
            rolesGridView.Columns.Add("Role", "תפקיד");
            rolesGridView.Columns.Add("Required", "נדרש");
            rolesGridView.Columns.Add("Employee", "עובד משובץ");
            rolesGridView.Columns.Add("Rate", "דירוג");
            rolesGridView.Columns.Add("IsMentor", "חונך");

            // הוספת הטבלה לחלון
            this.Controls.Add(rolesGridView);
        }

        // טעינת התפקידים והעובדים המשובצים במשמרת לטבלה
        // פרמטרים: אין
        // ערך מוחזר: אין
        private void LoadRolesAndEmployees()
        {
            rolesGridView.Rows.Clear();

            // מציאת הכרומוזום הטוב ביותר
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


      
    }
}