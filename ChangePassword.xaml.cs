using Npgsql;
using System;
using System.Collections.Generic;
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

namespace Library
{
    public partial class ChangePassword : Window
    {
        private string loginUser;
        private string connectionString = LoginWindow.connectionString;
        public ChangePassword(string login)
        {
            InitializeComponent();
            loginUser = login;
        }

        private void AcceptBtn_Click(object sender, RoutedEventArgs e)
        {
            if (PasswordStr.Password.Length < 6 || PasswordRepeatStr.Password.Length < 6)
            {
                ErrorMessage.Visibility = Visibility.Visible;
                ErrorMessage2.Visibility = Visibility.Hidden;
            }
            else if (PasswordStr.Password != PasswordRepeatStr.Password)
            {
                ErrorMessage.Visibility = Visibility.Hidden;
                ErrorMessage2.Visibility = Visibility.Visible;
            }
            else
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    var command = new NpgsqlCommand("UPDATE account SET password = '" + LoginWindow.HashPassword(PasswordStr.Password)
                        + "' WHERE login = '" + loginUser + "'", connection);
                    command.ExecuteNonQuery();
                }
                Close();
            }
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
