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
using System.Xml.Linq;

namespace Library
{
    public partial class AddAccount : Window
    {
        DirectorFuncs mainWindow;
        string phone = "";
        public AddAccount()
        {
            InitializeComponent();
            mainWindow = Application.Current.MainWindow as DirectorFuncs;
        }

        private void AcceptBtn_Click(object sender, RoutedEventArgs e)
        {
            if (RevealPswrd.IsChecked == false)
            {
                RealPassword.Text = PasswordStr.Password;
            }
            if (RealPassword.Text.Length < 6)
            {
                MessageBox.Text = "Пароль должен содержать хотя бы 6 символов";
            }
            else if (SurnameStr.Text.Trim() != "" && NameStr.Text.Trim() != "" && LoginStr.Text.Trim() != "" && RealPassword.Text != "")
            {
                NpgsqlDataReader reader;
                string log = LoginStr.Text.Trim();
                using (var connection = new NpgsqlConnection(mainWindow.connectionString))
                {
                    connection.Open();
                    var command = new NpgsqlCommand("SELECT COUNT(*) FROM account WHERE login = '" + log + "'", connection);
                    Int64 count = (Int64)command.ExecuteScalar();
                    if (count == 0)
                    {
                        using var insertCommand = new NpgsqlCommand("INSERT INTO employee (surname, name, patronymic_name, phone_number, id_job) VALUES (($1), ($2), ($3), ($4), 2) RETURNING id_employee", connection)
                        {
                            Parameters =
                            {
                                new() { Value= SurnameStr.Text.Trim()},
                                new() { Value= NameStr.Text.Trim()},
                                new() { Value= PatronymicStr.Text.Trim()},
                                new() { Value= NumStr.Text}
                            }
                        };
                        int id = (int)insertCommand.ExecuteScalar();
                        using var insertCommand2 = new NpgsqlCommand("INSERT INTO account (login, password, id_employee) VALUES (($1), ($2), ($3))", connection)
                        {
                            Parameters =
                            {
                                new() { Value= log},
                                new() { Value= LoginWindow.HashPassword(RealPassword.Text)},
                                new() { Value= id}
                            }
                        };
                        insertCommand2.ExecuteNonQuery();
                        Employee toAdd = new Employee();
                        toAdd.Id = id;
                        toAdd.Surname = SurnameStr.Text.Trim();
                        toAdd.Name = NameStr.Text.Trim();
                        toAdd.Patronymic = PatronymicStr.Text.Trim();
                        toAdd.Login = LoginStr.Text.Trim();
                        toAdd.Job = "библиотекарь";
                        mainWindow.employees.Add(toAdd);
                        mainWindow.InfoTab.Items.Refresh();
                        Close();
                    }
                    else
                    {
                        MessageBox.Text = "Логин не является уникальным";
                    }
                }
            }
            else
            {
                MessageBox.Text = "Поля фамилии, имени, логина и пароля являются обязательными для заполнения";
            }
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void NumStr_TextChanged(object sender, TextChangedEventArgs e)
        {
            var numbox = sender as TextBox;
            UInt64 testNumber;
            if (numbox.Text != "" && !UInt64.TryParse(numbox.Text, out testNumber))
            {
                numbox.Text = phone;
                numbox.Select(numbox.Text.Length, 0);
            }
            else
            {
                phone = numbox.Text;
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

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            mainWindow.IsEnabled = true;
        }
    }
}
