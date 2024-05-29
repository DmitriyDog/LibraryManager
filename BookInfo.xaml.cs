using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
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
using System.Windows.Media.TextFormatting;
using System.Windows.Shapes;
using Npgsql;

namespace Library
{
    public partial class BookInfo : Window
    {
        // tabStatus
        // 0 - долги по данной книге
        // 1 - все читатели, которым можно будет выдать книгу
        // 2 - описание книги
        private int tabStatus = 1;
        private int idBook = 0;
        private Book viewBook = null;
        private string loginUser = "";
        private List<BorrowedBook> books = new List<BorrowedBook>();
        private List<Reader> readers = new List<Reader>();
        private List<Reader> searchReader = new List<Reader>();
        private string connectionString = LoginWindow.connectionString;

        private int searchId = 0;
        private string nameReader = "";
        private string surnameReader = "";
        private string patronymicReader = "";
        private string phone = "";
        public BookInfo(Book viewBook, string login)
        {
            InitializeComponent();

            idBook = viewBook.Id;
            this.viewBook = viewBook;
            loginUser = login;

            LoadBorrowedBooks();
            LoadReaders();

            LoadThisBook();
        }

        private void LoadThisBook()
        {
            NpgsqlDataReader reader;
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                var command = new NpgsqlCommand("SELECT books.id_book, books.name, author.surname," +
                    " author.name, author.patronymic, edition.name, lang.name, books.year, books.number_of_books, books.description, books.placement FROM books," +
                    " author, edition, lang" +
                    " WHERE books.id_author = author.id_author AND books.id_edition = edition.id_edition" +
                    " AND books.id_lang = lang.id_lang AND id_book = " + idBook, connection);
                reader = command.ExecuteReader();
                reader.Read();
                BookTitle.Text = (string)reader[1];
                SurnameStr.Text = (string)reader[2];
                NameStr.Text = (string)reader[3];
                PatronymicStr.Text = (string)reader[4];
                EditionStr.Text = (string)reader[5];
                LanguageStr.Text = (string)reader[6];
                YearStr.Text = reader[7].ToString();
                NumberOfBooksStr.Text = reader[8].ToString();
                DescriptionText.Text = (string)reader[9];
                BookPlacement.Text = (string)reader[10];
            }
        }

        private void LoadReaders()
        {
            DataTable db = new DataTable();

            using (var connectionReader = new NpgsqlConnection(connectionString))
            {
                connectionReader.Open();
                var command = new NpgsqlCommand("SELECT id_reader, surname, name, patronymic_name, phone_number FROM readers", connectionReader);
                NpgsqlDataReader reader = command.ExecuteReader();
                db.Load(reader);
                connectionReader.Close();
            }
            readers = new List<Reader>();
            foreach (DataRow row in db.Rows)
            {
                Reader man = new Reader();
                man.Id = Convert.ToInt32(row.ItemArray[0]);
                man.Surname = row.ItemArray[1].ToString();
                man.Name = row.ItemArray[2].ToString();
                man.Patronymic = row.ItemArray[3].ToString();
                man.PhoneNumber = row.ItemArray[4].ToString();
                readers.Add(man);
            }
        }

