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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Library
{
    public partial class MainWindow : Window
    {
        string connectionString = "Host=localhost;Username=postgres;Password=password;Database=Библиотека";
        NpgsqlConnection vCon;
        NpgsqlCommand vCmd;
        List<Book> bookList;

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void UpdBtn_Click(object sender, RoutedEventArgs e)
        {
            await using var vCon = new NpgsqlConnection(connectionString);
            await vCon.OpenAsync();

            await using var vCmd = new NpgsqlCommand("SELECT * FROM books", vCon);
            await using var reader = await vCmd.ExecuteReaderAsync();
            DataTable db = new DataTable();
            db.Load(reader);
            await vCon.CloseAsync();
            bookList = new List<Book>();
            foreach (DataRow row in db.Rows)
            {
                //Book book = new Book();
                //book.Id = Convert.ToInt16(row.ItemArray[0]);
                //book.Name = row.ItemArray[1].ToString();
                //book.Author = row.ItemArray[2].ToString();
                //book.Edition = row.ItemArray[3].ToString();
                //book.Year = Convert.ToInt16(row.ItemArray[4]);
                //book.NumberOfBooks = Convert.ToInt16(row.ItemArray[6]);
                //bookList.Add(book);
            }
            DBTable.ItemsSource = bookList;
            DBTable.Columns[0].Header = "Id";
            DBTable.Columns[1].Header = "Название";
            DBTable.Columns[2].Header = "Автор";
            DBTable.Columns[3].Header = "Издательство";
            DBTable.Columns[4].Header = "Год издания";
            DBTable.Columns[5].Header = "Количество книг";
        }

        private async void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            await using var vCon = new NpgsqlConnection(connectionString);
            await vCon.OpenAsync();
            string name = TextName.Text.Trim();
            string author = TextAuthor.Text.Trim();
            string edition = TextEdition.Text.Trim();
            int year;
            int quantity;
            if (int.TryParse(TextYear.Text, out year) && int.TryParse(TextNumber.Text, out quantity)
                && name != "" && quantity >= 0 && year >= 0 && TextName.Text != "Неверный ввод данных!")
            {
                await using var vCmd = new NpgsqlCommand("INSERT INTO books (name, author, edition, year, number_of_books) VALUES ($1, $2, $3, $4, $5)", vCon)
                {
                    Parameters =
                    {
                        new() { Value=name },
                        new() { Value=author},
                        new() { Value=edition},
                        new() { Value=year},
                        new() {Value=quantity}
                    }
                };
                await vCmd.ExecuteNonQueryAsync();
                TextEdition.Text = "";
                TextAuthor.Text = "";
                TextName.Text = "";
                TextYear.Text = "";
                TextNumber.Text = "";
            }
            else
            {
                TextName.Text = "Неверный ввод данных!";
            }
        }

        private async void DelBtn_Click(object sender, RoutedEventArgs e)
        {
            int index = DBTable.SelectedIndex;
            if (index != -1)
            {
                index = bookList[index].Id;
                await using var vCon = new NpgsqlConnection(connectionString);
                await vCon.OpenAsync();

                await using var vCmd = new NpgsqlCommand("DELETE FROM books WHERE id_book=$1", vCon)
                {
                    Parameters =
                {
                    new() {Value=index}
                }
                };
                await vCmd.ExecuteNonQueryAsync();
            }
        }
    }

    //public class Book
    //{
    //    public int Id { get; set; }
    //    public string Name { get; set; }
    //    public string Author { get; set; }
    //    public string Edition { get; set; }
    //    public int Year { get; set; }
    //    public int NumberOfBooks { get; set; }
    //}
}
