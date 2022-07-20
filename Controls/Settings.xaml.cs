using Microsoft.Win32;
using MyDrive.Base;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reflection;
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
using static MyDrive.Base.AppSettings;

namespace MyDrive.Controls
{
    /// <summary>
    /// Lógica de interacción para Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        Grid _devicesContainer;

        public delegate void VisualStyleChange(ControlSettings settings);
        public event VisualStyleChange OnVisualChange;



        public Settings()
        {
            InitializeComponent();
            Options = new ApplicationSettings();
            _devicesContainer = new Grid();

            Loaded += Settings_Loaded;
        }

        private void Settings_Loaded(object sender, RoutedEventArgs e)
        {
            // General Settings
            _showOnClick.IsChecked = Options.Generals.ShowOnTopClick;
            _showOnTopMouseSlide.IsChecked = Options.Generals.ShowOnTopMouseSlide;
            _startAtBoot.IsChecked = Options.Generals.StartAtOsBoot;
            _updateAtShown.IsChecked = Options.Generals.UpdateAtShown;

            // Easy Access Settings
            LoadEasyAccessItems();

            // Load Colors
            _fontColor.Background = new SolidColorBrush(Options.Visual.FontColor);
            _fullSpaceColor.Background = new SolidColorBrush(Options.Visual.FullSpaceBarColor);
            _enoughtSpaceColor.Background = new SolidColorBrush(Options.Visual.GoodSpaceBarColor);
            _warningSpaceColor.Background = new SolidColorBrush(Options.Visual.WarningSpaceBarColor);
            _tranlucentWindowColor.Background = new SolidColorBrush(Options.Visual.TraslucentColor);

        }


        private void LoadEasyAccessItems()
        {
            _easyAccessControlsContainer.Children.Remove(_devicesContainer);
            int i = 0;
            foreach (var eai in Options.EasyAccessItems.AccessItems)
            {
                var deviceUI = new DeviceControlUI(eai);
                deviceUI.OnDeleteItemClick += (e) =>
                {
                    MainWindow.mHook.UnHook();
                    _devicesContainer.Children.Remove(e);
                    Options.EasyAccessItems.AccessItems.Remove(e.EasyAccessItem);
                    Options.SaveSettings(Options.EasyAccessItems);
                };
                _devicesContainer.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                _devicesContainer.Children.Add(deviceUI);
                Grid.SetRow(deviceUI, i++);

            }

            Grid.SetColumn(_devicesContainer, 1);
            _easyAccessControlsContainer.Children.Add(_devicesContainer);
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Options.Generals.StartAtOsBoot = ((CheckBox)sender).IsChecked.Value;
        }

        
        private void _showOnClick_Checked(object sender, RoutedEventArgs e)
        {
            Options.Generals.ShowOnTopClick = ((CheckBox)sender).IsChecked.Value;
        }

        private void _showOnTopMouseSlide_Checked(object sender, RoutedEventArgs e)
        {
            Options.Generals.ShowOnTopMouseSlide = ((CheckBox)sender).IsChecked.Value;
        }

        private void _updateAtShown_Checked(object sender, RoutedEventArgs e)
        {
            Options.Generals.UpdateAtShown = ((CheckBox)sender).IsChecked.Value;
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Options.SaveSettings(Options.Generals);
            MessageBox.Show(this, "La configuracion se ha guardado correctamente", "MyDrive", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            // Guardar la etiqueta y su ruta
            Base.Types.EasyAccessItem item = new Base.Types.EasyAccessItem
            {
                RootPath = cfg_Path.Text,
                Tag = cfg_tagName.Text
            };

            Options.EasyAccessItems.AccessItems.Add(item);
            Options.SaveSettings(Options.EasyAccessItems);
            LoadEasyAccessItems();

            cfg_Path.Text = "";
            cfg_tagName.Text = "";

            OnVisualChange?.Invoke(Options.EasyAccessItems);
         }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog fd = new System.Windows.Forms.FolderBrowserDialog();
            fd.ShowDialog();

            if(!string.IsNullOrEmpty(fd.SelectedPath))
                cfg_Path.Text = fd.SelectedPath;
        }



