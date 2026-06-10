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

namespace PhoneBookWpf
{
    public partial class EditContactWindow : Window
    {
        public string ContactName { get; private set; } = string.Empty;
        public string ContactPhone { get; private set; } = string.Empty;

        public EditContactWindow(string currentName, string currentPhone)
        {
            InitializeComponent();
            NameBox.Text = currentName;
            PhoneBox.Text = currentPhone;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameBox.Text) ||
                string.IsNullOrWhiteSpace(PhoneBox.Text))
            {
                MessageBox.Show("Заполните все поля!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ContactName = NameBox.Text.Trim();
            ContactPhone = PhoneBox.Text.Trim();
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}