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

namespace MyDrive.Controls
{
    /// <summary>
    /// Lógica de interacción para ColorPicker.xaml
    /// </summary>
    public partial class ColorPicker : Window
    {
        public Color? SelectedColor { get; private set; }

        public ColorPicker()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
