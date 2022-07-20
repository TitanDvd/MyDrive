using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using static NetDrive.Base.Win32.User32;

namespace MyDrive
{
    public class ChromeStyle : Window
    {
        Point cursorOffset;
        double restoreTop;

        FrameworkElement BorderLeft;
        FrameworkElement BorderTopLeft;
        FrameworkElement BorderTop;
        FrameworkElement BorderTopRight;
        FrameworkElement BorderRight;
        FrameworkElement BorderBottomRight;
        FrameworkElement BorderBottom;
        FrameworkElement BorderBottomLeft;
        FrameworkElement caption;
        FrameworkElement frame;

        Button min;
        Button max;
        Button close;

        IntPtr handle;


        public ChromeStyle()
        {
            Style = (Style)TryFindResource("ChromeWindowStyle");

            SourceInitialized += (sender, e) =>
            {
                handle = new WindowInteropHelper(this).Handle;
                
                HwndSource.FromHwnd(handle).AddHook(new HwndSourceHook(WndProc));
            };
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch(msg)
            {
                case 0x0024:
                    WmGetMinMaxInfo(hwnd, lParam);
                    handled = true;
                    break;
            }

            return IntPtr.Zero;
        }

        private void WmGetMinMaxInfo(IntPtr hwnd, IntPtr lParam)
        {
            MINMAXINFO mmi = (MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(MINMAXINFO));
            int MONITOR_DEFAULTTONEAREST = 0x00000002;
            IntPtr monitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);

            if(monitor != IntPtr.Zero)
            {
                MONITORINFO monitorinfo = new MONITORINFO();
                GetMonitorInfo(monitor, monitorinfo);

                RECT rcWorkArea = monitorinfo.rcWork;
                RECT rcMonitorArea = monitorinfo.rcMonitor;

                mmi.ptMaxPosition.x = Math.Abs(rcWorkArea.left - rcMonitorArea.left);
                mmi.ptMaxPosition.y = Math.Abs(rcWorkArea.top - rcMonitorArea.top);
                mmi.ptMaxSize.x = Math.Abs(rcWorkArea.right - rcWorkArea.left);
                mmi.ptMaxSize.y = Math.Abs(rcWorkArea.bottom - rcWorkArea.top);
            }

            Marshal.StructureToPtr(mmi, lParam, true);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            RegisterFrame();
            RegisterBorders();
            RegisterCaption();
            RegisterMinButton();
            RegisterMaxButton();
            RegisterCloseButton();
        }

        private void RegisterCloseButton()
        {
            close = (Button)GetTemplateChild("PART_WindowCaptionClose");

            if (close != null)
            {
                close.Click += (sender, e) => Close();
            }
        }

        private void RegisterMaxButton()
        {
            max = (Button)GetTemplateChild("PART_WindowCaptionMax");

            if(max != null)
            {
                max.Click += (sender, e) =>
                {
                    if (WindowState == WindowState.Normal)
                        WindowState = WindowState.Maximized;
                    else
                        WindowState = WindowState.Normal;
                };
            }

        }

        private void RegisterMinButton()
        {
            min = (Button)GetTemplateChild("PART_WindowCaptionMin");

            if (min != null)
            {
                min.Click += (sender, e) =>
                {
                    WindowState = WindowState.Minimized;
                };
            }
        }

        private void RegisterCaption()
        {
            caption = (FrameworkElement)GetTemplateChild("PART_WindowCaption");

            if(caption != null)
            {
                caption.MouseLeftButtonDown += (sender, e) =>
                {
                    restoreTop = e.GetPosition(this).Y;

                    if(e.ClickCount == 2 && e.ChangedButton == MouseButton.Left && (ResizeMode != ResizeMode.CanMinimize && ResizeMode != ResizeMode.NoResize))
                    {
                        if(WindowState != WindowState.Maximized)
                        {
                            WindowState = WindowState.Maximized;
                        }
                        else
                        {
                            WindowState = WindowState.Normal;
                        }
                    }

                    DragMove();
                };


                caption.MouseMove += (sender, e) =>
                {
                    if(e.LeftButton == MouseButtonState.Pressed && caption.IsMouseOver)
                    {
                        if(WindowState == WindowState.Maximized)
                        {
                            WindowState = WindowState.Normal;
                            Top = restoreTop - 10;
                            DragMove();
                        }
                    }
                };
            }
        }

        private void RegisterBorders()
        {
            BorderLeft          = (FrameworkElement)GetTemplateChild("PART_WindowBorderLeft");
            BorderTopLeft       = (FrameworkElement)GetTemplateChild("PART_WindowBorderTopLeft");
            BorderTop           = (FrameworkElement)GetTemplateChild("PART_WindowBorderTop");
            BorderTopRight      = (FrameworkElement)GetTemplateChild("PART_WindowBorderTopRight");
            BorderRight         = (FrameworkElement)GetTemplateChild("PART_WindowBorderRight");
            BorderBottomRight   = (FrameworkElement)GetTemplateChild("PART_WindowBorderBottomRight");
            BorderBottom        = (FrameworkElement)GetTemplateChild("PART_WindowBorderBottom");
            BorderBottomLeft    = (FrameworkElement)GetTemplateChild("PART_WindowBorderBottomLeft");

            RegisterBorderEvent(WindowBorderEdge.Left, BorderLeft);
            RegisterBorderEvent(WindowBorderEdge.TopLeft, BorderTopLeft);
            RegisterBorderEvent(WindowBorderEdge.Top, BorderTop);
            RegisterBorderEvent(WindowBorderEdge.TopRight, BorderTopRight);
            RegisterBorderEvent(WindowBorderEdge.Right, BorderRight);
            RegisterBorderEvent(WindowBorderEdge.BottomRight, BorderBottomRight);
            RegisterBorderEvent(WindowBorderEdge.Bottom, BorderBottom);
            RegisterBorderEvent(WindowBorderEdge.BottomLeft, BorderBottomLeft);
        }


