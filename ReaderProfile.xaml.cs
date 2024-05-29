using Npgsql;
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

namespace Library
{
    public partial class ReaderProfile : Window
    {
        private List<Book> books = new List<Book>();
        private List<BorrowedBook> loans = new List<BorrowedBook>();
        private List<Book> searchBooks = new List<Book>();
        private string connectionString = LoginWindow.connectionString;

        private Reader viewReader = null;
        private bool tabStatus = true;

        private string searchEdition = "";
        private string searchLanguage = "";
        private string searchNumber = "";
        private string searchAuthorSurname = "";
        private string searchAuthorName = "";
        private string searchTitle = "";
        private string searchYear = "";
        private string loginUser = "";

        // tabStatus
        // false - долги
        // true - книги
        public ReaderProfile(Reader viewReader, string login)
        {
            InitializeComponent();

            this.viewReader = viewReader;
            IdStr.Text = viewReader.Id.ToString();
            SurnameReaderStr.Text = viewReader.Surname;
            NameReaderStr.Text = viewReader.Name;
            PatronymicReaderStr.Text = viewReader.Patronymic;
            NumStr.Text = viewReader.PhoneNumber;
            loginUser = login;

            LoadLoans();
            LoadBooks();
        }

        private void LoadLoans()
        {
            loans.Clear();
            DataTable db = new DataTable();
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                var command = new NpgsqlCommand("SELECT borrowed_books.id_borrowed_book," +
                    " borrowed_books.login, borrowed_books.id_reader, borrowed_books.borrow_date," +
                    " borrowed_books.return_date, borrowed_books.is_returned, borrowed_books.id_book," +
                    " books.name FROM borrowed_books, books WHERE books.id_book = borrowed_books.id_book AND borrowed_books.id_reader = " + viewReader.Id, connection);
                var reader = command.ExecuteReader();
                db.Load(reader);
            }
            foreach (DataRow row in db.Rows)
            {
                BorrowedBook book = new BorrowedBook();
                book.Id = Convert.ToInt64(row.ItemArray[0]);
                book.LoginEmployee = row.ItemArray[1].ToString();
                book.IdReader = (int)row.ItemArray[2];
                book.BorrowDate = row.ItemArray[3].ToString().Substring(0, 10);
                book.ReturnDate = row.ItemArray[4].ToString().Substring(0, 10);
                book.isReturned = (bool)row.ItemArray[5];
                book.IdBook = (int)row.ItemArray[6];
                book.TitleBook = (string)row.ItemArray[7];
                loans.Add(book);
            }
        }

