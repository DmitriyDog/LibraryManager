using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Npgsql;

namespace Library
{
    public class Employee
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string Surname { get; set; }
        public string Name { get; set; }
        public string Patronymic { get; set; }
        public string Job { get; set; }
    }

    public partial class DirectorFuncs : Window
    {
        internal string connectionString = LoginWindow.connectionString;
        private string selfLogin = "";
        public List<Employee> employees;
        public DirectorFuncs(string name, string login)
        {
            InitializeComponent();
            Application.Current.MainWindow = this;
            HelloBlock.Text = "Добро пожаловать, " + name;
            selfLogin = login;

            DataTable db = new DataTable();
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                var command = new NpgsqlCommand("SELECT employee.id_employee, account.login, employee.surname, employee.name, employee.patronymic_name, jobs.name FROM account," +
                    " employee, jobs WHERE account.id_employee = employee.id_employee AND jobs.id_job = employee.id_job", connection);
                NpgsqlDataReader reader = command.ExecuteReader();
                db.Load(reader);
                connection.Close();
            }
            employees = new List<Employee>();
            foreach (DataRow row in db.Rows)
            {
                Employee worker = new Employee();
                worker.Id = Convert.ToInt32(row.ItemArray[0]);
                worker.Login = row.ItemArray[1].ToString();
                worker.Surname = row.ItemArray[2].ToString();
                worker.Name = row.ItemArray[3].ToString();
                worker.Patronymic = row.ItemArray[4].ToString();
                worker.Job = row.ItemArray[5].ToString();
                employees.Add(worker);
            }
        }

        private void CreateBtn_Click(object sender, RoutedEventArgs e)
        {
            AddAccount window = new AddAccount();
            window.Owner = Application.Current.MainWindow;
            IsEnabled = false;
            window.Show();
        }

        private void ChangeBtn_Click(object sender, RoutedEventArgs e)
        {
            if (InfoTab.SelectedIndex != -1)
            {
                bool selfAdmin = false;
                int index = InfoTab.SelectedIndex;
                Employee forFunc = (Employee)InfoTab.Items[index];
                if (forFunc.Login == selfLogin)
                {
                    selfAdmin = true;
                }
                ChangeProfile window = new ChangeProfile(forFunc, selfAdmin);
                window.Owner = this;
                window.ShowDialog();
                if (window.DialogResult == true)
                {
                    InfoTab.Items.Refresh();
                }
            }
        }

        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            if (InfoTab.SelectedIndex != -1 && ((Employee)InfoTab.SelectedItem).Login != selfLogin)
            {
                int index = InfoTab.SelectedIndex;
                Employee forFunc = (Employee)InfoTab.Items[index];
                employees.Remove(forFunc);
                InfoTab.Items.Refresh();
                InfoTab.SelectedIndex = -1;
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    var command = new NpgsqlCommand("DELETE FROM account WHERE id_employee = " + forFunc.Id, connection);
                    command.ExecuteNonQuery();
                }
            }
        }

        private void ExitBtn_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow newLogin = new LoginWindow();
            newLogin.Show();
            Close();
        }

        private void PasswordBtn_Click(object sender, RoutedEventArgs e)
        {
            if (InfoTab.SelectedIndex != -1)
            {
                var select = (Employee)InfoTab.SelectedItem;
                ChangePassword changePaswrd = new ChangePassword(select.Login);
                changePaswrd.ShowDialog();
            }
        }

        private void InfoTab_Loaded(object sender, RoutedEventArgs e)
        {
            InfoTab.ItemsSource = employees;
            InfoTab.Columns[0].Visibility = Visibility.Collapsed;
            InfoTab.Columns[1].Header = "Логин";
            InfoTab.Columns[2].Header = "Фамилия";
            InfoTab.Columns[3].Header = "Имя";
            InfoTab.Columns[4].Header = "Отчество";
            InfoTab.Columns[5].Header = "Права";
            foreach (var column in InfoTab.Columns)
            {
                column.MinWidth = column.ActualWidth;
                column.Width = new DataGridLength(1, DataGridLengthUnitType.Star);
            }
        }
    }
}
