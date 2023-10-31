using System;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GraphX.Common.Enums;
using GraphX.Common.Exceptions;
using Brushes = System.Windows.Media.Brushes;
using Size = System.Windows.Size;

namespace GraphX.Controls
{
    public static class PrintHelper
    {
        /// <summary>
        /// Gets WPF default DPI
        /// </summary>
        public const double DEFAULT_DPI = 96d;

        /// <summary>
        /// Gets or sets the pixel format of an exported image
        /// </summary>
        public static PixelFormat PixelFormat = PixelFormats.Pbgra32;

        /// <summary>
        /// Helper method which calculates estimated image DPI based on the input criterias
        /// </summary>
        /// <param name="vis">GraphArea object</param>
        /// <param name="imgdpi">Desired DPI</param>
        /// <param name="dpiStep">DPI decrease step while estimating</param>
        /// <param name="estPixelCount">Pixel quantity threshold</param>
        public static double CalculateEstimatedDPI(IGraphAreaBase vis, double imgdpi, double dpiStep, ulong estPixelCount)
        {
            bool result = false;
            double currentDPI = imgdpi;
            while (!result)
            {
                if (CalulateSize(vis.ContentSize.Size, currentDPI) <= estPixelCount)
                    result = true;
                else currentDPI -= dpiStep;
                if (currentDPI < 0) return 0;
            }
            return currentDPI;
        }


        private static ulong CalulateSize(Size desiredSize, double dpi)
        {
            return (ulong) (desiredSize.Width*(dpi/DEFAULT_DPI) + 100) *
                   (ulong) (desiredSize.Height*(dpi/DEFAULT_DPI) + 100);
        }

        /// <summary>
        /// Method exports the GraphArea to an png image.
        /// </summary>
        /// <param name="surface">GraphArea control</param>
        /// <param name="path">Image destination path</param>
        /// <param name="useZoomControlSurface"></param>
        /// <param name="imgdpi">Optional image DPI parameter</param>
        /// <param name="imgQuality">Optional image quality parameter (for some formats like JPEG)</param>
        /// <param name="itype"></param>
        public static void ExportToImage(IGraphAreaBase surface, Uri path, ImageType itype, bool useZoomControlSurface = false, double imgdpi = DEFAULT_DPI, int imgQuality = 100)
        {
            if (!useZoomControlSurface)
                surface.SetPrintMode(true, true, 100);
            //Create a render bitmap and push the surface to it
            var vis = (UIElement) surface;
            if (useZoomControlSurface)
            {
                var canvas = (Canvas) surface;
                if (canvas.Parent is IZoomControl zoomControl)
                    vis = zoomControl.PresenterVisual;
                else
                {
                    var frameworkElement = canvas.Parent as FrameworkElement;
                    if (frameworkElement?.Parent is IZoomControl)
                        vis = ((IZoomControl) frameworkElement.Parent).PresenterVisual;
                }
            }

            var renderBitmap =
                    new RenderTargetBitmap(
                    (int)(vis.DesiredSize.Width * (imgdpi / DEFAULT_DPI) + 100),
                    (int)(vis.DesiredSize.Height * (imgdpi / DEFAULT_DPI) + 100),
                    imgdpi,
                    imgdpi,
                    PixelFormat);

            //Render the graphlayout onto the bitmap.
            renderBitmap.Render(vis);
                
           
            //Create a file stream for saving image
            using (FileStream outStream = new FileStream(path.LocalPath, FileMode.Create))
            {
                //Use png encoder for our data
                BitmapEncoder encoder;
                switch (itype)
                {
                    case ImageType.PNG: encoder = new PngBitmapEncoder();
                        break;
                    case ImageType.JPEG: encoder = new JpegBitmapEncoder() { QualityLevel = imgQuality };
                        break;
                    case ImageType.BMP: encoder = new BmpBitmapEncoder();
                        break;
                    case ImageType.GIF: encoder = new GifBitmapEncoder();
                        break;
                    case ImageType.TIFF: encoder = new TiffBitmapEncoder();
                        break;
                    default: throw new GX_InvalidDataException("ExportToImage() -> Unknown output image format specified!");
                }
                
                //Push the rendered bitmap to it
                encoder.Frames.Add(BitmapFrame.Create(renderBitmap));
                //Save the data to the stream
                encoder.Save(outStream);
            }
            renderBitmap.Clear();
            renderBitmap = null;
            //due to mem leak in wpf :(
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            if (!useZoomControlSurface)
                surface.SetPrintMode(false, true, 100);
        }



