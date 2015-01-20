using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace WindowGlows
{
    /// <summary>
    /// Contains static methods for making <see cref="Window"/>s glow.
    /// </summary>
    public static class GlowManager
    {
        static readonly DependencyProperty GlowInfoProperty =
            DependencyProperty.RegisterAttached( "GlowInfo",
                                                typeof(GlowInfo),
                                                typeof(GlowManager) );

        /// <summary>
        /// Identifies the ActiveGlowBrush attached property.
        /// </summary>
        public static readonly DependencyProperty ActiveGlowBrushProperty =
            DependencyProperty.RegisterAttached( "ActiveGlowBrush",
                                                typeof(Brush),
                                                typeof(GlowManager),
                                                new FrameworkPropertyMetadata(
                                                    new SolidColorBrush(
                                                        Color.FromArgb( 0xff, 0x00, 0x7a, 0xcc ) ) ) );

        /// <summary>
        /// Identifies the InactiveGlowBrush attached property.
        /// </summary>
        public static readonly DependencyProperty InactiveGlowBrushProperty =
            DependencyProperty.RegisterAttached( "InactiveGlowBrush",
                                                typeof(Brush),
                                                typeof(GlowManager),
                                                new FrameworkPropertyMetadata(
                                                    new SolidColorBrush( Colors.Black ) { Opacity = 0.25 } ) );

        /// <summary>
        /// Identifies the EnableGlow attached property
        /// </summary>
        public static readonly DependencyProperty EnableGlowProperty =
            DependencyProperty.RegisterAttached( "EnableGlow",
                                                typeof(bool),
                                                typeof(GlowManager),
                                                new FrameworkPropertyMetadata( ( d, e ) =>
        {
            if ( !DesignerProperties.GetIsInDesignMode( new DependencyObject() ) )
            {
                if ( (bool)e.NewValue == true )
                    Assign( (Window)d );
                else
                    Unassign( (Window)d );
            }
        } ) );

        static Action<Window> assignGlows = window =>
        {
            if ( GetGlowInfo( window ) != null )
                throw new InvalidOperationException( "Glows have already been assigned." );
            GlowInfo glowInfo = new GlowInfo();
            glowInfo.glows.Add( new GlowWindow { Location = Location.Left } );
            glowInfo.glows.Add( new GlowWindow { Location = Location.Top } );
            glowInfo.glows.Add( new GlowWindow { Location = Location.Right } );
            glowInfo.glows.Add( new GlowWindow { Location = Location.Bottom } );
            foreach ( GlowWindow glow in glowInfo.glows )
            {
                glow.Owner = window;
                glow.OwnerChanged();
            }
            SetGlowInfo( window, glowInfo );
        };

        static bool Assign( Window window )
        {
            if ( window == null )
                throw new ArgumentNullException( "window" );
            if ( GetGlowInfo( window ) != null )
                return false;
            else if ( !window.IsLoaded )
                window.SourceInitialized += delegate
                {
                    assignGlows( window );
                };
            else
            {
                assignGlows( window );
            }
            return true;
        }

        static bool Unassign( Window window )
        {
            GlowInfo info = GetGlowInfo( window );
            if ( info == null )
                return false;
            else
            {
                foreach ( GlowWindow glow in info.glows )
                {
                    try
                    {
                        glow.Close();
                    }
                    catch
                    {
                        // Do nothing
                    }
                }
                SetGlowInfo( window, null );
            }
            return true;
        }

        /// <summary>
        /// Gets the <see cref="Brush"/> for the glow when the <see cref="Window"/> is active.
        /// </summary>
        /// <param name="window">The <see cref="Window"/>.</param>
        /// <returns>The brush for the glow when the <see cref="Window"/> is active.</returns>
        [Category("Brush")]
        [AttachedPropertyBrowsableForType(typeof(Window))]
        public static Brush GetActiveGlowBrush( Window window )
        {
            return (Brush)window.GetValue( ActiveGlowBrushProperty );
        }

        /// <summary>
        /// Sets the <see cref="Brush"/> for the glow when the <see cref="Window"/> is active.
        /// </summary>
        /// <param name="window">The <see cref="Window"/>.</param>
        /// <param name="value">The <see cref="Brush"/> for the glow when the <see cref="Window"/> is active.</param>
        public static void SetActiveGlowBrush( Window window, Brush value )
        {
            window.SetValue( ActiveGlowBrushProperty, value );
        }

        /// <summary>
        /// Gets the <see cref="Brush"/> for the glow when the <see cref="Window"/> is inactive.
        /// </summary>
        /// <param name="window">The <see cref="Window"/>.</param>
        /// <returns>The <see cref="Brush"/> for the glow when the <see cref="Window"/> is inactive.</returns>
        [Category("Brush")]
        [AttachedPropertyBrowsableForType(typeof(Window))]
        public static Brush GetInactiveGlowBrush( Window window )
        {
            return (Brush)window.GetValue( InactiveGlowBrushProperty );
        }

        /// <summary>
        /// Sets the <see cref="Brush"/> for the glow when the <see cref="Window"/> is inactive.
        /// </summary>
        /// <param name="window">The <see cref="Window"/>.</param>
        /// <param name="value">The <see cref="Brush"/> for the glow when the <see cref="Window"/> is inactive.</param>
        public static void SetInactiveGlowBrush( Window window, Brush value )
        {
            window.SetValue( InactiveGlowBrushProperty, value );
        }

        /// <summary>
        /// Gets whether glows are enabled for the <see cref="Window"/>.
        /// </summary>
        /// <param name="window">The <see cref="Window"/>.</param>
        /// <returns>Whether glows are enabled for the <see cref="Window"/>.</returns>
        [Category("Appearance")]
        [AttachedPropertyBrowsableForType(typeof(Window))]
        public static bool GetEnableGlow( Window window )
        {
            return (bool)window.GetValue( EnableGlowProperty );
        }

        /// <summary>
        /// Sets whether glows are enabled for the <see cref="Window"/>.
        /// </summary>
        /// <param name="window">The <see cref="Window"/>.</param>
        /// <param name="value">Whether glows are enabled for the <see cref="Window"/>.</param>
        public static void SetEnableGlow( Window window, bool value )
        {
            window.SetValue( EnableGlowProperty, value );
        }

        internal static GlowInfo GetGlowInfo( Window window )
        {
            return (GlowInfo)window.GetValue( GlowInfoProperty );
        }

        internal static void SetGlowInfo( Window window, GlowInfo info )
        {
            window.SetValue( GlowInfoProperty, info );
        }
    }

    class GlowInfo
    {
        public readonly Collection<GlowWindow> glows = new Collection<GlowWindow>();
    }
}
