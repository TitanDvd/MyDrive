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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MyDrive.Controls
{
    /// <summary>
    /// Lógica de interacción para DeviceControlUI.xaml
    /// </summary>
    public partial class DeviceControlUI : UserControl
    {
        public delegate void DeleteItem(DeviceControlUI control);
        public event DeleteItem OnDeleteItemClick;
        public Base.Types.EasyAccessItem EasyAccessItem { get; set; }

        public DeviceControlUI(Base.Types.EasyAccessItem eai)
        {
            InitializeComponent();

            _tagName.Content = eai.Tag;
            _easyAccess.Content = eai.RootPath;
            EasyAccessItem = eai;
        }

        private void _deleteItem_Click(object sender, RoutedEventArgs e) => OnDeleteItemClick?.Invoke(this);
    }
}
