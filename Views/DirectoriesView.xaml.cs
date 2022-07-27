using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MyDrive.Base;
using static NetDrive.Base.Win32.User32;

namespace MyDrive.Views
{
    /// <summary>
    /// Lógica de interacción para DirectoriesView.xaml
    /// </summary>
    public partial class DirectoriesView : Window
    {
        private DoubleAnimation _dblanim;
        private bool _shown;
        private bool _isCtrlPressed;
        List<string> navigationHistory;


        public DirectoriesView()
        {
            _dblanim = new DoubleAnimation();
            navigationHistory = new List<string>();
            InitializeComponent();

            Opacity = 0;
        }



        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);

            if (_shown)
                return;

            _shown = true;


            _dblanim.From = 0;
            _dblanim.To = 1;
            _dblanim.Duration = TimeSpan.FromMilliseconds(300);
            BeginAnimation(OpacityProperty, _dblanim);
        }



        public void LoadFSEntriesUI(string _path)
        {
            navigationHistory.Add(_path);
            addressBar.Text = _path;
            ListContainer.Children.Clear();
            var fUtil = new Base.FolderUtils();
            Dispatcher.BeginInvoke(new Action(async () =>
            {
                try
                {
                    string[] entries = Directory.GetFileSystemEntries(_path);
                    var dirs = entries.Where(s => Directory.Exists(s));
                    var files = entries.Where(s => File.Exists(s));
                    List<string> fsentries = new List<string>();
                    fsentries.AddRange(dirs);
                    fsentries.AddRange(files);


                    foreach (var fs in fsentries)
                    {
                        Grid fsUiCont = new Grid();
                        Image iconImage = new Image();
                        TextBlock fsCapt = new TextBlock
                        {
                            FontSize = 11,
                            Text = fs.Split('\\').Last(),
                            HorizontalAlignment = HorizontalAlignment.Center,
                            Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                            TextWrapping = TextWrapping.Wrap,
                            TextTrimming = TextTrimming.WordEllipsis,
                            TextAlignment = TextAlignment.Center,
                            ToolTip = fs,
                            Width = 72
                        };

                        iconImage.Stretch = Stretch.Fill;
                        iconImage.HorizontalAlignment = HorizontalAlignment.Center;
                        iconImage.Width = 64;

                        fsUiCont.Width = 72;
                        fsUiCont.Margin = new Thickness(7);
                        fsUiCont.Cursor = Cursors.Hand;

                        fsUiCont.RowDefinitions.Add(new RowDefinition { Height = new GridLength(64) });
                        fsUiCont.RowDefinitions.Add(new RowDefinition { Height = new GridLength(35) });
                        Grid.SetRow(iconImage, 0);
                        Grid.SetRow(fsCapt, 1);

                        fsUiCont.Children.Add(iconImage);
                        fsUiCont.Children.Add(fsCapt);

                        KeyDown += (s, e) =>
                        {
                            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
                                _isCtrlPressed = true;
                        };


                        KeyUp += (s, e) =>
                        {
                            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
                                _isCtrlPressed = false;
                        };

                        fsUiCont.MouseDown += (s, e) =>
                        {
                            if (e.ChangedButton == MouseButton.Left && e.ClickCount == 2)
                            {
                                if (_isCtrlPressed || File.Exists(fs))
                                    System.Diagnostics.Process.Start("explorer", fs);
                                else
                                    LoadFSEntriesUI(fs);
                            }
                        };

                        RenderOptions.SetBitmapScalingMode(iconImage, BitmapScalingMode.HighQuality);

                        ScaleTransform scale = new ScaleTransform(1, 1, 32, 32);
                        TransformGroup tg = new TransformGroup();
                        tg.Children.Add(scale);
                        fsUiCont.RenderTransform = tg;


                        fsUiCont.MouseEnter += FsUiCont_MouseEnter;
                        fsUiCont.MouseLeave += FsUiCont_MouseLeave;

                        ListContainer.Children.Add(fsUiCont);

                        iconImage.Source = await fUtil.GetFileOrFolderIcon(fs);
                    }
                }
                catch(DirectoryNotFoundException nf)
                {
                    MessageBox.Show(this, nf.Message, "MyDrive", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch { }
            }));
        }



        private void FsUiCont_MouseLeave(object sender, MouseEventArgs e)
        {
            _dblanim.From = 1.1;

            _dblanim.To = 1;
            _dblanim.Duration = TimeSpan.FromMilliseconds(200);
            TransformGroup tg = (TransformGroup)((Grid)sender).RenderTransform;
            tg.Children[0].BeginAnimation(ScaleTransform.ScaleXProperty, _dblanim);
            tg.Children[0].BeginAnimation(ScaleTransform.ScaleYProperty, _dblanim);
        }

        private void FsUiCont_MouseEnter(object sender, MouseEventArgs e)
        {
            _dblanim.From = 1;
            _dblanim.To = 1.1;
            _dblanim.Duration = TimeSpan.FromMilliseconds(200);
            TransformGroup tg = (TransformGroup)((Grid)sender).RenderTransform;
            tg.Children[0].BeginAnimation(ScaleTransform.ScaleXProperty, _dblanim);
            tg.Children[0].BeginAnimation(ScaleTransform.ScaleYProperty, _dblanim);
        }

        internal void EnableBlur()
        {
            var windowHelper = new WindowInteropHelper(this);
            var accent = new AccentPolicy();
            var accentStructSize = Marshal.SizeOf(accent);
            var accentPtr = Marshal.AllocHGlobal(accentStructSize);
            var data = new WindowCompositionAttributeData();
            accent.AccentState = AccentState.ACCENT_ENABLE_BLURBEHIND;
            Marshal.StructureToPtr(accent, accentPtr, false);
            data.Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY;
            data.SizeOfData = accentStructSize;
            data.Data = accentPtr;
            SetWindowCompositionAttribute(windowHelper.Handle, ref data);
            Marshal.FreeHGlobal(accentPtr);
        }
        

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            EnableBlur();
        }

        private void Button_MouseEnter(object sender, MouseEventArgs e)
        {
            Button btn = (Button)sender;
            btn.Background = new SolidColorBrush(Color.FromRgb(60, 60, 60));
        }

        private void Button_MouseLeave(object sender, MouseEventArgs e)
        {
            Button btn = (Button)sender;
            btn.Background = new SolidColorBrush(Color.FromRgb(31, 31, 31));
        }



        private void _historyBack_Click(object sender, RoutedEventArgs e)
        {
            if (navigationHistory.Count > 1)
                navigationHistory.RemoveAt(navigationHistory.Count - 1);
            string lastHistPath = navigationHistory.Last();
            navigationHistory.Remove(lastHistPath);
            LoadFSEntriesUI(lastHistPath);
        }


        private void addressBar_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                LoadFSEntriesUI(addressBar.Text);
        }
    }
}
