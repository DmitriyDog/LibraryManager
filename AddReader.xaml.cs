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
    
    public partial class AddReader : Window
    {
        private string connectionString = LoginWindow.connectionString;
        public Reader toChange = null;
        private string phone = "";
        public AddReader()
        {
            InitializeComponent();
        }

        public AddReader(Reader toChangeReader)
        {
            InitializeComponent();
            toChange = toChangeReader;
            SurnameReaderStr.Text = toChange.Surname;
            NameReaderStr.Text = toChange.Name;
            PatronymicReaderStr.Text = toChange.Patronymic;
            NumStr.Text = toChange.PhoneNumber;
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

        private void AcceptBtn_Click(object sender, RoutedEventArgs e)
        {
            if (SurnameReaderStr.Text.Trim() != "" && NameReaderStr.Text.Trim() != "")
            {
                string surname = SurnameReaderStr.Text.Trim();
                string name = NameReaderStr.Text.Trim();
                string patronymic = PatronymicReaderStr.Text.Trim();
                if (toChange == null)
                {
                    int newId;
                    using (var connection = new NpgsqlConnection(connectionString))
                    {
                        connection.Open();
                        var command = new NpgsqlCommand("INSERT INTO readers (surname, name, patronymic_name, phone_number) VALUES (($1), ($2), ($3), ($4)) RETURNING id_reader", connection)
                        {
                            Parameters =
                            {
                                new() {Value = surname},
                                new() {Value = name},
                                new() {Value = patronymic},
                                new() {Value = phone}
                            }
                        };
                        newId = (int)command.ExecuteScalar();
                    }
                    Reader newReader = new Reader();
                    newReader.Surname = surname;
                    newReader.Name = name;
                    newReader.Patronymic = patronymic;
                    newReader.PhoneNumber = phone;
                    newReader.Id = newId;
                    var mainWin = Owner as LibrarianProfile;
                    mainWin.readers.Add(newReader);
                }
                else
                {
                    using (var connection = new NpgsqlConnection(connectionString))
                    {
                        connection.Open();
                        var command = new NpgsqlCommand("UPDATE readers SET surname = ($1), name = ($2), patronymic_name = ($3), phone_number = ($4) WHERE id_reader = ($5)", connection)
                        {
                            Parameters =
                            {
                                new() {Value = surname},
                                new() {Value = name},
                                new() {Value = patronymic},
                                new() {Value = phone},
                                new() {Value = toChange.Id}
                            }
                        };
                    }
                    toChange.Surname = surname;
                    toChange.Name = name;
                    toChange.Patronymic = patronymic;
                    toChange.PhoneNumber = phone;
                }
                DialogResult = true;
                Close();
            }
            else
            {
                ErrorMessage.Text = "Поля фамилии и имени обязательны для заполения";
            }
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
