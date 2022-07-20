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
using System.Windows.Forms;
using MyDrive.Controls;
using System.Windows.Interop;
using static NetDrive.Base.Win32.User32;
using System.Runtime.InteropServices;
using NetDrive.Base;
using System.Windows.Media.Animation;
using static MyDrive.Base.AppSettings;


namespace MyDrive
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MouseHook mHook;

        private NotifyIcon _notifyIcon;
        private DoubleAnimation dblanim;
        private bool _shown;
        private bool is00;
        private bool is0M;

        public MainWindow()
        {
            InitializeComponent();

            dblanim = new DoubleAnimation();
            _notifyIcon = new NotifyIcon();
            _notifyIcon.Icon = new System.Drawing.Icon("images/NetDrive.ico");
            _notifyIcon.ContextMenuStrip = new ContextMenuStrip();
            _notifyIcon.ContextMenuStrip.Items.Add(new ToolStripMenuItem("Opciones", null, new EventHandler(Settings)));
            _notifyIcon.ContextMenuStrip.Items.Add(new ToolStripMenuItem("Salir", null, new EventHandler(CloseWindow)));
            _notifyIcon.Visible = true;

            Loaded += MainWindow_Loaded;

            mHook = new MouseHook();
            mHook.OnMouseMove += MHook_OnMouseMove;
            mHook.OnMouseClick += MHook_OnMouseClick;
            mHook.Hook();
        }


        private void MHook_OnMouseClick(int x, int y)
        {
            if (Options.Generals.ShowOnTopClick)
            {
                if (y <= 0)
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        Activate();

                        dblanim.To = 0;
                        dblanim.Duration = TimeSpan.FromMilliseconds(300);
                        this.BeginAnimation(TopProperty, dblanim);

                        dblanim.From = 0;
                        dblanim.To = 1;
                        dblanim.Duration = TimeSpan.FromMilliseconds(100);
                        this.BeginAnimation(OpacityProperty, dblanim);

                        if (Options.Generals.UpdateAtShown)
                            foreach (DriveUIControl item in DriveControlContainer.Children)
                                item.GetSpaceInfo();
                    }));
                }
            }
        }


        private void MHook_OnMouseMove(int x, int y)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (Options.Generals.ShowOnTopMouseSlide)
                {
                    ShowIfSlideFromLeft(x, y);
                    ShowIfSlideFromRight(x, y);
                    HideWindowIfMouseLef(y);
                }
            }));

        }



        private void ShowDockWindow()
        {
            Activate();
            is00 = false;

            dblanim.To = 0;
            dblanim.Duration = TimeSpan.FromMilliseconds(300);
            BeginAnimation(TopProperty, dblanim);

            dblanim.From = 0;
            dblanim.To = 1;
            dblanim.Duration = TimeSpan.FromMilliseconds(100);
            BeginAnimation(OpacityProperty, dblanim);
        }



        private void ShowIfSlideFromLeft(int x, int y)
        {
            if (x <= 0 && y <= 0 && is00 == false)
                is00 = true;

            if (is00)
            {
                if (x >= Screen.PrimaryScreen.WorkingArea.Width * 0.15 && y <= 0)
                {
                    ShowDockWindow();

                    if (Options.Generals.UpdateAtShown)
                        foreach (DriveUIControl item in DriveControlContainer.Children)
                            item.GetSpaceInfo();
                }

                if (y > 0)
                    is00 = false;
            }
        }



        private void ShowIfSlideFromRight(int x, int y)
        {
            if (x >= Screen.PrimaryScreen.WorkingArea.Width && y <= 0 && is0M == false)
                is0M = true;

            if (is0M)
            {
                if (x <= Screen.PrimaryScreen.WorkingArea.Width * 0.85 && y <= 0)
                {
                    ShowDockWindow();

                    if (Options.Generals.UpdateAtShown)
                        foreach (DriveUIControl item in DriveControlContainer.Children)
                            item.GetSpaceInfo();
                }

                if (y > 0)
                    is0M = false;
            }
        }



        private void HideWindowIfMouseLef(int y)
        {
            if (y >= Height + 100)
            {
                if (Top == 0)
                {
                    dblanim.From = 1;
                    dblanim.To = 0;
                    dblanim.Duration = TimeSpan.FromMilliseconds(100);
                    BeginAnimation(OpacityProperty, dblanim);

                    dblanim.To = -Height;
                    dblanim.Duration = TimeSpan.FromMilliseconds(200);
                    BeginAnimation(TopProperty, dblanim);
                }

            }
        }



        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);

            if (_shown)
                return;

            _shown = true;

            PointWindowOnScreen();
        }



        void PointWindowOnScreen()
        {
            Top = -Height;
            var mid = Screen.PrimaryScreen.WorkingArea.Width * 0.5;
            var left = mid - (Width / 2);
            Left = left;
        }



        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadEasyAccess();
            EnableBlur();
            Background = new SolidColorBrush(Options.Visual.TraslucentColor);
        }


        private void LoadEasyAccess()
        {
            DriveControlContainer.Children.Clear();
            foreach (var d in Options.EasyAccessItems.AccessItems)
            {
                var dui = new DriveUIControl(d);
                DriveControlContainer.Children.Add(dui);
            }
        }


        private void CloseWindow(object s, EventArgs e)
        {
            _notifyIcon.Visible = false;
            mHook.UnHook();
            Close();
        }



        private void Settings(object s, EventArgs e)
        {
            Controls.Settings st = new Controls.Settings();
            st.OnVisualChange += St_OnVisualChange;
            st.Show();
        }

        private void St_OnVisualChange(Base.ControlSettings settings)
        {
            if (settings is Base.VisualSettings visualSettings)
                Background = new SolidColorBrush(visualSettings.TraslucentColor);
            else if (settings is Base.EAI items)
                PointWindowOnScreen();
            LoadEasyAccess();
        }

        internal void EnableBlur()
        {
            var windowHelper = new WindowInteropHelper(this);

            var accent = new AccentPolicy();
            accent.AccentState = AccentState.ACCENT_ENABLE_BLURBEHIND;

            var accentStructSize = Marshal.SizeOf(accent);

            var accentPtr = Marshal.AllocHGlobal(accentStructSize);
            Marshal.StructureToPtr(accent, accentPtr, false);

            var data = new WindowCompositionAttributeData();
            data.Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY;
            data.SizeOfData = accentStructSize;
            data.Data = accentPtr;

            SetWindowCompositionAttribute(windowHelper.Handle, ref data);

            Marshal.FreeHGlobal(accentPtr);
        }
    }
}
