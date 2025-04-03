using Final;
using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;

namespace EmployeeSchedulingApp
{
    public partial class MainPage : Form
    {
        private List<Employee> EmployeesList;
        private ListView employeesListView;
        private List<Branch> BranchesList;
        private ListView branchesListView;


        public MainPage()
        {
            EmployeesList = Program.Employees;
            BranchesList = Program.Branches;
            SetupUI();
            LoadEmployees();
            LoadBranches();
        }

        private void SetupUI()
        {
            this.Text = "מסך ראשי - ניהול הרשת";
            this.Size = new System.Drawing.Size(800, 600);
            Button generateShiftsButton = new Button()
            {
                Text = "צור משמרות",
                Size = new System.Drawing.Size(150, 40),
                Location = new System.Drawing.Point(300, 500)
            };
            generateShiftsButton.Click += (sender, e) => { Program.createSceduele(); };

            this.Controls.Add(generateShiftsButton);

            Label titleLabel = new Label()
            {
                Text = "ניהול רשת המסעדות",
                AutoSize = true,
                Font = new System.Drawing.Font("Arial", 16, System.Drawing.FontStyle.Bold),
                Location = new System.Drawing.Point(300, 20)
            };

            Button addBranchButton = new Button()
            {
                Text = "הוסף סניף",
                Size = new System.Drawing.Size(150, 40),
                Location = new System.Drawing.Point(50, 80)
            };
            addBranchButton.Click += (sender, e) => { OpenAddBranchPage(); };

            Button addEmployeeButton = new Button()
            {
                Text = "הוסף עובד",
                Size = new System.Drawing.Size(150, 40),
                Location = new System.Drawing.Point(220, 80)
            };
            addEmployeeButton.Click += (sender, e) => { OpenAddEmployeePage(); };

            branchesListView = new ListView()
            {
                Location = new System.Drawing.Point(50, 150),
                Size = new System.Drawing.Size(300, 300),
                View = View.Details,
                FullRowSelect = true,
                GridLines = true
            };
            branchesListView.Columns.Add("שם הסניף", 150);


            employeesListView = new ListView()
            {
                Location = new System.Drawing.Point(400, 150),
                Size = new System.Drawing.Size(300, 300),
                View = View.Details,
                FullRowSelect = true,
                GridLines = true
            };

            employeesListView.Columns.Add("שם העובד", 150);
            employeesListView.Columns.Add("תפקיד", 150);



            this.Controls.Add(titleLabel);
            this.Controls.Add(addBranchButton);
            this.Controls.Add(addEmployeeButton);
            this.Controls.Add(branchesListView);
            this.Controls.Add(employeesListView);
            employeesListView.MouseDoubleClick += EmployeesListView_MouseDoubleClick;

        }
        private void EmployeesListView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (employeesListView.SelectedItems.Count > 0)
            {
                string selectedName = employeesListView.SelectedItems[0].Text;
                Employee selectedEmployee = Program.Employees.FirstOrDefault(emp => emp.Name == selectedName);

                if (selectedEmployee != null)
                {
                    EditEmployeePage editPage = new EditEmployeePage(selectedEmployee);
                    editPage.FormClosed += (s, args) => LoadEmployees(); // רענון הרשימה לאחר סגירה
                    editPage.Show();
                }
            }
        }


        public void LoadEmployees()
        {
            employeesListView.Items.Clear(); // מנקה את הרשימה

            foreach (Employee emp in Program.Employees)
            {
                ListViewItem item = new ListViewItem(emp.Name);
                item.SubItems.Add(emp.Roles.FirstOrDefault() ?? "לא מוגדר"); // מציג את התפקיד הראשון
                employeesListView.Items.Add(item);
            }
        }


        private void LoadBranches()
        {
            branchesListView.Items.Clear();

            foreach (Branch br in Program.Branches)
            {
                ListViewItem item = new ListViewItem(br.Name);
                item.Tag = br;
                branchesListView.Items.Add(item);
            }

            branchesListView.MouseDoubleClick -= OpenViewShiftsPage;
            branchesListView.MouseDoubleClick += OpenViewShiftsPage;
        }
        private void OpenViewShiftsPage(object sender, EventArgs e)
        {

            Branch selectedBranch = (Branch)branchesListView.SelectedItems[0].Tag;
            ViewShiftsPage viewShiftsPage = new ViewShiftsPage(selectedBranch);
            viewShiftsPage.Show();

        }



        private void OpenAddBranchPage()
        {
            AddBranchPage addBranch = new AddBranchPage();
            addBranch.FormClosed += (sender, e) => { LoadBranches(); };

            addBranch.Show();
        }

        private void OpenAddEmployeePage()
        {
            AddEmployeePage addEmployee = new AddEmployeePage();
            addEmployee.FormClosed += (sender, e) => { LoadEmployees(); };
            addEmployee.Show();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // MainPage
            // 
            this.ClientSize = new System.Drawing.Size(282, 253);
            this.Name = "MainPage";
            this.ResumeLayout(false);

        }

     
    }
}