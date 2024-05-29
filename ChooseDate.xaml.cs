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
    public partial class ChooseDate : Window
    {
        public DateTime selectedStart, selectedEnd;
        public ChooseDate()
        {
            InitializeComponent();
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void AcceptBtn_Click(object sender, RoutedEventArgs e)
        {
            DateTime? start = DateBorrowStr.SelectedDate;
            DateTime? end = DateReturnStr.SelectedDate;
            if (start.HasValue && end.HasValue)
            {
                selectedStart = (DateTime)start;
                selectedEnd = (DateTime)end;
                if (selectedStart > selectedEnd)
                {
                    ErrorMessage.Text = "Дата выдачи позже даты возврата";
                }
                else
                {
                    DialogResult = true;
                    Close();
                }
            }
            else
            {
                ErrorMessage.Text = "Неккоректный ввод даты";
            }
        }
    }
}