        private void LoadBooks()
        {
            books.Clear();
            DataTable db = new DataTable();
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                var command = new NpgsqlCommand("SELECT books.id_book, books.name, author.surname," +
                    " author.name, edition.name, lang.name, books.year, books.number_of_books FROM books," +
                    " author, edition, lang" +
                    " WHERE books.id_author = author.id_author AND books.id_edition = edition.id_edition" +
                    " AND books.id_lang = lang.id_lang", connection);
                NpgsqlDataReader reader = command.ExecuteReader();
                db.Load(reader);
                connection.Close();
            }
            foreach (DataRow row in db.Rows)
            {
                Book book = new Book();
                book.Id = Convert.ToInt32(row.ItemArray[0]);
                book.Title = row.ItemArray[1].ToString();
                book.AuthorSurname = row.ItemArray[2].ToString();
                book.AuthorName = row.ItemArray[3].ToString();
                book.Edition = row.ItemArray[4].ToString();
                book.Language = row.ItemArray[5].ToString();
                book.Year = Convert.ToInt32(row.ItemArray[6]);
                book.Number = Convert.ToInt32(row.ItemArray[7]);
                books.Add(book);
            }
        }

        private void CorrectTable()
        {
            // книги
            if (tabStatus)
            {
                InfoTab.Columns[0].Visibility = Visibility.Collapsed;
                InfoTab.Columns[1].Header = "Название";
                InfoTab.Columns[2].Header = "Фамилия автора";
                InfoTab.Columns[3].Header = "Имя автора";
                InfoTab.Columns[4].Header = "Издательство";
                InfoTab.Columns[5].Header = "Язык";
                InfoTab.Columns[6].Header = "Год";
                InfoTab.Columns[7].Header = "Количество";
            }
            else  // долги
            {
                InfoTab.Columns[0].Header = "Id долга";
                InfoTab.Columns[1].Visibility = Visibility.Hidden;
                InfoTab.Columns[2].Header = "Название книги";
                InfoTab.Columns[3].Visibility = Visibility.Hidden;
                InfoTab.Columns[4].Header = "Логин сотрудника";
                InfoTab.Columns[5].Header = "Дата выдачи";
                InfoTab.Columns[6].Header = "Дата возврата";
                InfoTab.Columns[7].Header = "Сдано";
            }
            foreach (var column in InfoTab.Columns)
            {
                column.MinWidth = column.ActualWidth;
                column.Width = new DataGridLength(1, DataGridLengthUnitType.Star);
            }
        }

        private void BooksBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!tabStatus)
            {
                tabStatus = true;
                BookParams.Visibility = Visibility.Visible;
                BorrowBtn.Visibility = Visibility.Visible;
                ReaderParams.Visibility = Visibility.Hidden;
                changeStatus.Visibility = Visibility.Collapsed;
            }
            InfoTab.ItemsSource = books;
            CorrectTable();
        }

        private void BorrowPageBtn_Click(object sender, RoutedEventArgs e)
        {
            if (tabStatus)
            {
                tabStatus = false;
                ReaderParams.Visibility = Visibility.Visible;
                BookParams.Visibility = Visibility.Hidden;
                BorrowBtn.Visibility = Visibility.Hidden;
                changeStatus.Visibility = Visibility.Visible;
            }
            InfoTab.ItemsSource = loans;
            CorrectTable();
        }

        private void ChangeBtn_Click(object sender, RoutedEventArgs e)
        {
            AddReader changeReader = new AddReader(viewReader);
            changeReader.Owner = this;
            changeReader.ShowDialog();
            if (changeReader.DialogResult == true)
            {
                IdStr.Text = viewReader.Id.ToString();
                SurnameReaderStr.Text = viewReader.Surname;
                NameReaderStr.Text = viewReader.Name;
                PatronymicReaderStr.Text = viewReader.Patronymic;
                NumStr.Text = viewReader.PhoneNumber;
            }
        }

        private void BorrowBtn_Click(object sender, RoutedEventArgs e)
        {
            var selectedBook = (Book)InfoTab.SelectedItem;
            if (InfoTab.SelectedIndex != -1 && selectedBook.Number > 0)
            {
                ChooseDate period = new ChooseDate();
                period.Owner = this;
                period.ShowDialog();
                DateTime start = period.selectedStart;
                DateTime end = period.selectedEnd;
                if (period.DialogResult == true)
                {
                    Int64 newId;
                    using (var connection = new NpgsqlConnection(connectionString))
                    {
                        connection.Open();
                        var command = new NpgsqlCommand("INSERT INTO borrowed_books" +
                             " (id_book, login, id_reader, borrow_date, return_date) " +
                             "VALUES (($1), ($2), ($3), ($4), ($5))" +
                             " RETURNING id_borrowed_book", connection)
                        {
                            Parameters =
                            {
                                new() { Value= selectedBook.Id},
                                new() { Value= loginUser},
                                new() { Value= viewReader.Id},
                                new() {Value = start},
                                new() {Value = end}
                            }
                        };
                        newId = (Int64)command.ExecuteScalar();
                        var command2 = new NpgsqlCommand("UPDATE books " +
                            " SET number_of_books = number_of_books - 1 WHERE id_book = " + selectedBook.Id, connection);
                        command2.ExecuteNonQuery();
                    }
                    BorrowedBook newLoan = new BorrowedBook();
                    newLoan.isReturned = false;
                    newLoan.ReturnDate = end.ToString().Substring(0, 10);
                    newLoan.BorrowDate = start.ToString().Substring(0, 10);
                    newLoan.LoginEmployee = loginUser;
                    newLoan.IdReader = viewReader.Id;
                    newLoan.Id = newId;
                    newLoan.IdBook = selectedBook.Id;
                    newLoan.TitleBook = selectedBook.Title;
                    var mainWin = Owner as LibrarianProfile;
                    mainWin.loans.Add(newLoan);

                    Book findItem = mainWin.books.Where(book => book.Id == selectedBook.Id).First();
                    findItem.Number -= 1;
                    DialogResult = true;
                    Close();
                }
            }
        }

        private void SearchBtn_Click(object sender, RoutedEventArgs e)
        {
            if (searchTitle != "" || searchEdition != "" || searchYear != "" || searchAuthorSurname != ""
                    || searchAuthorName != "" || searchLanguage != "")
            {
                string request = "SELECT books.id_book, books.name, author.surname, author.name, edition.name, lang.name, books.year, books.number_of_books FROM books, author, edition, lang" +
                    " WHERE books.id_author = author.id_author AND lang.id_lang = books.id_lang AND edition.id_edition = books.id_edition ";
                if (searchTitle != "")
                {
                    request = request + "AND ";
                    request = request + "books.name ILIKE '%" + searchTitle + "%' ";
                }
                if (searchEdition != "")
                {
                    request = request + "AND ";
                    request += "edition.name ILIKE '%" + searchEdition + "%' ";
                }
                if (searchYear != "")
                {
                    request = request + "AND ";
                    request += "CAST(books.year as varchar) ILIKE '%" + searchYear + "%' ";
                }
                if (searchAuthorSurname != "")
                {
                    request = request + "AND ";
                    request += "author.surname ILIKE '%" + searchAuthorSurname + "%' ";
                }
                if (searchAuthorName != "")
                {
                    request = request + "AND ";
                    request += "author.name ILIKE '%" + searchAuthorName + "%' ";
                }
                if (searchLanguage != "")
                {
                    request = request + "AND ";
                    request += "lang.name ILIKE '%" + searchLanguage + "%' ";
                }

                DataTable db = new DataTable();
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    var command = new NpgsqlCommand(request, connection);
                    NpgsqlDataReader reader = command.ExecuteReader();
                    db.Load(reader);
                    connection.Close();
                }
                searchBooks = new List<Book>();
                foreach (DataRow row in db.Rows)
                {
                    Book book = new Book();
                    book.Id = Convert.ToInt32(row.ItemArray[0]);
                    book.Title = row.ItemArray[1].ToString();
                    book.AuthorSurname = row.ItemArray[2].ToString();
                    book.AuthorName = row.ItemArray[3].ToString();
                    book.Edition = row.ItemArray[4].ToString();
                    book.Language = row.ItemArray[5].ToString();
                    book.Year = Convert.ToInt32(row.ItemArray[6]);
                    book.Number = Convert.ToInt32(row.ItemArray[7]);
                    searchBooks.Add(book);
                }

                InfoTab.ItemsSource = searchBooks;
            }
            else
            {
                searchBooks.Clear();
                InfoTab.ItemsSource = books;
            }
            InfoTab.Items.Refresh();
            CorrectTable();
        }

        private void EditionStr_TextChanged(object sender, TextChangedEventArgs e)
        {
            var editionBox = sender as TextBox;
            searchEdition = editionBox.Text.Trim();
        }

        private void YearStr_TextChanged(object sender, TextChangedEventArgs e)
        {
            var yearBox = sender as TextBox;
            uint testNumber;
            if (yearBox.Text != "" && !UInt32.TryParse(yearBox.Text, out testNumber))
            {
                yearBox.Text = searchYear;
                yearBox.Select(yearBox.Text.Length, 0);
            }
            else
            {
                searchYear = yearBox.Text.Trim();
            }
        }

        private void LanguageStr_TextChanged(object sender, TextChangedEventArgs e)
        {
            var languageBox = sender as TextBox;
            searchLanguage = languageBox.Text.Trim();
        }

        private void NumberOfBooksStr_TextChanged(object sender, TextChangedEventArgs e)
        {
            var numberBox = sender as TextBox;
            uint testNumber;
            if (numberBox.Text != "" && !UInt32.TryParse(numberBox.Text, out testNumber))
            {
                numberBox.Text = searchNumber;
                numberBox.Select(numberBox.Text.Length, 0);
            }
            else
            {
                searchNumber = numberBox.Text.Trim();
            }
        }

        private void SurnameStr_TextChanged(object sender, TextChangedEventArgs e)
        {
            var surnameBox = sender as TextBox;
            searchAuthorSurname = surnameBox.Text.Trim();
        }

        private void NameStr_TextChanged(object sender, TextChangedEventArgs e)
        {
            var nameBox = sender as TextBox;
            searchAuthorName = nameBox.Text.Trim();
        }

        private void TitleStr_TextChanged(object sender, TextChangedEventArgs e)
        {
            var titleBox = sender as TextBox;
            searchTitle = titleBox.Text.Trim();
        }

        private void ExitBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void InfoTab_Loaded(object sender, RoutedEventArgs e)
        {
            InfoTab.ItemsSource = books;
            CorrectTable();
        }

        private void changeStatus_Click(object sender, RoutedEventArgs e)
        {
            if (InfoTab.SelectedIndex != -1)
            {
                BorrowedBook selectedLoan = InfoTab.SelectedItem as BorrowedBook;
                var mainWin = Owner as LibrarianProfile;
                Book changeNumber = mainWin.books.Where(search => search.Id == selectedLoan.IdBook).First();
                bool borrowStatus = false;
                bool wasEdited = false;
                if (selectedLoan.isReturned && changeNumber.Number > 0)
                {
                    changeNumber.Number -= 1;
                    selectedLoan.isReturned = false;
                    wasEdited = true;
                }
                else if (!selectedLoan.isReturned)
                {
                    changeNumber.Number += 1;
                    borrowStatus = true;
                    selectedLoan.isReturned = true;
                    wasEdited = true;
                }
                if (wasEdited)
                {
                    Book subWinBook = books.Where(search => search.Id == changeNumber.Id).First();
                    subWinBook.Number = changeNumber.Number;
                    InfoTab.Items.Refresh();
                    using (var connection = new NpgsqlConnection(connectionString))
                    {
                        connection.Open();
                        var command = new NpgsqlCommand("UPDATE borrowed_books SET is_returned = " + borrowStatus +
                            " WHERE id_borrowed_book = " + selectedLoan.Id, connection);
                        command.ExecuteNonQuery();
                        var command2 = new NpgsqlCommand("UPDATE books SET number_of_books = "
                        + changeNumber.Number + " WHERE id_book = " + changeNumber.Id, connection);
                        command2.ExecuteNonQuery();
                    }
                    BorrowedBook toChangeStat = mainWin.loans.Where(loan => loan.Id == selectedLoan.Id).FirstOrDefault();
                    toChangeStat.isReturned = borrowStatus;
                }
            }
        }
    }
}
