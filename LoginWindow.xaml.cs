using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security;
using System.Security.Cryptography;
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
    public partial class LoginWindow : Window
    {
        public static string connectionString = "Host=localhost;Username=postgres;Password=password;Database=Library2";
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void LoginBtn_Click(object sender, RoutedEventArgs e)
        {
            if (RevealPswrd.IsChecked == false)
            {
                RealPassword.Text = PasswordStr.Password;
            }
            NpgsqlDataReader reader;
            string login = LoginStr.Text;
            DataTable db = new DataTable();
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                var command = new NpgsqlCommand("SELECT employee.id_job, employee.name FROM account," +
                    " employee WHERE employee.id_employee = account.id_employee AND account.password = '"
                    + HashPassword(RealPassword.Text) + "' AND account.login = '" + login + "'", connection);
                reader = command.ExecuteReader();
                db.Load(reader);
            }
            if (db.Rows.Count == 0)
            {
                IncorrectLogin.Visibility = Visibility.Visible;
            }
            else
            {
                if ((int)db.Rows[0].ItemArray[0] == 2)
                {
                    LibrarianProfile openProfile = new LibrarianProfile(login, (string)db.Rows[0].ItemArray[1]);
                    openProfile.Show();
                    Close();
                }
                else
                {
                    DirectorFuncs openAdmin = new DirectorFuncs((string)db.Rows[0].ItemArray[1], login);
                    openAdmin.Show();
                    Close();
                }
            }
        }

        private void RevealPswrd_Checked(object sender, RoutedEventArgs e)
        {
            if (RevealPswrd.IsChecked == true)
            {
                RealPassword.Text = PasswordStr.Password;
                PasswordStr.Visibility = Visibility.Hidden;
                RealPassword.Visibility = Visibility.Visible;
            }
            else
            {
                PasswordStr.Password = RealPassword.Text;
                PasswordStr.Visibility = Visibility.Visible;
                RealPassword.Visibility = Visibility.Hidden;
            }
        }

        public static string HashPassword(string password)
        {
            MD5 md5 = MD5.Create();

            byte[] b = Encoding.ASCII.GetBytes(password);
            byte[] hash = md5.ComputeHash(b);

            StringBuilder sb = new StringBuilder();
            foreach (var a in hash)
            {
                sb.Append(a.ToString("X2"));
            }

            return Convert.ToString(sb);
        }
    }
}
