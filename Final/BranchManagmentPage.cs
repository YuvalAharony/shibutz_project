//using System;
//using System.Windows.Forms;

//namespace EmployeeSchedulingApp
//{
//    public partial class BranchManagementPage : Form
//    {
//        private string branchName;

//        public BranchManagementPage(string branchName)
//        {
//            this.branchName = branchName;
//            InitializeComponent();
//            SetupUI();
//        }

//        private void SetupUI()
//        {
//            this.Text = $"ניהול סניף - {branchName}";
//            this.Size = new System.Drawing.Size(500, 500);

//            Label titleLabel = new Label()
//            {
//                Text = $"ניהול סניף: {branchName}",
//                AutoSize = true,
//                Font = new System.Drawing.Font("Arial", 14, System.Drawing.FontStyle.Bold),
//                Location = new System.Drawing.Point(150, 20)
//            };

//            Button editShiftsButton = new Button()
//            {
//                Text = "ניהול משמרות",
//                Size = new System.Drawing.Size(200, 50),
//                Location = new System.Drawing.Point(150, 80)
//            };
//            // editShiftsButton.Click += (sender, e) => { OpenShiftManagement(); };

//            Button runAlgorithmButton = new Button()
//            {
//                Text = "הרץ אלגוריתם סידור עבודה",
//                Size = new System.Drawing.Size(200, 50),
//                Location = new System.Drawing.Point(150, 150)
//            };
//            runAlgorithmButton.Click += (sender, e) => { RunSchedulingAlgorithm(); };

//            Button backButton = new Button()
//            {
//                Text = "חזור",
//                Size = new System.Drawing.Size(100, 40),
//                Location = new System.Drawing.Point(200, 220)
//            };
//            backButton.Click += (sender, e) => { this.Close(); };

//            this.Controls.Add(titleLabel);
//            this.Controls.Add(editShiftsButton);
//            this.Controls.Add(runAlgorithmButton);
//            this.Controls.Add(backButton);
//        }

//        /*  private void OpenShiftManagement()
//          {
//              ShiftManagementPage shiftManagement = new ShiftManagementPage(branchName);
//              shiftManagement.Show();
//          }*/

//        private void RunSchedulingAlgorithm()
//        {
//            MessageBox.Show("האלגוריתם הופעל בהצלחה!", "הצלחה", MessageBoxButtons.OK, MessageBoxIcon.Information);
//            // כאן ניתן להוסיף לוגיקה אמיתית להפעלת האלגוריתם
//        }
//    }
//}