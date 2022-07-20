using MyDrive.Base.Types;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static MyDrive.Base.AppSettings;


namespace MyDrive.Controls
{
    /// <summary>
    /// Lógica de interacción para DriveUIControl.xaml
    /// </summary>
    public partial class DriveUIControl : UserControl
    {
        private EasyAccessItem _ndt { get; set; }
        private DoubleAnimation dblanim;
        delegate void FillSpaceDiskInfoDelegate(long total, long freespace, long ocupated);


        public DriveUIControl(EasyAccessItem ndt)
        {
            dblanim = new DoubleAnimation();
            _ndt = ndt;
            InitializeComponent();

            Mouse.AddMouseEnterHandler(this, new MouseEventHandler(UserControl_MouseEnter));
            Mouse.AddMouseLeaveHandler(this, new MouseEventHandler(UserControl_MouseLeave));

            drive_Label.Foreground = new SolidColorBrush(Options.Visual.FontColor);
            Loaded += DriveUIControl_Loaded;
        }

        public DriveUIControl()
        {
            InitializeComponent();
        }

        private void DriveUIControl_Loaded(object sender, RoutedEventArgs e)
        {
            drive_Label.Content = _ndt.Tag;
            GetSpaceInfo();
        }


        public async void GetSpaceInfo()
        {
            await Task.Factory.StartNew(() =>
            {
                long freeSpace = 0;
                long totalSpace = 0;
                long availableSpace = 0;

                Base.Win32.Kernel32.GetDiskFreeSpaceEx(_ndt.RootPath, ref availableSpace, ref totalSpace, ref freeSpace);

                FillSpaceDiskInfoDelegate del = FillDiskSpaceInfo;
                Dispatcher.BeginInvoke(del, totalSpace, availableSpace, totalSpace - availableSpace);
            });
        }



        void FillDiskSpaceInfo(long total, long free, long ocupated)
        {
            double t = Math.Round(total / Math.Pow(1024, 3)); // GB
            double f = Math.Round(free / Math.Pow(1024, 3)); // GB
            double o = Math.Round(ocupated / Math.Pow(1024, 3)); // GB
            //lbl_totalSpace.Text = $"{t} GB";
            //lbl_OcupatedSpace.Text = $"{o} GB";

            var freePercent = (o * 100) / t;
            if (freePercent <= 45)
                pbar.Foreground = new SolidColorBrush(Options.Visual.GoodSpaceBarColor);
            else if (freePercent > 45 && freePercent <= 90)
                pbar.Foreground = new SolidColorBrush(Options.Visual.WarningSpaceBarColor);
            else
                pbar.Foreground = new SolidColorBrush(Options.Visual.FullSpaceBarColor);

            pbar.Maximum = (int)t;
            pbar.Value = (int)o;

            drive_freeAndTotal.Foreground = new SolidColorBrush( Options.Visual.FontColor );
            lbl_ocupated.Foreground = new SolidColorBrush(Options.Visual.FontColor);

            if (t > 1024)
            {
                t = t / 1024; // TB
                drive_freeAndTotal.Content = $"{f} GB Libres de {(int)Math.Round(t)} TB";
            }
            else
                drive_freeAndTotal.Content = $"{f} GB Libres de {t} GB";

            lbl_ocupated.Content = $"{o} GB ocupados";
        }



        

        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            dblanim.From = 1;

            dblanim.To = 1.1;
            dblanim.Duration = TimeSpan.FromMilliseconds(200);
            rectScale.BeginAnimation(ScaleTransform.ScaleXProperty, dblanim);
            rectScale.BeginAnimation(ScaleTransform.ScaleYProperty, dblanim);

        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            dblanim.From = 1.1;

            dblanim.To = 1;
            dblanim.Duration = TimeSpan.FromMilliseconds(200);
            rectScale.BeginAnimation(ScaleTransform.ScaleXProperty, dblanim);
            rectScale.BeginAnimation(ScaleTransform.ScaleYProperty, dblanim);

        }

        private void UserControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start(_ndt.RootPath);
        }
    }
}