        private void btn_changeEnoughtSpaceColor_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.ColorDialog colorD = new System.Windows.Forms.ColorDialog();
            colorD.ShowDialog();

            Color color = new Color
            {
                A = colorD.Color.A,
                B = colorD.Color.B,
                G = colorD.Color.G,
                R = colorD.Color.R
            };

            //ColorPickerWPF.ColorPickerControl cp = new ColorPickerWPF.ColorPickerControl();
            //ColorPickerWPF.ColorPickerWindow.ShowDialog(out color, ColorPickerWPF.Code.ColorPickerDialogOptions.SimpleView);
            Options.Visual.GoodSpaceBarColor = color;
           _enoughtSpaceColor.Background = new SolidColorBrush(color);
        }



        private void _change_warningSpaceColor_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.ColorDialog colorD = new System.Windows.Forms.ColorDialog();
            colorD.ShowDialog();

            Color color = new Color
            {
                A = colorD.Color.A,
                B = colorD.Color.B,
                G = colorD.Color.G,
                R = colorD.Color.R
            };
            //ColorPickerWPF.ColorPickerControl cp = new ColorPickerWPF.ColorPickerControl();
            //ColorPickerWPF.ColorPickerWindow.ShowDialog(out color, ColorPickerWPF.Code.ColorPickerDialogOptions.SimpleView);
            Options.Visual.WarningSpaceBarColor = color;
            _warningSpaceColor.Background = new SolidColorBrush(color);
        }

        private void _change_fullSpaceColor_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.ColorDialog colorD = new System.Windows.Forms.ColorDialog();
            colorD.ShowDialog();

            Color color = new Color
            {
                A = colorD.Color.A,
                B = colorD.Color.B,
                G = colorD.Color.G,
                R = colorD.Color.R
            };
            //ColorPickerWPF.ColorPickerControl cp = new ColorPickerWPF.ColorPickerControl();
            //ColorPickerWPF.ColorPickerWindow.ShowDialog(out color, ColorPickerWPF.Code.ColorPickerDialogOptions.SimpleView);
            Options.Visual.FullSpaceBarColor = color;
            _fullSpaceColor.Background = new SolidColorBrush(color);
        }

        private void _change_tranlucentWindowColor_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.ColorDialog colorD = new System.Windows.Forms.ColorDialog();
            colorD.ShowDialog();

            Color color = new Color
            {
                A = 50,
                B = colorD.Color.B,
                G = colorD.Color.G,
                R = colorD.Color.R
            };

            Options.Visual.TraslucentColor = color;
            // Restore alpha in order to be shown properly
            color.A = 255;
            _tranlucentWindowColor.Background = new SolidColorBrush(color);
        }

        private void _change_fontColor_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.ColorDialog colorD = new System.Windows.Forms.ColorDialog();
            colorD.ShowDialog();

            Color color = new Color
            {
                A = colorD.Color.A,
                B = colorD.Color.B,
                G = colorD.Color.G,
                R = colorD.Color.R
            };
            //ColorPickerWPF.ColorPickerControl cp = new ColorPickerWPF.ColorPickerControl();
            //ColorPickerWPF.ColorPickerWindow.ShowDialog(out color, ColorPickerWPF.Code.ColorPickerDialogOptions.SimpleView);
            Options.Visual.FontColor = color;
            _fontColor.Background = new SolidColorBrush(color);
        }


        private void btn_saveVisualSettings_Click(object sender, RoutedEventArgs e)
        {
            Options.SaveSettings(Options.Visual);
            MessageBox.Show(this, "La configuracion se ha guardado correctamente", "MyDrive", MessageBoxButton.OK, MessageBoxImage.Information);
            OnVisualChange?.Invoke(Options.Visual);
        }
    }
}
