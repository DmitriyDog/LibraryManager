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
using Npgsql;

namespace Library
{
    public partial class ParamsTab : Window
    {
        List<Author> authorList = new List<Author>();
        List<Language> languageList = new List<Language>();
        List<Edition> editionList = new List<Edition>();
        internal string connectionString = LoginWindow.connectionString;
        private int tabStatus = 0;
        public int id = 0;
        public string returnString = "";
        private string getSurname = "";
        private string getName = "";
        private string getPatronymic = "";
        private string getEditionName = "";
        private string getLangName = "";

        private int visibilitystatus = 0;
        private bool workResult = false;
        public ParamsTab()
        {
            InitializeComponent();
            LoadAuthors();
            LoadEditions();
            LoadLangs();
            
            ParamsContainer.Children.Add(FindResource("ShowGridAuthor") as Grid);
        }

        public ParamsTab(int tabStatus)
        {
            InitializeComponent();
            this.tabStatus = tabStatus;
            visibilitystatus = 1;
            if (tabStatus == 0)
            {
                ParamsContainer.Children.Add(FindResource("ShowGridAuthor") as Grid);
                LoadAuthors();
            }
            else
            {
                if (tabStatus == 1)
                {
                    LoadEditions();
                }
                else
                {
                    LoadLangs();
                }
                ParamsContainer.Children.Add(FindResource("ShowGridOthers") as Grid);
            }
        }

        private void LoadLangs()
        {
            DataTable db = new DataTable();

            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                var command = new NpgsqlCommand("SELECT * FROM lang", connection);
                NpgsqlDataReader reader = command.ExecuteReader();
                db.Load(reader);
                connection.Close();
            }
            foreach (DataRow row in db.Rows)
            {
                Language lang = new Language();
                lang.Id = Convert.ToInt32(row.ItemArray[0]);
                lang.Name = row.ItemArray[1].ToString();
                languageList.Add(lang);
            }
        }