        public static void PrintVisualDialog(Visual surface, string description = "", bool compat = false)
        {
            try
            {
                //apply layout rounding
                var isCtrl = surface is Control;
                bool oldLR = false;
                double oldWidth = 0;
                double oldHeight = 0;
                if (isCtrl && compat)
                {
                    var ctrl = (Control) surface;
                    oldLR = ctrl.UseLayoutRounding;
                    if (oldLR != true) ctrl.UseLayoutRounding = true;

                    oldWidth = ctrl.Width;
                    oldHeight = ctrl.Height;
                    ctrl.Width = ctrl.ActualWidth;
                    ctrl.Height = ctrl.ActualHeight;
                }

                var printDialog = new PrintDialog();
                if (printDialog.ShowDialog() == true)
                {
                    printDialog.PrintVisual(surface, description);
                }
                if (isCtrl && compat)
                {
                    var ctrl = (Control)surface;
                    ctrl.UseLayoutRounding = oldLR;
                    ctrl.Width = oldWidth;
                    ctrl.Height = oldHeight;
                }            
            }
            catch (Exception)
            {
                MessageBox.Show("Unexpected exception occured while trying to access default printer. Please ensure that default printer is installed in your OS!");
            }
        }


        public static void PrintToFit(IGraphAreaBase ga, string description, int margin = 0)
        {
            var visual = (Canvas) ga;
            var pd = new PrintDialog();
            if (pd.ShowDialog() == true)
            {
                ga.SetPrintMode(true, true, margin);

                //store original scale
                var originalScale = visual.LayoutTransform;
                //get selected printer capabilities
                var capabilities = pd.PrintQueue.GetPrintCapabilities(pd.PrintTicket);

                //get scale of the print wrt to screen of WPF visual
                var scale = Math.Min(capabilities.PageImageableArea.ExtentWidth / visual.ActualWidth, capabilities.PageImageableArea.ExtentHeight /
                               visual.ActualHeight);

                //Transform the Visual to scale
                var group = new TransformGroup();
                group.Children.Add(new ScaleTransform(scale, scale));
                visual.LayoutTransform = group;
                visual.InvalidateArrange();
                visual.UpdateLayout();

                //now print the visual to printer to fit on the one page.
                pd.PrintVisual(visual, description);

                //apply the original transform.
                visual.LayoutTransform = originalScale;
                ga.SetPrintMode(false, true, margin);
            }
        }

        public static void PrintWithDPI(IGraphAreaBase ga, string description, double dpi, int margin = 0)
        {
            var visual = (Canvas) ga;
            var pd = new PrintDialog();
            if (pd.ShowDialog() == true)
            {
                ga.SetPrintMode(true, true, margin);
                //store original scale
                var originalScale = visual.LayoutTransform;
                //get scale from DPI
                var scale = dpi/DEFAULT_DPI;
                //Transform the Visual to scale
                var group = new TransformGroup();
                group.Children.Add(new ScaleTransform(scale, scale));
                visual.LayoutTransform = group;
                //update visual
                visual.InvalidateArrange();
                visual.UpdateLayout();

                //now print the visual to printer to fit on the one page.
                pd.PrintVisual(visual, description);
                //apply the original transform.
                visual.LayoutTransform = originalScale;
                ga.SetPrintMode(false, true, margin);
            }
        }
    }
}
