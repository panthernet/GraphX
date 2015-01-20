using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;

namespace WindowGlows
{
    enum Location
    {
        Left, Top, Right, Bottom
    }

    [TemplatePart(Name = "PART_Glow", Type = typeof(Border))]
    class GlowWindow : Window
    {
        static GlowWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata( typeof(GlowWindow), new FrameworkPropertyMetadata( typeof(GlowWindow) ) );
        }

        public double glowThickness = 40d, tolerance = 72d;

        public Location Location;
        public Action Update;
        public Func<Point, Cursor> GetCursor;
        public Func<Point, HT> GetHT;
        Border Glow;
        Func<bool> canResize;

        public void NotifyResize( HT ht )
        {
            NativeMethods.SendNotifyMessage( new WindowInteropHelper( Owner ).Handle, (int)WM.NCLBUTTONDOWN, (IntPtr)ht, IntPtr.Zero );
        }

        public GlowWindow()
        {
            WindowStyle = WindowStyle.None;
            ResizeMode = ResizeMode.NoResize;
            AllowsTransparency = true;
            SnapsToDevicePixels = true;
            ShowInTaskbar = false;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            Glow = (Border)GetTemplateChild( "PART_Glow" );
            if ( Glow == null )
                throw new Exception( "PART_Glow not found." );
            Update();
        }

        protected override void OnSourceInitialized( EventArgs e )
        {
            base.OnSourceInitialized( e );
            IntPtr hwnd = new WindowInteropHelper( this ).Handle;
            int ws_ex = NativeMethods.GetWindowLong( hwnd, (int)GWL.EXSTYLE );
            ws_ex |= (int)WS_EX.TOOLWINDOW;
            NativeMethods.SetWindowLong( hwnd, (int)GWL.EXSTYLE, ws_ex );
            HwndSource.FromHwnd( hwnd ).AddHook( WinMain );
        }

        public virtual IntPtr WinMain( IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled )
        {
            switch ( (WM)msg )
            {
                case WM.MOUSEACTIVATE:
                    {
                        handled = true;
                        return new IntPtr( 3 ); // MA_NOACTIVATE
                    }
                case WM.NCHITTEST:
                    {
                        if ( canResize() )
                        {
                            Point ptScreen = NativeMethods.LPARAMTOPOINT( lParam );
                            Point ptClient = PointFromScreen( ptScreen );
                            Cursor = GetCursor( ptClient );
                        }
                        break;
                    }
                case WM.LBUTTONDOWN:
                    {
                        POINT ptScreenWin32;
                        NativeMethods.GetCursorPos( out ptScreenWin32 );
                        Point ptScreen = new Point( ptScreenWin32.x, ptScreenWin32.y );
                        Point ptClient = PointFromScreen( ptScreen );
                        HT result = GetHT( ptClient );
                        IntPtr ownerHwnd = new WindowInteropHelper( Owner ).Handle;
                        NativeMethods.SendNotifyMessage( ownerHwnd, (int)WM.NCLBUTTONDOWN, (IntPtr)result, IntPtr.Zero );
                        break;
                    }
            }
            return IntPtr.Zero;
        }