        private void LoadEditions()
        {
            DataTable db = new DataTable();

            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                var command = new NpgsqlCommand("SELECT * FROM edition", connection);
                NpgsqlDataReader reader = command.ExecuteReader();
                db.Load(reader);
                connection.Close();
            }
            foreach (DataRow row in db.Rows)
            {
                Edition edition = new Edition();
                edition.Id = Convert.ToInt32(row.ItemArray[0]);
                edition.Name = row.ItemArray[1].ToString();
                editionList.Add(edition);
            }
        }

        private void LoadAuthors()
        {
            DataTable db = new DataTable();
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                var command = new NpgsqlCommand("SELECT * FROM author", connection);
                NpgsqlDataReader reader = command.ExecuteReader();
                db.Load(reader);
                connection.Close();
            }
            foreach (DataRow row in db.Rows)
            {
                Author author = new Author();
                author.Id = Convert.ToInt32(row.ItemArray[0]);
                author.Surname = row.ItemArray[1].ToString();
                author.Name = row.ItemArray[2].ToString();
                author.Patronymic = row.ItemArray[3].ToString();
                authorList.Add(author);
            }
        }

        private void CorrectInfoTab()
        {
            InfoTab.Columns[0].Visibility = Visibility.Hidden;
            if (tabStatus == 0)
            {
                InfoTab.Columns[1].Header = "Фамилия";
                InfoTab.Columns[2].Header = "Имя";
                InfoTab.Columns[3].Header = "Отчество";
            }
            else if (tabStatus == 1)
            {
                InfoTab.Columns[1].Header = "Название издания";
            }
            else if (tabStatus == 2)
            {
                InfoTab.Columns[1].Header = "Название языка";
            }
            foreach (var column in InfoTab.Columns)
            {
                column.MinWidth = column.ActualWidth;
                column.Width = new DataGridLength(1, DataGridLengthUnitType.Star);
            }
            foreach (var item in ((Grid)ParamsContainer.Children[0]).Children)
            {
                if (item is TextBox)
                {
                    TextBox box = item as TextBox;
                    box.Text = "";
                }
            }
            InfoTab.Items.Refresh();
        }

        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            if (tabStatus == 0)
            {
                if (getSurname.Trim() != "" && getName.Trim() != "")
                {
                    using (var connection = new NpgsqlConnection(connectionString))
                    {
                        connection.Open();
                        var command = new NpgsqlCommand("INSERT INTO author (surname, name, patronymic) VALUES (($1), ($2), ($3)) RETURNING id_author", connection)
                        {
                            Parameters =
                            {
                                new() { Value= getSurname.Trim()},
                                new() { Value= getName.Trim()},
                                new() { Value= getPatronymic.Trim()}
                            }
                        };
                        id = (int)command.ExecuteScalar();
                        connection.Close();
                    }
                    Author toAdd = new Author();
                    toAdd.Name = getName;
                    toAdd.Surname = getSurname;
                    toAdd.Patronymic = getPatronymic;
                    toAdd.Id = id;
                    authorList.Add(toAdd);
                }
            }
            else if (tabStatus == 1)
            {
                if (getEditionName.Trim() != "")
                {
                    using (var connection = new NpgsqlConnection(connectionString))
                    {
                        connection.Open();
                        var command = new NpgsqlCommand("INSERT INTO edition (name) VALUES ('" + getEditionName.Trim() + "')  RETURNING id_edition", connection);
                        id = (int)command.ExecuteScalar();
                        connection.Close();
                    }
                    Edition toAdd = new Edition();
                    toAdd.Name = getEditionName.Trim();
                    toAdd.Id = id;
                    editionList.Add(toAdd);
                }
            }
            else
            {
                if (getLangName.Trim() != "")
                {
                    using (var connection = new NpgsqlConnection(connectionString))
                    {
                        connection.Open();
                        var command = new NpgsqlCommand("INSERT INTO lang (name) VALUES ('" + getLangName.Trim() + "')  RETURNING id_lang", connection);
                        id = (int)command.ExecuteScalar();
                        connection.Close();
                    }
                    Language toAdd = new Language();
                    toAdd.Name = getLangName.Trim();
                    toAdd.Id = id;
                    languageList.Add(toAdd);
                }
            }
            InfoTab.Items.Refresh();
            InfoTab.SelectedIndex = -1;
        }

        private void ChangeItem_Click(object sender, RoutedEventArgs e)
        {
            if (id != 0)
            {
                if (tabStatus == 0)
                {
                    if (getSurname.Trim() != "" && getName.Trim() != "")
                    {
                        using (var connection = new NpgsqlConnection(connectionString))
                        {
                            connection.Open();
                            var command = new NpgsqlCommand("UPDATE author SET surname = ($1), name = ($2), patronymic = ($3) WHERE id_author = ($4)", connection)
                            {
                                Parameters =
                            {
                                new() { Value= getSurname.Trim()},
                                new() { Value= getName.Trim()},
                                new() { Value= getPatronymic.Trim()},
                                new() { Value= id}
                            }
                            };
                            command.ExecuteNonQuery();
                            connection.Close();
                        }
                        Author toChange = (Author)InfoTab.Items[InfoTab.SelectedIndex];
                        toChange.Name = getName.Trim();
                        toChange.Surname = getSurname.Trim();
                        toChange.Patronymic = getPatronymic.Trim();
                    }
                }
                else if (tabStatus == 1)
                {
                    if (getEditionName.Trim() != "")
                    {
                        using (var connection = new NpgsqlConnection(connectionString))
                        {
                            connection.Open();
                            var command = new NpgsqlCommand("UPDATE edition SET name = ($1) WHERE id_edition = ($2)", connection)
                            {
                                Parameters =
                            {
                                new() { Value= getEditionName.Trim()},
                                new() { Value= id}
                            }
                            };
                            command.ExecuteNonQuery();
                            connection.Close();
                        }
                        Edition toChange = (Edition)InfoTab.Items[InfoTab.SelectedIndex];
                        toChange.Name = getEditionName.Trim();
                    }
                }
                else
                {
                    if (getLangName.Trim() != "")
                    {
                        using (var connection = new NpgsqlConnection(connectionString))
                        {
                            connection.Open();
                            var command = new NpgsqlCommand("UPDATE lang SET name = ($1) WHERE id_lang = ($2)", connection)
                            {
                                Parameters =
                            {
                                new() { Value= getLangName.Trim()},
                                new() { Value= id}
                            }
                            };
                            command.ExecuteNonQuery();
                            connection.Close();
                        }
                        Language toChange = (Language)InfoTab.Items[InfoTab.SelectedIndex];
                        toChange.Name = getLangName.Trim();
                    }
                }
                workResult = true;
                InfoTab.Items.Refresh();
            }
        }

        private void DeleteItem_Click(object sender, RoutedEventArgs e)
        {
            if (id != 0)
            {
                if (tabStatus == 0)
                {
                    using (var connection = new NpgsqlConnection(connectionString))
                    {
                        connection.Open();
                        var command = new NpgsqlCommand("DELETE FROM author WHERE id_author = " + id, connection);
                        command.ExecuteNonQuery();
                        connection.Close();
                    }
                    Author toDelete = (Author)InfoTab.Items[InfoTab.SelectedIndex];
                    authorList.Remove(toDelete);
                }
                else if (tabStatus == 1)
                {
                    using (var connection = new NpgsqlConnection(connectionString))
                    {
                        connection.Open();
                        var command = new NpgsqlCommand("DELETE FROM edition WHERE id_edition = " + id, connection);
                        command.ExecuteNonQuery();
                        connection.Close();
                    }
                    Edition toDelete = (Edition)InfoTab.Items[InfoTab.SelectedIndex];
                    editionList.Remove(toDelete);
                }
                else
                {
                    using (var connection = new NpgsqlConnection(connectionString))
                    {
                        connection.Open();
                        var command = new NpgsqlCommand("DELETE FROM lang WHERE id_lang = " + id, connection);
                        command.ExecuteNonQuery();
                        connection.Close();
                    }
                    Language toDelete = (Language)InfoTab.Items[InfoTab.SelectedIndex];
                    languageList.Remove(toDelete);
                }
                InfoTab.Items.Refresh();
            }
        }

        private void LangsBtn_Click(object sender, RoutedEventArgs e)
        {
            ParamsContainer.Children.Clear();
            ParamsContainer.Children.Add(FindResource("ShowGridOthers") as Grid);
            InfoTab.ItemsSource = languageList;
            InfoTab.SelectedIndex = -1;
            tabStatus = 2;
            CorrectInfoTab();

            getSurname = "";
            getPatronymic = "";
            getName = "";
            getEditionName = "";
        }

        private void EditionsBtn_Click(object sender, RoutedEventArgs e)
        {
            ParamsContainer.Children.Clear();
            ParamsContainer.Children.Add(FindResource("ShowGridOthers") as Grid);
            InfoTab.ItemsSource = editionList;
            InfoTab.SelectedIndex = -1;
            tabStatus = 1;
            CorrectInfoTab();

            getSurname = "";
            getPatronymic = "";
            getName = "";
            getLangName = "";
        }

        private void AuthorsBtn_Click(object sender, RoutedEventArgs e)
        {

            ParamsContainer.Children.Clear();
            ParamsContainer.Children.Add(FindResource("ShowGridAuthor") as Grid);
            InfoTab.ItemsSource = authorList;
            InfoTab.SelectedIndex = -1;
            tabStatus = 0;
            CorrectInfoTab();

            getLangName = "";
            getEditionName = "";
        }

        private void ChooseBtn_Click(object sender, RoutedEventArgs e)
        {
            if (InfoTab.SelectedIndex != -1)
            {
                DialogResult = true;
                if (tabStatus == 0)
                {
                    returnString = ((Author)InfoTab.Items[InfoTab.SelectedIndex]).Surname;
                }
                else if (tabStatus == 1)
                {
                    returnString = ((Edition)InfoTab.Items[InfoTab.SelectedIndex]).Name;
                }
                else
                {
                    returnString = ((Language)InfoTab.Items[InfoTab.SelectedIndex]).Name;
                }
                Close();
            }
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private class Language
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        private class Edition
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        private class Author
        {
            public int Id { get; set; }
            public string Surname { get; set; }
            public string Name { get; set; }
            public string Patronymic { get; set; }
        }

        private void InfoTab_Loaded(object sender, RoutedEventArgs e)
        {
            if (tabStatus == 0)
            {
                InfoTab.ItemsSource = authorList;
            }
            else if (tabStatus == 1)
            {
                InfoTab.ItemsSource = editionList;
            }
            else
            {
                InfoTab.ItemsSource = languageList;
            }
            CorrectInfoTab();

            if (visibilitystatus == 0)
            {
                ChooseGrid.Visibility = Visibility.Hidden;
            }
            else
            {
                PagesGrid.Visibility = Visibility.Hidden;
            }
        }

        private void InfoTab_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var table = sender as DataGrid;
            if (table.SelectedIndex != -1)
            {
                if (tabStatus == 0)
                {
                    id = ((Author)table.Items[table.SelectedIndex]).Id;
                }
                else if (tabStatus == 1)
                {
                    id = ((Edition)table.Items[table.SelectedIndex]).Id;
                }
                else
                {
                    id = ((Language)table.Items[table.SelectedIndex]).Id;
                }
                int count = 1;
                foreach (var item in ((Grid)ParamsContainer.Children[0]).Children)
                {
                    if (item is TextBox)
                    {
                        TextBox box = item as TextBox;
                        var block = table.Columns[count++].GetCellContent(table.Items[table.SelectedIndex]) as TextBlock;
                        box.Text = block.Text;
                    }
                }
            }
            else
            {
                id = 0;
            }
        }

        private void SurnameStr_TextChanged(object sender, TextChangedEventArgs e)
        {
            var surnameBox = sender as TextBox;
            getSurname = surnameBox.Text;
        }

        private void PatronymicStr_TextChanged(object sender, TextChangedEventArgs e)
        {
            var patronymicBox = sender as TextBox;
            getPatronymic = patronymicBox.Text;
        }

        private void NameForParam_TextChanged(object sender, TextChangedEventArgs e)
        {
            var nameBox = sender as TextBox;
            if (tabStatus == 0)
            {
                getName = nameBox.Text;
            }
            else if (tabStatus == 1)
            {
                getEditionName = nameBox.Text;
            }
            else
            {
                getLangName = nameBox.Text;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (workResult)
            {
                DialogResult = true;
            }
        }
    }
}