        private void LoadBorrowedBooks()
        {
            DataTable db = new DataTable();
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                var command = new NpgsqlCommand("SELECT id_borrowed_book, login, id_reader," +
                    " borrow_date, return_date, is_returned FROM borrowed_books WHERE id_book = " + viewBook.Id, connection);
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
                book.TitleBook = viewBook.Title;
                books.Add(book);
            }
        }

        private void CorrectTable()
        {
            if (tabStatus == 0)
            {
                InfoTab.Columns[0].Header = "Id долга";
                InfoTab.Columns[1].Visibility = Visibility.Hidden;
                InfoTab.Columns[2].Visibility = Visibility.Hidden;
                InfoTab.Columns[3].Header = "Id читателя";
                InfoTab.Columns[4].Header = "Логин сотрудника";
                InfoTab.Columns[5].Header = "Дата выдачи";
                InfoTab.Columns[6].Header = "Дата возврата";
                InfoTab.Columns[7].Header = "Сдано";
            }
            else if (tabStatus == 1)
            {
                InfoTab.Columns[0].Header = "Id читателя";
                InfoTab.Columns[1].Header = "Фамилия";
                InfoTab.Columns[2].Header = "Имя";
                InfoTab.Columns[3].Header = "Отчество";
                InfoTab.Columns[4].Header = "Номер телефона";
            }
            foreach (var column in InfoTab.Columns)
            {
                column.MinWidth = column.ActualWidth;
                column.Width = new DataGridLength(1, DataGridLengthUnitType.Star);
            }
            InfoTab.Items.Refresh();
        }

        private void ChangeBtn_Click(object sender, RoutedEventArgs e)
        {
            AddBook changeBook = new AddBook(viewBook);
            changeBook.Owner = this;
            changeBook.ShowDialog();
            if (changeBook.DialogResult == true)
            {
                if (Owner is LibrarianProfile)
                {
                    ((LibrarianProfile)Owner).InfoTab.Items.Refresh();
                    ((LibrarianProfile)Owner).LoadLoans();
                }
                LoadThisBook();
                InfoTab.Items.Refresh();
            }
        }

        private void InfoTab_Loaded(object sender, RoutedEventArgs e)
        {
            InfoTab.ItemsSource = readers;
            CorrectTable();
        }

        private void ReadersBtn_Click(object sender, RoutedEventArgs e)
        {
            if (tabStatus == 0 || tabStatus == 2)
            {
                BookParams.Visibility = Visibility.Hidden;
                DescriptionText.Visibility = Visibility.Hidden;
                InfoTab.Visibility = Visibility.Visible;
                ChangeStatus.Visibility = Visibility.Collapsed;
            }
            ReaderParams.Visibility = Visibility.Visible;
            BorrowBtn.Visibility = Visibility.Visible;
            SearchBtn.Visibility = Visibility.Visible;
            tabStatus = 1;
            InfoTab.ItemsSource = readers;
            CorrectTable();
        }

        private void BorrowPageBtn_Click(object sender, RoutedEventArgs e)
        {
            if (tabStatus == 1)
            {
                BorrowBtn.Visibility = Visibility.Hidden;
                ReaderParams.Visibility = Visibility.Hidden;
            }
            if (tabStatus == 2)
            {
                DescriptionText.Visibility = Visibility.Hidden;
                InfoTab.Visibility = Visibility.Visible;
            }
            BookParams.Visibility = Visibility.Visible;
            SearchBtn.Visibility = Visibility.Hidden;
            ChangeStatus.Visibility = Visibility.Visible;
            tabStatus = 0;
            InfoTab.ItemsSource = books;
            CorrectTable();
        }

        private void DescriptionBtn_Click(object sender, RoutedEventArgs e)
        {
             if (tabStatus == 1)
            {
                BorrowBtn.Visibility = Visibility.Hidden;
                ReaderParams.Visibility = Visibility.Hidden;
            }
            ChangeStatus.Visibility = Visibility.Collapsed;
            DescriptionText.Visibility = Visibility.Visible;
            InfoTab.Visibility = Visibility.Hidden;
            BookParams.Visibility = Visibility.Visible;
            tabStatus = 2;
            InfoTab.ItemsSource = readers;
            CorrectTable();
        }

        // Выдача книги выделенному читателю
        private void BorrowBtn_Click(object sender, RoutedEventArgs e)
        {
            if (viewBook.Number > 0 && InfoTab.SelectedIndex != -1)
            {
                ChooseDate period = new ChooseDate();
                period.Owner = this;
                period.ShowDialog();
                DateTime start = period.selectedStart;
                DateTime end = period.selectedEnd;
                if (period.DialogResult == true)
                {
                    var select = (Reader)InfoTab.SelectedItem;
                    Int64 newId;
                    using (var connection = new NpgsqlConnection(connectionString))
                    {
                        connection.Open();
                        var command = new NpgsqlCommand("INSERT INTO borrowed_books" +
                             " (id_book, login, id_reader, borrow_date, return_date) VALUES (($1), ($2), ($3), ($4), ($5))" +
                             " RETURNING id_borrowed_book", connection)
                        {
                            Parameters =
                            {
                                new() { Value= idBook},
                                new() { Value= loginUser},
                                new() { Value= select.Id},
                                new() {Value = start},
                                new() {Value = end}
                            }
                        };
                        newId = (Int64)command.ExecuteScalar();
                        var command2 = new NpgsqlCommand("UPDATE books " +
                            " SET number_of_books = number_of_books - 1 WHERE id_book = " + idBook, connection);
                        command2.ExecuteNonQuery();
                    }
                    BorrowedBook newLoan = new BorrowedBook();
                    newLoan.isReturned = false;
                    newLoan.ReturnDate = end.ToString().Substring(0, 10);
                    newLoan.BorrowDate = start.ToString().Substring(0, 10);
                    newLoan.LoginEmployee = loginUser;
                    newLoan.IdReader = select.Id;
                    newLoan.Id = newId;
                    newLoan.IdBook = idBook;
                    newLoan.TitleBook = viewBook.Title;

                    viewBook.Number -= 1;
                    var mainWin = Owner as LibrarianProfile;
                    mainWin.loans.Add(newLoan);
                    DialogResult = true;
                    Close();
                }
            }
        }

        // Поля читателя для поиска

        private void ExitBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void AddToRequest(ref string request, ref bool wasEdited)
        {
            if (wasEdited)
            {
                request = request + "AND ";
            }
            wasEdited = true;
        }

        private void SearchBtn_Click(object sender, RoutedEventArgs e)
        {
            string request;
            if (searchId != 0 || surnameReader != "" || nameReader != "" || patronymicReader != "" || phone != "")
            {
                bool wasEdited = false;
                request = "SELECT id_reader, surname, name, patronymic_name, phone_number FROM readers WHERE ";
                if (searchId != 0)
                {
                    request = request + "id_reader = " + searchId + " ";
                    wasEdited = true;
                }
                if (surnameReader != "")
                {
                    AddToRequest(ref request, ref wasEdited);
                    request = request + "surname ILIKE '%" + surnameReader + "%' ";
                }
                if (nameReader != "")
                {
                    AddToRequest(ref request, ref wasEdited);
                    request = request + "name ILIKE '%" + nameReader + "%' ";
                }
                if (patronymicReader != "")
                {
                    AddToRequest(ref request, ref wasEdited);
                    request = request + "patronymic_name ILIKE '%" + patronymicReader + "%' ";
                }
                if (phone != "")
                {
                    AddToRequest(ref request, ref wasEdited);
                    request = request + "phone_number LIKE '%" + phone + "%' ";
                }
                DataTable db = new DataTable();

                using (var connectionReader = new NpgsqlConnection(connectionString))
                {
                    connectionReader.Open();
                    var command = new NpgsqlCommand(request, connectionReader);
                    NpgsqlDataReader reader = command.ExecuteReader();
                    db.Load(reader);
                    connectionReader.Close();
                }
                searchReader = new List<Reader>();
                foreach (DataRow row in db.Rows)
                {
                    Reader man = new Reader();
                    man.Id = Convert.ToInt32(row.ItemArray[0]);
                    man.Surname = row.ItemArray[1].ToString();
                    man.Name = row.ItemArray[2].ToString();
                    man.Patronymic = row.ItemArray[3].ToString();
                    man.PhoneNumber = row.ItemArray[4].ToString();
                    searchReader.Add(man);
                }

                InfoTab.ItemsSource = searchReader;
            }
            else
            {
                searchReader.Clear();
                InfoTab.ItemsSource = readers;
            }
            InfoTab.Items.Refresh();
            CorrectTable();
        }

        private void IdStr_TextChanged(object sender, TextChangedEventArgs e)
        {
            var idBox = sender as TextBox;
            int testId;
            if (idBox.Text != "" && !int.TryParse(idBox.Text, out testId))
            {
                idBox.Text = searchId.ToString();
                idBox.Select(idBox.Text.Length, 0);
            }
            else
            {
                if (idBox.Text == "")
                {
                    searchId = 0;
                }
                else
                {
                    searchId = Convert.ToInt32(idBox.Text);
                }
            }
        }

        private void SurnameReaderStr_TextChanged(object sender, TextChangedEventArgs e)
        {
            var surnameBox = sender as TextBox;
            surnameReader = surnameBox.Text.Trim();
        }

        private void NameReaderStr_TextChanged(object sender, TextChangedEventArgs e)
        {
            var nameBox = sender as TextBox;
            nameReader = nameBox.Text.Trim();
        }

        private void PatronymicReaderStr_TextChanged(object sender, TextChangedEventArgs e)
        {
            var patronymicBox = sender as TextBox;
            patronymicReader = patronymicBox.Text.Trim();
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

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (InfoTab.SelectedIndex != -1)
            {
                BorrowedBook selectedLoan = InfoTab.SelectedItem as BorrowedBook;
                var mainWin = Owner as LibrarianProfile;
                bool borrowStatus = false;
                bool wasEdited = false;
                if (selectedLoan.isReturned && viewBook.Number > 0)
                {
                    viewBook.Number -= 1;
                    selectedLoan.isReturned = false;
                    wasEdited = true;
                }
                else if (!selectedLoan.isReturned)
                {
                    viewBook.Number += 1;
                    borrowStatus = true;
                    selectedLoan.isReturned = true;
                    wasEdited = true;
                }
                if (wasEdited)
                {
                    NumberOfBooksStr.Text = viewBook.Number.ToString();
                    InfoTab.Items.Refresh();
                    using (var connection = new NpgsqlConnection(connectionString))
                    {
                        connection.Open();
                        var command = new NpgsqlCommand("UPDATE borrowed_books SET is_returned = " + borrowStatus +
                            " WHERE id_borrowed_book = " + selectedLoan.Id, connection);
                        command.ExecuteNonQuery();
                        var command2 = new NpgsqlCommand("UPDATE books SET number_of_books = "
                        + viewBook.Number + " WHERE id_book = " + viewBook.Id, connection);
                        command2.ExecuteNonQuery();
                    }
                    BorrowedBook toChangeStat = mainWin.loans.Where(loan => loan.Id == selectedLoan.Id).FirstOrDefault();
                    toChangeStat.isReturned = borrowStatus;
                    mainWin.InfoTab.Items.Refresh();
                }
            }
        }
    }
}