        private void RegisterBorderEvent(WindowBorderEdge edge, FrameworkElement border)
        {
            border.MouseEnter += (sender, e) =>
            {
                if(WindowState != WindowState.Minimized && ResizeMode == ResizeMode.CanResize)
                {
                    switch(edge)
                    {
                        case WindowBorderEdge.Left:
                        case WindowBorderEdge.Right:
                            border.Cursor = Cursors.SizeWE;
                            break;

                        case WindowBorderEdge.Top:
                        case WindowBorderEdge.Bottom:
                            border.Cursor = Cursors.SizeNS;
                            break;

                        case WindowBorderEdge.TopLeft:
                        case WindowBorderEdge.BottomRight:
                            border.Cursor = Cursors.SizeNWSE;
                            break;

                        case WindowBorderEdge.TopRight:
                        case WindowBorderEdge.BottomLeft:
                            border.Cursor = Cursors.SizeNESW;
                            break;
                    }
                }
                else
                {
                    border.Cursor = Cursors.Arrow;
                }
            };


            border.MouseLeftButtonDown += (sender, e) =>
            {
                if (WindowState != WindowState.Minimized && ResizeMode == ResizeMode.CanResize)
                {
                    Point cursorLocation = e.GetPosition(this);
                    Point cursorOffset = new Point();

                    switch (edge)
                    {
                        case WindowBorderEdge.Left:
                            cursorOffset.X = cursorLocation.X;
                            break;
                        case WindowBorderEdge.TopLeft:
                            cursorOffset.X = cursorLocation.X;
                            cursorOffset.Y = cursorLocation.Y;
                            break;
                        case WindowBorderEdge.Top:
                            cursorOffset.Y = cursorLocation.Y;
                            break;
                        case WindowBorderEdge.TopRight:
                            cursorOffset.X = (Width - cursorLocation.X);
                            cursorOffset.Y = (Height - cursorLocation.Y);
                            break;
                        case WindowBorderEdge.Bottom:
                            cursorOffset.Y = (Height - cursorLocation.Y);
                            break;
                        case WindowBorderEdge.BottomLeft:
                            cursorOffset.X = cursorLocation.X;
                            cursorOffset.Y = (Height - cursorLocation.Y);
                            break;
                    }

                    this.cursorOffset = cursorOffset;
                    border.CaptureMouse();

                }
            };


            border.MouseMove += (sender, e) =>
            {
                if(WindowState != WindowState.Maximized && border.IsMouseCaptured && ResizeMode == ResizeMode.CanResize)
                {
                    Point cursorLocation = e.GetPosition(this);

                    double nHorizontalChange = (cursorLocation.X - cursorOffset.X);
                    double pHorizontalChange = (cursorLocation.X + cursorOffset.X);
                    double nVerticalChange = (cursorLocation.Y - cursorOffset.Y);
                    double pVerticalChange = (cursorLocation.Y + cursorOffset.Y);

                    switch(edge)
                    {
                        case WindowBorderEdge.Left:
                            if (Width - nHorizontalChange <= MinWidth)
                                break;
                            Left += nHorizontalChange;
                            Width -= nHorizontalChange;
                            break;

                        case WindowBorderEdge.TopLeft:
                            if (Width - nHorizontalChange <= MinWidth)
                                break;
                            Left += nHorizontalChange;
                            Width -= nHorizontalChange;
                            if (Height - nVerticalChange <= MinHeight)
                                break;
                            Top += nVerticalChange;
                            Height -= nVerticalChange;
                            break;

                        case WindowBorderEdge.Top:
                            if (Height - nVerticalChange <= MinHeight)
                                break;
                            Top += nVerticalChange;
                            Height -= nVerticalChange;
                            break;

                        case WindowBorderEdge.TopRight:
                            if (pHorizontalChange >= MinWidth)
                                break;
                            Width = pHorizontalChange;
                            if (Height - nVerticalChange <= MinHeight)
                                break;
                            Top += nVerticalChange;
                            Height -= nVerticalChange;
                            break;

                        case WindowBorderEdge.Right:
                            if (pHorizontalChange <= MinWidth)
                                break;
                            Width = pHorizontalChange;
                            break;

                        case WindowBorderEdge.BottomRight:
                            if (pHorizontalChange <= MinWidth)
                                break;
                            Width = pHorizontalChange;
                            if (pVerticalChange <= MinHeight)
                                break;
                            Height = pVerticalChange;
                            break;

                        case WindowBorderEdge.Bottom:
                            if (pVerticalChange <= MinWidth)
                                break;
                            Height = pVerticalChange;
                            break;

                        case WindowBorderEdge.BottomLeft:
                            if (Width - nHorizontalChange <= MinWidth)
                                break;
                            Left += nHorizontalChange;
                            Width -= nHorizontalChange;
                            if (pVerticalChange <= MinHeight)
                                break;
                            Height = pVerticalChange;
                            break;
                    }
                }
            };


            border.MouseLeftButtonUp += (sender, e) =>
            {
                border.ReleaseMouseCapture();
            };
        }



        public enum WindowBorderEdge
        {
            Left,
            TopLeft,
            Top,
            TopRight,
            Right,
            BottomRight,
            Bottom,
            BottomLeft
        }


        private void RegisterFrame()
        {
            frame = (FrameworkElement)GetTemplateChild("PART_WindowFrame");
        }
    }
}
