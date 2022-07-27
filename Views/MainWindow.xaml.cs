using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Forms;
using MyDrive.Controls;
using System.Windows.Interop;
using static NetDrive.Base.Win32.User32;
using System.Runtime.InteropServices;
using NetDrive.Base;
using System.Windows.Media.Animation;
using static MyDrive.Base.AppSettings;
using System.Windows.Input;
using MyDrive.Base;

namespace MyDrive
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        /// <summary>
        /// Global Mouse Hook Helper using Kernel32 Interop
        /// </summary>
        public static MouseHook mHook;
        public static KeyBoardHook kHook;

        private NotifyIcon _notifyIcon;
        private DoubleAnimation _dblanim;
        private Action<int,int> _mouseMove;
        private Views.DirectoriesView _dirViews;


        /// <summary>
        /// The height o the directories windows.
        /// </summary>
        private int _leftLimit
        {
            get
            {
                if(_dirViews != null)
                    return (int)_dirViews.Height;
                return 0;
            }
        }


        /// <summary>
        /// Indicates that window has been shown for the very first time
        /// </summary>
        private bool _shown;


        /// <summary>
        /// Is mouse pointer in 0,0 (Left Top Corner)
        /// </summary>
        private bool is00LC;


        /// <summary>
        /// Is mouse pointer in 0,Maximum Screen Width (Right Top Corner) 
        /// </summary>
        private bool is0MRC;


        /// <summary>
        /// Indicate if form has been shown by user sliding pointer from corners or click interaction
        /// </summary>
        private bool _shownFromCornerSlideOrClick;
        private bool _isCtrlPressed;

        public MainWindow()
        {
            InitializeComponent();

            // Setted far from top
            Top = -300;

            _dblanim = new DoubleAnimation();
            _notifyIcon = new NotifyIcon();
            _notifyIcon.Icon = new System.Drawing.Icon("images/NetDrive.ico");
            _notifyIcon.ContextMenuStrip = new ContextMenuStrip();
            _notifyIcon.ContextMenuStrip.Items.Add(new ToolStripMenuItem("Settings", null, new EventHandler(Settings)));
           //  _notifyIcon.ContextMenuStrip.Items.Add(new ToolStripMenuItem("Ayuda", null, new EventHandler(Settings)));
            _notifyIcon.ContextMenuStrip.Items.Add(new ToolStripMenuItem("Exit", null, new EventHandler(CloseWindow)));
            _notifyIcon.Visible = true;

            _mouseMove = new Action<int,int>((x,y) =>
            {
                if (Options.Generals.ShowOnTopMouseSlide)
                {
                    if (!_shownFromCornerSlideOrClick)
                    {
                        if (ShowIfSlideFromLeft(x, y) || ShowIfSlideFromRight(x, y))
                            ShowDockWindow();
                    }
                    HideWindowIfMouseLeft(y);
                }
            });
            
            Loaded += MainWindow_Loaded;

            mHook = new MouseHook();
            mHook.OnMouseMove += MHook_OnMouseMove;
            mHook.OnMouseClick += MHook_OnMouseClick;
            mHook.Hook();

            kHook = new KeyBoardHook();
            kHook.OnKeyPressed += KHook_OnKeyPressed;
            kHook.OnKeyRealesed += KHook_OnKeyRealesed;
            kHook.Hook();
        }



        private void KHook_OnKeyRealesed(Keys key)
        {
            if (key == Keys.LControlKey || key == Keys.RControlKey)
                _isCtrlPressed = false;
        }


        private void KHook_OnKeyPressed(Keys key)
        {
            if (key == Keys.LControlKey || key == Keys.RControlKey)
                _isCtrlPressed = true;
        }

        private void MHook_OnMouseClick(int x, int y)
        {
            if (Options.Generals.ShowOnTopClick)
                if (y <= 0)
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        ShowDockWindow();
                    }));
        }



        private void MHook_OnMouseMove(int x, int y)
        {
            Dispatcher.BeginInvoke(_mouseMove, x, y);
        }



        private void ShowDockWindow()
        {
            if (Options.Generals.UpdateAtShown)
                foreach (DriveUIControl item in DriveControlContainer.Children)
                    item.GetSpaceInfo();

            Activate();

            _dblanim.To = 0;
            _dblanim.Duration = TimeSpan.FromMilliseconds(300);
            BeginAnimation(TopProperty, _dblanim);

            _dblanim.From = 0;
            _dblanim.To = 1;
            _dblanim.Duration = TimeSpan.FromMilliseconds(100);
            BeginAnimation(OpacityProperty, _dblanim);
        }



        private bool ShowIfSlideFromLeft(int x, int y)
        {
            if (x <= 0 && y <= 0 && is00LC == false)
                is00LC = true;

            // With this, if Y axis is higher than 25 pixels before windows is shown
            // Prevent the window show animation when x >= Screen.PrimaryScreen.WorkingArea.Width * 0.15 && !_shownFromL2R
            if (y > 25)
                is00LC = false;

            if (is00LC)
                if (x >= Screen.PrimaryScreen.WorkingArea.Width * 0.15 && !_shownFromCornerSlideOrClick)
                    return _shownFromCornerSlideOrClick = true;

            return false;
        }



        private bool ShowIfSlideFromRight(int x, int y)
        {
            if (x >= Screen.PrimaryScreen.WorkingArea.Width && y <= 0 && is0MRC == false)
                is0MRC = true;

            // With this, if Y axis is higher than 25 pixels before windows is shown
            // Prevent the window show animation when x <= Screen.PrimaryScreen.WorkingArea.Width * 0.85 && !_shownFromCornerSlideOrClick
            if (y > 25)
                is0MRC = false;

            if (is0MRC)
                if (x <= Screen.PrimaryScreen.WorkingArea.Width * 0.85 && !_shownFromCornerSlideOrClick)
                    return _shownFromCornerSlideOrClick = true;

            return false;
        }



        private void HideWindowIfMouseLeft(int y)
        {
            if (y >= Height + 30 + _leftLimit)
            {
                if (Top == 0)
                {
                    _shownFromCornerSlideOrClick = false;

                    _dblanim.From = 1;
                    _dblanim.To = 0;
                    _dblanim.Duration = TimeSpan.FromMilliseconds(100);
                    BeginAnimation(OpacityProperty, _dblanim);

                    _dblanim.To = -Height;
                    _dblanim.Duration = TimeSpan.FromMilliseconds(200);
                    BeginAnimation(TopProperty, _dblanim);

                    if (_dirViews != null)
                    {
                        _dirViews.Close();
                        _dirViews = null;
                    }
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
            var mid = Screen.PrimaryScreen.WorkingArea.Width * 0.52;
            Left = mid - (Width * 0.5);
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
                dui.MouseDoubleClick += (s, e) =>
                {
                    if (_isCtrlPressed)
                    {
                        if (_dirViews != null)
                        {
                            _dirViews.Close();
                            _dirViews = null;
                        }
                        System.Diagnostics.Process.Start(d.RootPath);
                    }
                };

                dui.ControlClick += (o, e) =>
                {
                    if (!_isCtrlPressed)
                    {
                        if (_dirViews != null)
                        {
                            _dirViews.Close();
                            _dirViews = null;
                        }


                        _dirViews = new Views.DirectoriesView
                        {
                            Top = Height + 10,
                            Left = Left,
                            Width = Width  
                        };
                        _dirViews.LoadFSEntriesUI(d.RootPath);
                        _dirViews.Show();
                    }
                };
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
