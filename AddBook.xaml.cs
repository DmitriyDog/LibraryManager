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
    public partial class AddBook : Window
    {
        private string connectionString = LoginWindow.connectionString;
        private string year = "";
        private string number = "";
        private int idAuthor = 0;
        private int idLang = 0;
        private int idEdition = 0;
        private int toChangeId = -1;
        Book toChangeBook = null;
        public AddBook()
        {
            InitializeComponent();
        }

        public AddBook(Book toChangeBook)
        {
            InitializeComponent();
            this.toChangeBook = toChangeBook;
            toChangeId = toChangeBook.Id;
            NpgsqlDataReader reader;
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                var command = new NpgsqlCommand("SELECT books.name, author.surname, author.id_author," +
                    " edition.name, edition.id_edition, lang.name, lang.id_lang, books.year," +
                    " books.number_of_books, books.description, books.placement FROM books," +
                    " author, edition, lang" +
                    " WHERE books.id_author = author.id_author AND books.id_edition = edition.id_edition" +
                    " AND books.id_lang = lang.id_lang AND id_book = " + toChangeId, connection);
                reader = command.ExecuteReader();
                reader.Read();
                TitleStr.Text = (string)reader[0];
                AuthorBtn.Content = (string)reader[1];
                idAuthor = (int)reader[2];
                EditionBtn.Content = (string)reader[3];
                idEdition = (int)reader[4];
                LangBtn.Content = (string)reader[5];
                idLang = (int)reader[6];
                YearStr.Text = reader[7].ToString();
                NumberStr.Text = reader[8].ToString();
                DescriptionStr.Text = (string)reader[9];
                PlacementStr.Text = (string)reader[10];
            }
        }

        private void NumberStr_TextChanged(object sender, TextChangedEventArgs e)
        {
            var numberBox = sender as TextBox;
            UInt16 test;
            if (numberBox.Text != "" && !UInt16.TryParse(numberBox.Text, out test))
            {
                numberBox.Text = number;
                numberBox.Select(numberBox.Text.Length, 0);
            }
            else
            {
                number = numberBox.Text;
            }
        }

        private void AcceptBtn_Click(object sender, RoutedEventArgs e)
        {
            if (year == "" || idAuthor == 0 || idLang == 0 || idEdition == 0 || TitleStr.Text.Trim() == "")
            {
                ErrorMessage.Text = "Ошибка. Поля 'Название', 'Год издания', 'Язык', 'Издательство' и 'Автор' обязательны для заполнения";
            }
            else if (toChangeId != -1)
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    var command = new NpgsqlCommand("UPDATE books SET name = ($1), id_author = ($2)," +
                        " id_edition = ($3), id_lang = ($4), year = ($5), description = ($6)," +
                        " placement = ($7), number_of_books = ($8) WHERE id_book = ($9)", connection)
                    {
                        Parameters =
                            {
                                new() { Value= TitleStr.Text.Trim()},
                                new() { Value= idAuthor},
                                new() { Value= idEdition},
                                new() { Value= idLang},
                                new() { Value= Convert.ToInt16(year)},
                                new() { Value= DescriptionStr.Text.Trim()},
                                new() { Value= PlacementStr.Text.Trim()},
                                new() { Value= Convert.ToInt16(number)},
                                new() {Value = toChangeId}
                            }
                    };
                    command.ExecuteNonQuery();

                    toChangeBook.Title = TitleStr.Text.Trim();
                    toChangeBook.Number = Convert.ToInt32(number);
                    toChangeBook.Year = Convert.ToInt32(year);
                    var command2 = new NpgsqlCommand("SELECT author.surname," +
                        " author.name, edition.name, lang.name FROM books," +
                        " author, edition, lang" +
                        " WHERE books.id_author = author.id_author AND books.id_edition = edition.id_edition" +
                        " AND books.id_lang = lang.id_lang AND books.id_book = " + toChangeId, connection);
                    var reader = command2.ExecuteReader();
                    reader.Read();
                    toChangeBook.AuthorSurname = (string)reader[0];
                    toChangeBook.AuthorName = (string)reader[1];
                    toChangeBook.Edition = (string)reader[2];
                    toChangeBook.Language = (string)reader[3];
                }

                DialogResult = true;
                Close();
            }
            else
            {
                int idBook = 0;
                Book newBook = new Book();
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    var command = new NpgsqlCommand("INSERT INTO books (name, id_author, id_edition, id_lang, year, description, placement, number_of_books)" +
                        "VALUES (($1), ($2), ($3), ($4), ($5), ($6), ($7), ($8)) RETURNING id_book", connection)
                    {
                        Parameters =
                            {
                                new() { Value= TitleStr.Text.Trim()},
                                new() { Value= idAuthor},
                                new() { Value= idEdition},
                                new() { Value= idLang},
                                new() { Value= Convert.ToInt16(year)},
                                new() { Value= DescriptionStr.Text.Trim()},
                                new() { Value= PlacementStr.Text.Trim()},
                                new() { Value= Convert.ToInt16(number)}
                            }
                    };
                    idBook = (int)command.ExecuteScalar();
                    var command2 = new NpgsqlCommand("SELECT books.name, author.surname," +
                        " author.name, edition.name, lang.name, books.year, books.number_of_books FROM books," +
                        " author, edition, lang" +
                        " WHERE books.id_author = author.id_author AND books.id_edition = edition.id_edition" +
                        " AND books.id_lang = lang.id_lang AND books.id_book = " + idBook, connection);
                    NpgsqlDataReader reader = command2.ExecuteReader();
                    reader.Read();
                    var mainWindow = Owner as LibrarianProfile;
                    newBook.Title = reader[0].ToString();
                    newBook.Id = idBook;
                    newBook.AuthorSurname = reader[1].ToString();
                    newBook.AuthorName = reader[2].ToString();
                    newBook.Edition = reader[3].ToString();
                    newBook.Language = reader[4].ToString();
                    newBook.Year = Convert.ToInt32(reader[5]);
                    newBook.Number = Convert.ToInt32(reader[6]);
                    mainWindow.books.Add(newBook);
                    connection.Close();
                }
                toChangeBook = newBook;
                DialogResult = true;
                Close();
            }
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void AuthorBtn_Click(object sender, RoutedEventArgs e)
        {
            ParamsTab chooseAuthor = new ParamsTab(0);
            chooseAuthor.Owner = this;
            chooseAuthor.ShowDialog();
            if (chooseAuthor.DialogResult == true)
            {
                idAuthor = chooseAuthor.id;
                AuthorBtn.Content = chooseAuthor.returnString;
            }
            else
            {
                idAuthor = 0;
                AuthorBtn.Content = "Выбрать";
            }
        }

        private void LangBtn_Click(object sender, RoutedEventArgs e)
        {
            ParamsTab chooseLang = new ParamsTab(2);
            chooseLang.Owner = this;
            chooseLang.ShowDialog();
            if (chooseLang.DialogResult == true)
            {
                idLang = chooseLang.id;
                LangBtn.Content = chooseLang.returnString;
            }
            else
            {
                idLang = 0;
                LangBtn.Content = "Выбрать";
            }
        }

        private void EditionBtn_Click(object sender, RoutedEventArgs e)
        {
            ParamsTab chooseEdition = new ParamsTab(1);
            chooseEdition.Owner = this;
            chooseEdition.ShowDialog();
            if (chooseEdition.DialogResult == true)
            {
                idEdition = chooseEdition.id;
                EditionBtn.Content = chooseEdition.returnString;
            }
            else
            {
                idEdition = 0;
                EditionBtn.Content = "Выбрать";
            }
        }

        private void YearStr_TextChanged(object sender, TextChangedEventArgs e)
        {
            var yearbox = sender as TextBox;
            UInt16 testYear;
            if (yearbox.Text != "" && !UInt16.TryParse(yearbox.Text, out testYear))
            {
                yearbox.Text = year;
                yearbox.Select(yearbox.Text.Length, 0);
            }
            else
            {
                year = yearbox.Text;
            }
        }
    }
}
