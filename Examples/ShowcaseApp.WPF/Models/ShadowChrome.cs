using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;

namespace ShowcaseApp.WPF.Models
{
    public class ShadowChrome : Decorator
    {
        private static SolidColorBrush backgroundBrush;
        private static LinearGradientBrush rightBrush;
        private static LinearGradientBrush bottomBrush;
        private static RadialGradientBrush bottomRightBrush;
        private static RadialGradientBrush topRightBrush;
        private static RadialGradientBrush bottomLeftBrush;

        // *** Constructors ***
        static ShadowChrome()
        {
            MarginProperty.OverrideMetadata(typeof(ShadowChrome), new FrameworkPropertyMetadata(new Thickness(0, 0, 4, 4)));
            CreateBrushes();
        }

        // *** Overriden base methods ***
        protected override void OnRender(DrawingContext drawingContext)
        {
            // Calculate the size of the shadow
            double shadowSize = Math.Min(Margin.Right, Margin.Bottom);
            // If there is no shadow, or it is bigger than the size of the child, then just return
            if (shadowSize <= 0 || this.ActualWidth < shadowSize * 2 || this.ActualHeight < shadowSize * 2)
                return;
            // Draw the background (this may show through rounded corners of the child object)
            Rect backgroundRect = new Rect(shadowSize, shadowSize, this.ActualWidth - shadowSize, this.ActualHeight - shadowSize);
            drawingContext.DrawRectangle(backgroundBrush, null, backgroundRect);
            // Now draw the shadow gradients
            Rect topRightRect = new Rect(this.ActualWidth, shadowSize, shadowSize, shadowSize);
            drawingContext.DrawRectangle(topRightBrush, null, topRightRect);
            Rect rightRect = new Rect(this.ActualWidth, shadowSize * 2, shadowSize, this.ActualHeight - shadowSize * 2);
            drawingContext.DrawRectangle(rightBrush, null, rightRect);

            Rect bottomRightRect = new Rect(this.ActualWidth, this.ActualHeight, shadowSize, shadowSize);
            drawingContext.DrawRectangle(bottomRightBrush, null, bottomRightRect);

            Rect bottomRect = new Rect(shadowSize * 2, this.ActualHeight, this.ActualWidth - shadowSize * 2, shadowSize);
            drawingContext.DrawRectangle(bottomBrush, null, bottomRect);

            Rect bottomLeftRect = new Rect(shadowSize, this.ActualHeight, shadowSize, shadowSize);
            drawingContext.DrawRectangle(bottomLeftBrush, null, bottomLeftRect);
        }


        // *** Private static methods ***
        private static void CreateBrushes()
        {
            // Get the colors for the shadow
            Color shadowColor = Color.FromArgb(128, 0, 0, 0);
            Color transparentColor = Color.FromArgb(16, 0, 0, 0);
            // Create a GradientStopCollection from these
            GradientStopCollection gradient = new GradientStopCollection(2);
            gradient.Add(new GradientStop(shadowColor, 0.5));
            gradient.Add(new GradientStop(transparentColor, 1.0));
            gradient.Freeze();
            // Create the background brush
            backgroundBrush = new SolidColorBrush(shadowColor);
            backgroundBrush.Freeze();
            // Create the LinearGradientBrushes
            rightBrush = new LinearGradientBrush(gradient, new Point(0.0, 0.0), new Point(1.0, 0.0)); rightBrush.Freeze();
            bottomBrush = new LinearGradientBrush(gradient, new Point(0.0, 0.0), new Point(0.0, 1.0)); bottomBrush.Freeze();
            // Create the RadialGradientBrushes
            bottomRightBrush = new RadialGradientBrush(gradient);
            bottomRightBrush.GradientOrigin = new Point(0.0, 0.0);
            bottomRightBrush.Center = new Point(0.0, 0.0);
            bottomRightBrush.RadiusX = 1.0;
            bottomRightBrush.RadiusY = 1.0;
            bottomRightBrush.Freeze();

            topRightBrush = new RadialGradientBrush(gradient);
            topRightBrush.GradientOrigin = new Point(0.0, 1.0);
            topRightBrush.Center = new Point(0.0, 1.0);
            topRightBrush.RadiusX = 1.0;
            topRightBrush.RadiusY = 1.0;
            topRightBrush.Freeze();

            bottomLeftBrush = new RadialGradientBrush(gradient);
            bottomLeftBrush.GradientOrigin = new Point(1.0, 0.0);
            bottomLeftBrush.Center = new Point(1.0, 0.0);
            bottomLeftBrush.RadiusX = 1.0;
            bottomLeftBrush.RadiusY = 1.0;
            bottomLeftBrush.Freeze();
        }

    }
    
}
