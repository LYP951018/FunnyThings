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

namespace RegexChart
{
    /// <summary>
    /// Interaction logic for RegexInput.xaml
    /// </summary>
    public partial class RegexInput : Window
    {
        public RegexInput()
        {
            InitializeComponent();
            RegexYesButton.Click += RegexYesButton_Click;
            RegexCancelButton.Click += RegexCancelButton_Click;
        }

        private void RegexCancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void RegexYesButton_Click(object sender, RoutedEventArgs e)
        {
            RegexString = RegexBox.Text;
            Close();
        }

        public string RegexString { get; private set; } = string.Empty;
    }
}
