using System;
using System.Linq;
using System.Windows.Forms;

namespace Final
{
    // דף לצפייה בסידור המשמרות של סניף
    public partial class ViewShiftsPage : Form
    {
        // הסניף הנבחר לצפייה
        private Branch selectedBranch;
        // טבלה להצגת המשמרות
        private DataGridView shiftsDataGridView;
        // מערך של ימות השבוע
        private readonly string[] daysOfWeek = new string[] { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };

        // בנאי המחלקה - יוצר טופס לצפייה בסידור המשמרות של סניף
        // פרמטרים
        // branch - הסניף לצפייה
        // ערך מוחזר: אין
        public ViewShiftsPage(Branch branch)
        {
            selectedBranch = branch;
            InitializeComponent();
            SetupUI();
            shiftsDataGridView.CellDoubleClick += ShiftsGridView_CellDoubleClick;
            LoadShifts();
        }

        // הגדרת ממשק המשתמש של הטופס
        // פרמטרים: אין
        // ערך מוחזר: אין
        private void SetupUI()
        {
            this.Text = $"סידור משמרות - {selectedBranch.Name}";
            this.Size = new System.Drawing.Size(800, 600);


            Label titleLabel = new Label()
            {
                Text = $"📌 סידור משמרות עבור {selectedBranch.Name}",
                AutoSize = true,
                Font = new System.Drawing.Font("Arial", 14, System.Drawing.FontStyle.Bold),
                Location = new System.Drawing.Point(150, 20)
            };

            shiftsDataGridView = new DataGridView()
            {
                Location = new System.Drawing.Point(20, 60),
                Size = new System.Drawing.Size(750, 500),
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
            };
            shiftsDataGridView.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            // הגדרת מעבר שורה אוטומטי בתוך התאים

            shiftsDataGridView.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            // התאמת גובה השורות לגובה הטקסט שבהן

            // הגדרת העמודות: העמודה הראשונה – סוג המשמרת, ולאחריה עמודה לכל יום בשבוע
            shiftsDataGridView.ColumnCount = 8;
            shiftsDataGridView.Columns[0].Name = "משמרת";
            for (int i = 0; i < daysOfWeek.Length; i++)
            {
                shiftsDataGridView.Columns[i + 1].Name = daysOfWeek[i];
            }

            this.Controls.Add(titleLabel);
            this.Controls.Add(shiftsDataGridView);
        }

        // טעינת המשמרות לטופס
        // פרמטרים: אין
        // ערך מוחזר: אין
        private void LoadShifts()
        {
            shiftsDataGridView.Rows.Clear();

            // קריאה ישירה לפונקציה הסטטית
            Chromosome bestChromosome = Program.GetBestChromosome();

            if (bestChromosome == null)
            {
                MessageBox.Show("לא נמצא סידור משמרות.", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (bestChromosome.Shifts.ContainsKey(selectedBranch.Name))
            {
                var shifts = bestChromosome.Shifts[selectedBranch.Name];

                // קיבוץ לפי סוג המשמרת
                var groupedShifts = shifts.GroupBy(s => s.TimeSlot)
                                          .OrderBy(g => g.Key);

                foreach (var group in groupedShifts)
                {
                    // מערך: תא ראשון = סוג המשמרת, תאים 1..n = ימים
                    string[] row = new string[daysOfWeek.Length + 1];
                    row[0] = group.Key;

                    foreach (var shift in group)
                    {
                        int dayIndex = Array.IndexOf(daysOfWeek, shift.day);
                        if (dayIndex >= 0)
                        {
                            string employees = string.Join(Environment.NewLine,
                                shift.AssignedEmployees.SelectMany(role => role.Value)
                                    .Select(emp => emp?.Name ?? "לא ידוע")
                                    .Distinct());

                            if (string.IsNullOrEmpty(row[dayIndex + 1]))
                                row[dayIndex + 1] = employees;
                            else if (!string.IsNullOrEmpty(employees))
                                row[dayIndex + 1] += "; " + employees;
                        }
                    }

                    shiftsDataGridView.Rows.Add(row);
                }
            }
            else
            {
                MessageBox.Show("אין משמרות עבור סניף זה.", "הודעה", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        // אירוע לחיצה כפולה על תא בטבלת המשמרות - פותח דף פרטי משמרת
        // פרמטרים
        // sender - האובייקט שהפעיל את האירוע
        // e - נתוני האירוע
        // ערך מוחזר: אין
        private void ShiftsGridView_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            // דילוג אם נלחץ כותרת עמודה/שורה או העמודה הראשונה
            if (e.RowIndex < 0 || e.ColumnIndex <= 0)
                return;

            // קבלת היום מכותרת העמודה
            string dayOfWeek = shiftsDataGridView.Columns[e.ColumnIndex].Name;

            // קבלת סוג המשמרת מתא בעמודה הראשונה
            string shiftTime = shiftsDataGridView.Rows[e.RowIndex].Cells[0].Value?.ToString();

            if (string.IsNullOrEmpty(shiftTime))
                return;

            // שליפת הכרומוזום הטוב ביותר
            Chromosome bestChromosome = Program.GetBestChromosome();
            if (bestChromosome == null || !bestChromosome.Shifts.ContainsKey(selectedBranch.Name))
            {
                MessageBox.Show("לא נמצא סידור משמרות לסניף זה", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // חיפוש המשמרת המתאימה
            Shift selectedShift = bestChromosome.Shifts[selectedBranch.Name]
                .FirstOrDefault(s => s.day == dayOfWeek && s.TimeSlot == shiftTime);

            if (selectedShift != null)
            {
                // יצירת והצגת תצוגת פרטי משמרת
                ViewShiftDetailPage detailPage = new ViewShiftDetailPage(selectedShift, selectedBranch);
                detailPage.Show();
            }
            else
            {
                MessageBox.Show($"לא נמצאו פרטי משמרת ליום {dayOfWeek}, משמרת {shiftTime}",
                    "הודעה", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }




       
    }
}