using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.TextFormatting;
using System.Windows.Shapes;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

namespace Library
{

    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string AuthorSurname { get; set; }
        public string AuthorName { get; set; }
        public string Language { get; set; }
        public string Edition { get; set; }
        public int Year { get; set; }
        public int Number { get; set; }
    }

    public class Reader
    {
        public int Id { get; set; }
        public string Surname { get; set; }
        public string Name { get; set; }
        public string Patronymic { get; set; }
        public string PhoneNumber { get; set; }
    }

    public class BorrowedBook
    {
        public Int64 Id { get; set; }
        public int IdBook { get; set; }
        public string TitleBook { get; set; }
        public int IdReader { get; set; }
        public string LoginEmployee { get; set; }
        public string BorrowDate { get; set; }
        public string ReturnDate { get; set; }
        public bool isReturned { get; set; }
    }

    public partial class LibrarianProfile : Window
    {
        public List<Book> books = new List<Book>();
        public List<Reader> readers = new List<Reader>();
        public List<BorrowedBook> loans = new List<BorrowedBook>();
        private List<Book> searchBooks = new List<Book>();
        private List<Reader> searchReader = new List<Reader>();
        private List<BorrowedBook> searchLoans = new List<BorrowedBook>();
        internal string connectionString = LoginWindow.connectionString;
        private int viewStatus = 0;
        private string phone = "";
        private string year = "";

        private string titleSearch = "";
        private string editionSearch = "";
        private string languageSearch = "";
        private string surnameSearch = "";
        private string nameSearch = "";
        private string patronymicSearch = "";

        private int searchId = 0;
        private string nameReader = "";
        private string surnameReader = "";
        private string patronymicReader = "";

        private bool isWaiting = false;
        private bool isReturned = false;
        private DateTime? dateBorrow = null;
        private DateTime? dateReturn = null;
        private string searchLogin = "";
        private string searchIdReader = "";
        private string searchIdLoan = "";

        private string loginUser = "";
        // viewStatus 
        // 0 - книги в библиотеке
        // 1 - читатели
        // 2 - книжные долги
        public LibrarianProfile(string login, string name)
        {
            InitializeComponent();

            LoadBooks();
            LoadReaders();
            LoadLoans();

            loginUser = login;
            HelloMessage.Text += name;
        }

        public void LoadBooks()
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

        private void LoadReaders()
        {
            DataTable db = new DataTable();

            using (var connectionReader = new NpgsqlConnection(connectionString))
            {
                connectionReader.Open();
                var command = new NpgsqlCommand("SELECT id_reader, surname, name," +
                    " patronymic_name, phone_number FROM readers", connectionReader);
                NpgsqlDataReader reader = command.ExecuteReader();
                db.Load(reader);
                connectionReader.Close();
            }
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

        public void LoadLoans()
        {
            loans.Clear();
            DataTable db = new DataTable();
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                var command = new NpgsqlCommand("SELECT borrowed_books.id_borrowed_book," +
                    " borrowed_books.login, borrowed_books.id_reader, borrowed_books.borrow_date," +
                    " borrowed_books.return_date, borrowed_books.is_returned, borrowed_books.id_book," +
                    " books.name FROM borrowed_books, books WHERE books.id_book = borrowed_books.id_book", connection);
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

        private void ExitBtn_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow newLogin = new LoginWindow();
            newLogin.Show();
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
            if (viewStatus == 0) // книги
            {
                if (titleSearch != "" || editionSearch != "" || year != "" || surnameSearch != ""
                    || nameSearch != "" || patronymicSearch != "" || languageSearch != "")
                {
                    request = "SELECT books.id_book, books.name, author.surname," +
                        " author.name, edition.name, lang.name, books.year, books.number_of_books" +
                        " FROM books, author, edition, lang" +
                        " WHERE books.id_author = author.id_author AND" +
                        " lang.id_lang = books.id_lang AND edition.id_edition = books.id_edition ";
                    if (titleSearch != "")
                    {
                        request = request + "AND ";
                        request = request + "books.name ILIKE '%" + titleSearch + "%' ";
                    }
                    if (editionSearch != "")
                    {
                        request = request + "AND ";
                        request += "edition.name ILIKE '%" + editionSearch + "%' ";
                    }
                    if (year != "")
                    {
                        request = request + "AND ";
                        request += "CAST(books.year as varchar) ILIKE '%" + year + "%' ";
                    }
                    if (surnameSearch != "")
                    {
                        request = request + "AND ";
                        request += "author.surname ILIKE '%" + surnameSearch + "%' ";
                    }
                    if (nameSearch != "")
                    {
                        request = request + "AND ";
                        request += "author.name ILIKE '%" + nameSearch + "%' ";
                    }
                    if (patronymicSearch != "")
                    {
                        request = request + "AND ";
                        request += "author.patronymic ILIKE '%" + patronymicSearch + "%' ";
                    }
                    if (languageSearch != "")
                    {
                        request = request + "AND ";
                        request += "lang.name ILIKE '%" + languageSearch + "%' ";
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
                    CorrectDataGrid();
                    InfoTab.Items.Refresh();
                }
                else
                {
                    ClearBtn_Click(null, null);
                }
            }
            else if (viewStatus == 1) // читатели
            {
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
                    CorrectDataGrid();
                    InfoTab.Items.Refresh();
                }
                else
                {
                    ClearBtn_Click(null, null);
                }
            }
            else if (viewStatus == 2)  // долги
            {
                if (dateBorrow.HasValue || dateReturn.HasValue || searchLogin != ""
                    || searchIdReader != "" || searchIdLoan != "" || (!isWaiting && isReturned || isWaiting && !isReturned))
                {
                    request = "SELECT borrowed_books.id_borrowed_book, borrowed_books.login, borrowed_books.id_reader," +
                        " borrowed_books.borrow_date, borrowed_books.return_date, borrowed_books.is_returned," +
                        " borrowed_books.id_book, books.name FROM borrowed_books, books WHERE books.id_book = borrowed_books.id_book ";
                    if (dateBorrow.HasValue)
                    {
                        request = request + "AND borrow_date = '" + dateBorrow.Value + "' ";
                    }
                    if (dateReturn.HasValue)
                    {
                        request = request + "AND return_date = '" + dateReturn.Value + "' ";
                    }
                    if (searchLogin != "")
                    {
                        request = request + "AND login ILIKE '%" + searchLogin + "%' ";
                    }
                    if (searchIdReader != "")
                    {
                        request = request + "AND CAST(id_reader as varchar) ILIKE '%" + searchIdReader + "%' ";
                    }
                    if (searchIdLoan != "")
                    {
                        request = request + "AND CAST(id_borrowed_book as varchar) ILIKE '%" + searchIdLoan + "%' ";
                    }
                    if (!isWaiting && isReturned)
                    {
                        request = request + "AND is_returned = true ";
                    }
                    if (isWaiting && !isReturned)
                    {
                        request = request + "AND is_returned = false ";
                    }
                    if (titleSearch != "")
                    {
                        request = request + "AND books.name ILIKE '%" + titleSearch + "%' ";
                    }

                    DataTable db = new DataTable();
                    using (var connection = new NpgsqlConnection(connectionString))
                    {
                        connection.Open();
                        var command = new NpgsqlCommand(request, connection);
                        var reader = command.ExecuteReader();
                        db.Load(reader);
                    }
                    searchLoans = new List<BorrowedBook>();
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
                        searchLoans.Add(book);
                    }

                    InfoTab.ItemsSource = searchLoans;
                    CorrectDataGrid();
                    InfoTab.Items.Refresh();
                }
                else
                {
                    ClearBtn_Click(null, null);
                }
            }
        }

        private void InfoTab_Loaded(object sender, RoutedEventArgs e)
        {
            InfoTab.ItemsSource = books;
            CorrectDataGrid();
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

        private void ParamsContainer_Loaded(object sender, RoutedEventArgs e)
        {
            ParamsContainer.Content = FindResource("BookParams") as Grid;
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

        private void ClearBtn_Click(object sender, RoutedEventArgs e)
        {
            var grid = ParamsContainer.Content as Grid;
            foreach (var item in grid.Children)
            {
                if (item is TextBox)
                {
                    ((TextBox)item).Text = "";
                }
                else if (item is CheckBox)
                {
                    ((CheckBox)item).IsChecked = false;
                }
                else if (item is DatePicker)
                {
                    ((DatePicker)item).Text = "";
                }
            }
            if (viewStatus == 0)
            {
                searchBooks.Clear();
                InfoTab.ItemsSource = books;
            }
            else if (viewStatus == 1)
            {
                searchReader.Clear();
                InfoTab.ItemsSource = readers;

            }
            else
            {
                dateBorrow = null;
                dateReturn = null;
                searchLoans.Clear();
                InfoTab.ItemsSource = loans;
            }
            CorrectDataGrid();
            InfoTab.Items.Refresh();
        }

        private void CorrectDataGrid()
        {
            if (viewStatus == 0)
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
            else if (viewStatus == 1)
            {
                InfoTab.Columns[1].Header = "Фамилия";
                InfoTab.Columns[2].Header = "Имя";
                InfoTab.Columns[3].Header = "Отчество";
                InfoTab.Columns[4].Header = "Номер телефона";
            }
            else
            {
                InfoTab.Columns[0].Header = "Id долга";
                InfoTab.Columns[1].Visibility = Visibility.Hidden;
                InfoTab.Columns[2].Header = "Название книги";
                InfoTab.Columns[3].Header = "Id читателя";
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

        private void TitleStr_TextChanged(object sender, TextChangedEventArgs e)
        {
            var titleBox = sender as TextBox;
            titleSearch = titleBox.Text.Trim();
        }

        private void EditionStr_TextChanged(object sender, TextChangedEventArgs e)
        {
            var editionBox = sender as TextBox;
            editionSearch = editionBox.Text.Trim();
        }

        private void LanguageStr_TextChanged(object sender, TextChangedEventArgs e)
        {
            var langBox = sender as TextBox;
            languageSearch = langBox.Text.Trim();
        }

        private void SurnameStr_TextChanged(object sender, TextChangedEventArgs e)
        {
            var surnameBox = sender as TextBox;
            surnameSearch = surnameBox.Text.Trim();
        }

        private void NameStr_TextChanged(object sender, TextChangedEventArgs e)
        {
            var nameBox = sender as TextBox;
            nameSearch = nameBox.Text.Trim();
        }

        private void PatronymicStr_TextChanged(object sender, TextChangedEventArgs e)
        {
            var patronymicBox = sender as TextBox;
            patronymicSearch = patronymicBox.Text.Trim();
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

        private void LoanBtn_Click(object sender, RoutedEventArgs e)
        {
            if (viewStatus != 2)
            {
                ParamsContainer.Content = FindResource("LoanParams") as Grid;
                viewStatus = 2;

                InfoTab.ItemsSource = loans;
                CorrectDataGrid();


                AddContext.Visibility = Visibility.Collapsed;
                ChangeContext.Visibility = Visibility.Collapsed;
                ViewContext.Visibility = Visibility.Collapsed;
                ChangeStatus.Visibility = Visibility.Visible;
            }
        }

        private void BooksBtn_Click(object sender, RoutedEventArgs e)
        {
            if (viewStatus == 2)
            {
                AddContext.Visibility = Visibility.Visible;
                ChangeContext.Visibility = Visibility.Visible;
                ViewContext.Visibility = Visibility.Visible;
                ChangeStatus.Visibility = Visibility.Collapsed;
            }
            if (viewStatus != 0)
            {
                ParamsContainer.Content = FindResource("BookParams") as Grid;
                viewStatus = 0;

                InfoTab.ItemsSource = books;
                CorrectDataGrid();
            }
        }

        private void ReadersBtn_Click(object sender, RoutedEventArgs e)
        {
            if (viewStatus == 2)
            {
                AddContext.Visibility = Visibility.Visible;
                ChangeContext.Visibility = Visibility.Visible;
                ViewContext.Visibility = Visibility.Visible;
                ChangeStatus.Visibility = Visibility.Collapsed;
            }
            if (viewStatus != 1)
            {
                ParamsContainer.Content = FindResource("ReaderParams") as Grid;
                viewStatus = 1;

                InfoTab.ItemsSource = readers;
                CorrectDataGrid();
            }
        }

        // контексткное меню кнопка Добавить
        private void AddItem_Click(object sender, RoutedEventArgs e)
        {
            if (viewStatus == 0) // книги
            {
                AddBook newBook = new AddBook();
                newBook.Owner = this;
                newBook.ShowDialog();
                if (newBook.DialogResult == true)
                {
                    InfoTab.Items.Refresh();
                }
            }
            else if (viewStatus == 1) // чиатели
            {
                AddReader newReader = new AddReader();
                newReader.Owner = this;
                newReader.ShowDialog();
                if (newReader.DialogResult == true)
                {
                    InfoTab.Items.Refresh();
                }
            }
        }

        // контекстное меню кнопка Просмотреть
        private void CheckItem_Click(object sender, RoutedEventArgs e)
        {
            if (InfoTab.SelectedIndex != -1)
            {
                if (viewStatus == 0) // книги
                {
                    var select = (Book)InfoTab.SelectedItem;
                    BookInfo checkBook = new BookInfo(select, loginUser);  // Вставить логин пользователя
                    checkBook.Owner = this;
                    checkBook.ShowDialog();
                }
                else if (viewStatus == 1) // читатели
                {
                    var select = (Reader)InfoTab.SelectedItem;
                    ReaderProfile checkReader = new ReaderProfile(select, loginUser);
                    checkReader.Owner = this;
                    checkReader.ShowDialog();
                }
            }
        }

        // контекстное меню кнопка Удалить
        private void DeleteItem_Click(object sender, RoutedEventArgs e)
        {
            if (InfoTab.SelectedIndex != -1)
            {
                if (viewStatus == 0) // книги
                {
                    var select = (Book)InfoTab.SelectedItem;
                    using (var connection = new NpgsqlConnection(connectionString))
                    {
                        connection.Open();
                        var command = new NpgsqlCommand("DELETE FROM books WHERE id_book = " + select.Id, connection);
                        command.ExecuteNonQuery();
                    }
                    books.Remove(select);
                    foreach (var loan in loans.Where(book => book.IdBook == select.Id).ToArray())
                    {
                        loans.Remove(loan);
                    }
                    InfoTab.Items.Refresh();
                }
                else if (viewStatus == 1) // читатели
                {
                    var select = (Reader)InfoTab.SelectedItem;
                    using (var connection = new NpgsqlConnection(connectionString))
                    {
                        connection.Open();
                        var command = new NpgsqlCommand("DELETE FROM readers WHERE id_reader = " + select.Id, connection);
                        command.ExecuteNonQuery();
                    }
                    readers.Remove(select);
                    foreach (var loan in loans.Where(book => book.IdReader == select.Id).ToArray())
                    {
                        loans.Remove(loan);
                    }
                    InfoTab.Items.Refresh();
                }
                else  // долги
                {
                    var select = (BorrowedBook)InfoTab.SelectedItem;
                    using (var connection = new NpgsqlConnection(connectionString))
                    {
                        connection.Open();
                        var command = new NpgsqlCommand("DELETE FROM borrowed_books WHERE id_borrowed_book = " + select.Id, connection);
                        command.ExecuteNonQuery();
                    }
                    loans.Remove(select);
                    InfoTab.Items.Refresh();
                }
            }
        }

        // контекстное меню кнопка Изменить
        private void ChangeItem_Click(object sender, RoutedEventArgs e)
        {
            if (InfoTab.SelectedIndex != -1)
            {
                if (viewStatus == 0) // книги
                {
                    AddBook changeBook = new AddBook((Book)InfoTab.SelectedItem);
                    changeBook.Owner = this;
                    changeBook.ShowDialog();
                    if (changeBook.DialogResult == true)
                    {
                        InfoTab.Items.Refresh();
                    }
                }
                else if (viewStatus == 1) // читатели
                {
                    AddReader newReader = new AddReader((Reader)InfoTab.SelectedItem);
                    newReader.Owner = this;
                    newReader.ShowDialog();
                    if (newReader.DialogResult == true)
                    {
                        InfoTab.Items.Refresh();
                    }
                }
            }
        }

        private void IdLoan_TextChanged(object sender, TextChangedEventArgs e)
        {
            var idBox = sender as TextBox;
            uint testId;
            if (idBox.Text != "" && !uint.TryParse(idBox.Text, out testId))
            {
                idBox.Text = searchIdLoan;
                idBox.Select(idBox.Text.Length, 0);
            }
            else
            {
                searchIdLoan = idBox.Text;
            }
        }

        private void IdForReader_TextChanged(object sender, TextChangedEventArgs e)
        {
            var idBox = sender as TextBox;
            uint testId;
            if (idBox.Text != "" && !uint.TryParse(idBox.Text, out testId))
            {
                idBox.Text = searchIdReader;
                idBox.Select(idBox.Text.Length, 0);
            }
            else
            {
                searchIdReader = idBox.Text;
            }
        }

        private void ChangeBookData_Click(object sender, RoutedEventArgs e)
        {
            ParamsTab changeTab = new ParamsTab();
            changeTab.Owner = this;
            changeTab.ShowDialog();
            if (changeTab.DialogResult == true)
            {
                LoadBooks();
                if (viewStatus == 0)
                {
                    InfoTab.Items.Refresh();
                }
            }
        }

        private void CheckBoxReturn_Checked(object sender, RoutedEventArgs e)
        {
            var checkbox = sender as CheckBox;
            if (checkbox.IsChecked == true)
            {
                isReturned = true;
            }
            else
            {
                isReturned = false;
            }
        }

        private void CheckBoxWait_Checked(object sender, RoutedEventArgs e)
        {
            var checkbox = sender as CheckBox;
            if (checkbox.IsChecked == true)
            {
                isWaiting = true;
            }
            else
            {
                isWaiting = false;
            }
        }

        private void LoginLoan_TextChanged(object sender, TextChangedEventArgs e)
        {
            var loginBox = sender as TextBox;
            searchLogin = loginBox.Text.Trim();
        }

        private void CalendarBorrow_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            var calendarBox = sender as DatePicker;
            DateTime? date = calendarBox.SelectedDate;
            if (date.HasValue)
            {
                dateBorrow = date.Value;
            }
        }

        private void CalendarReturn_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            var calendarBox = sender as DatePicker;
            DateTime? date = calendarBox.SelectedDate;
            if (date.HasValue)
            {
                dateReturn = date.Value;
            }
        }

        private void ChangeStatus_Click(object sender, RoutedEventArgs e)
        {
            BorrowedBook selectedLoan = InfoTab.SelectedItem as BorrowedBook;
            Book selectedBook = books.Where(book => book.Id == selectedLoan.IdBook).First();
            bool borrowStatus = false;
            bool wasEdited = false;
            if (selectedLoan.isReturned && selectedBook.Number > 0)
            {
                selectedBook.Number -= 1;
                selectedLoan.isReturned = false;
                wasEdited = true;
            }
            else if (!selectedLoan.isReturned)
            {
                borrowStatus = true;
                selectedBook.Number += 1;
                selectedLoan.isReturned = true;
                wasEdited = true;
            }
            if (wasEdited)
            {
                InfoTab.Items.Refresh();
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    var command = new NpgsqlCommand("UPDATE borrowed_books SET is_returned = " + borrowStatus +
                        " WHERE id_borrowed_book = " + selectedLoan.Id, connection);
                    command.ExecuteNonQuery();
                    var command2 = new NpgsqlCommand("UPDATE books SET number_of_books = "
                        + selectedBook.Number + " WHERE id_book = " + selectedLoan.IdBook, connection);
                    command2.ExecuteNonQuery();
                }
            }
        }

        private void ExportBtn_Click(object sender, RoutedEventArgs e)
        {
            string path = Environment.CurrentDirectory + @"\books.csv";
            using (var writer = new StreamWriter(path, false, Encoding.GetEncoding("UTF-16")))
            {
                var csvConfig = new CsvConfiguration(CultureInfo.GetCultureInfo("ru-RU"))
                {
                    Delimiter = ";"
                };
                using (var csvWriter = new CsvWriter(writer, csvConfig))
                {
                    csvWriter.WriteRecords(books);
                }
            }
            path = Environment.CurrentDirectory + @"\loans.csv";
            using (var writer = new StreamWriter(path, false, Encoding.GetEncoding("UTF-16")))
            {
                var csvConfig = new CsvConfiguration(CultureInfo.GetCultureInfo("ru-RU"))
                {
                    Delimiter = ";"
                };
                using (var csvWriter = new CsvWriter(writer, csvConfig))
                {
                    csvWriter.WriteRecords(loans);
                }
            }
        }
    }
}
