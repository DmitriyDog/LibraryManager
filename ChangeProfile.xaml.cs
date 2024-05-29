using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
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
    public partial class ChangeProfile : Window
    {
        DirectorFuncs mainWindow;
        int id;
        Employee changingWorker;
        string phone;
        public ChangeProfile(Employee changingWorker, bool selfAdmin)
        {
            InitializeComponent();
            mainWindow = Application.Current.MainWindow as DirectorFuncs;
            this.changingWorker = changingWorker;

            SurnameStr.Text = changingWorker.Surname;
            NameStr.Text = changingWorker.Name;
            PatronymicStr.Text = changingWorker.Patronymic;
            if (changingWorker.Job == "администратор")
            {
                JobStr.SelectedIndex = 0;
            }
            else
            {
                JobStr.SelectedIndex = 1;
            }
            if (selfAdmin)
            {
                JobStr.IsEnabled = false;
            }

            id = changingWorker.Id;

            using (var connection = new NpgsqlConnection(mainWindow.connectionString))
            {
                connection.Open();
                using var getCommand = new NpgsqlCommand("SELECT employee.phone_number FROM employee WHERE employee.id_employee = ($1)", connection)
                {
                    Parameters =
                    {
                        new() { Value= id}
                    }
                };
                var reader = getCommand.ExecuteReader();

                reader.Read();
                NumStr.Text = reader[0].ToString().Trim();
            }
        }

        private void AcceptBtn_Click(object sender, RoutedEventArgs e)
        {
            if (SurnameStr.Text.Trim() != "" && NameStr.Text.Trim() != "")
            {
                NpgsqlDataReader reader;
                using (var connection = new NpgsqlConnection(mainWindow.connectionString))
                {
                    connection.Open();
                    using var insertCommand = new NpgsqlCommand("UPDATE employee SET surname=($1), name=($2), patronymic_name=($3), phone_number=($4), id_job=($5) WHERE id_employee=($6)", connection)
                    {
                        Parameters =
                        {
                            new() { Value= SurnameStr.Text.Trim()},
                            new() { Value= NameStr.Text.Trim()},
                            new() { Value= PatronymicStr.Text.Trim()},
                            new() { Value= NumStr.Text},
                            new() { Value= JobStr.SelectedIndex + 1},
                            new() { Value=id}
                        }
                    };
                    insertCommand.ExecuteNonQuery();

                    changingWorker.Surname = SurnameStr.Text.Trim();
                    changingWorker.Name = NameStr.Text.Trim();
                    changingWorker.Patronymic = PatronymicStr.Text.Trim();

                    if (JobStr.SelectedIndex == 0)
                    {
                        changingWorker.Job = "администратор";
                    }
                    else
                    {
                        changingWorker.Job = "библиотекарь";
                    }
                    DialogResult = true;

                    Close();
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
    }
}
