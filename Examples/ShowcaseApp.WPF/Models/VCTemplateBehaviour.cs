using System.Windows;
using System.Windows.Media;

namespace ShowcaseApp.WPF.Models
{
    /// <summary>
    /// Contains helpful attached properties for VertexControl class
    /// </summary>
    public static class VCTemplateBehaviour
    {
        public static readonly DependencyProperty BackgroundColorProperty = DependencyProperty.RegisterAttached(
            "BackgroundColor", typeof(SolidColorBrush), typeof(VCTemplateBehaviour),
            new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty BorderThicknessProperty = DependencyProperty.RegisterAttached(
            "BorderThickness", typeof(Thickness), typeof(VCTemplateBehaviour),
            new FrameworkPropertyMetadata(new Thickness(2)));

        public static Thickness GetBorderThickness(DependencyObject dependencyObject)
        {
            return (Thickness)dependencyObject.GetValue(BorderThicknessProperty);
        }

        public static void SetBorderThickness(DependencyObject dependencyObject, Thickness value)
        {
            dependencyObject.SetValue(BorderThicknessProperty, value);
        }

        public static SolidColorBrush GetBackgroundColor(DependencyObject dependencyObject)
        {
            return (SolidColorBrush)dependencyObject.GetValue(BackgroundColorProperty);
        }

        public static void SetBackgroundColor(DependencyObject dependencyObject, SolidColorBrush value)
        {
            dependencyObject.SetValue(BackgroundColorProperty, value);
        }
    }
}