        public void OwnerChanged()
        {
            canResize = () => Owner.ResizeMode == ResizeMode.CanResize ? true :
            Owner.ResizeMode == ResizeMode.CanResizeWithGrip ? true : false;

            switch ( Location )
            {
                case Location.Left:
                    {
                        GetCursor = pt =>
                            new Rect( new Point( 0, 0 ), new Size( ActualWidth, tolerance ) ).Contains( pt ) ?
                            Cursors.SizeNWSE :
                            new Rect( new Point( 0, ActualHeight - tolerance ), new Size( ActualWidth, tolerance ) ).Contains( pt ) ?
                            Cursors.SizeNESW :
                            Cursors.SizeWE;

                        GetHT = pt => new Rect( new Point( 0, 0 ), new Size( ActualWidth, tolerance ) ).Contains( pt ) ?
                            HT.TOPLEFT :
                            new Rect( new Point( 0, ActualHeight - tolerance ), new Size( ActualWidth, tolerance ) ).Contains( pt ) ?
                            HT.BOTTOMLEFT :
                            HT.LEFT;

                        Update = delegate
                        {
                            if ( Glow != null )
                                Glow.Margin = new Thickness( glowThickness, glowThickness, -glowThickness, glowThickness );
                            Left = Owner.Left - glowThickness;
                            Top = Owner.Top - glowThickness;
                            Width = glowThickness;
                            Height = Owner.ActualHeight + glowThickness * 2;
                        };
                        break;
                    }

                case Location.Top:
                    {
                        GetCursor = pt =>
                            new Rect( new Point( 0, 0 ), new Size( tolerance, glowThickness ) ).Contains( pt ) ?
                            Cursors.SizeNWSE :
                            new Rect( new Point( ActualWidth - tolerance, 0 ), new Size( tolerance, ActualHeight ) ).Contains( pt ) ?
                            Cursors.SizeNESW :
                            Cursors.SizeNS;

                        GetHT = pt =>
                            new Rect( new Point( 0, 0 ), new Size( tolerance, glowThickness ) ).Contains( pt ) ?
                            HT.TOPLEFT :
                            new Rect( new Point( ActualWidth - tolerance, 0 ), new Size( tolerance, ActualHeight ) ).Contains( pt ) ?
                            HT.TOPRIGHT :
                            HT.TOP;

                        Update = delegate
                        {
                            if ( Glow != null )
                                Glow.Margin = new Thickness( glowThickness, glowThickness, glowThickness, -glowThickness );
                            Left = Owner.Left - glowThickness;
                            Top = Owner.Top - glowThickness;
                            Width = Owner.ActualWidth + glowThickness * 2;
                            Height = glowThickness;
                        };
                        break;
                    }

                case Location.Right:
                    {
                        GetCursor = pt =>
                            new Rect( new Point( 0, 0 ), new Size( ActualWidth, tolerance ) ).Contains( pt ) ?
                            Cursors.SizeNESW :
                            new Rect( new Point( 0, ActualHeight - tolerance ), new Size( ActualWidth, tolerance ) ).Contains( pt ) ?
                            Cursors.SizeNWSE :
                            Cursors.SizeWE;

                        GetHT = pt => new Rect( new Point( 0, 0 ), new Size( ActualWidth, tolerance ) ).Contains( pt ) ?
                            HT.TOPRIGHT :
                            new Rect( new Point( 0, ActualHeight - tolerance ), new Size( ActualWidth, tolerance ) ).Contains( pt ) ?
                            HT.BOTTOMRIGHT :
                            HT.RIGHT;


                        Update = delegate
                        {
                            if ( Glow != null )
                                Glow.Margin = new Thickness( -glowThickness, glowThickness, glowThickness, glowThickness );
                            Left = Owner.Left + Owner.ActualWidth;
                            Top = Owner.Top - glowThickness;
                            Width = glowThickness;
                            Height = Owner.ActualHeight + glowThickness * 2;
                        };
                        break;
                    }

                case Location.Bottom:
                    {
                        GetCursor = pt =>
                            new Rect( new Point( 0, 0 ), new Size( tolerance, glowThickness ) ).Contains( pt ) ?
                            Cursors.SizeNESW :
                            new Rect( new Point( ActualWidth - tolerance, 0 ), new Size( tolerance, ActualHeight ) ).Contains( pt ) ?
                            Cursors.SizeNWSE :
                            Cursors.SizeNS;

                        GetHT = pt =>
                            new Rect( new Point( 0, 0 ), new Size( tolerance, glowThickness ) ).Contains( pt ) ?
                            HT.BOTTOMLEFT :
                            new Rect( new Point( ActualWidth - tolerance, 0 ), new Size( tolerance, ActualHeight ) ).Contains( pt ) ?
                            HT.BOTTOMRIGHT :
                            HT.BOTTOM;

                        Update = delegate
                        {
                            if ( Glow != null )
                                Glow.Margin = new Thickness( glowThickness, -glowThickness, glowThickness, glowThickness );
                            Left = Owner.Left - glowThickness;
                            Top = Owner.Top + Owner.ActualHeight;
                            Width = Owner.ActualWidth + glowThickness * 2;
                            Height = glowThickness;
                        };
                        break;
                    }
            }
            Owner.LocationChanged += delegate
            {
                Update();
            };
            Owner.SizeChanged += delegate
            {
                Update();
            };
            Owner.StateChanged += delegate
            {
                switch ( Owner.WindowState )
                {
                    case WindowState.Maximized:
                        Hide();
                        break;
                    default:
                        Show();
                        Owner.Activate();
                        break;
                }
            };
            Owner.Activated += delegate
            {
                Binding activeBrushBinding = new Binding();
                activeBrushBinding.Path = new PropertyPath( GlowManager.ActiveGlowBrushProperty );
                activeBrushBinding.Source = Owner;
                SetBinding( ForegroundProperty, activeBrushBinding );
            };
            Owner.Deactivated += delegate
            {
                Binding activeBrushBinding = new Binding();
                activeBrushBinding.Path = new PropertyPath( GlowManager.InactiveGlowBrushProperty );
                activeBrushBinding.Source = Owner;
                SetBinding( ForegroundProperty, activeBrushBinding );
            };
            Update();
            Show();
        }
    }
}
