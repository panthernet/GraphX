using System;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GraphX.PCL.Common.Enums;
using GraphX.PCL.Common.Exceptions;
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
        public static double CalculateEstimatedDPI(GraphAreaBase vis, double imgdpi, double dpiStep, int estPixelCount)
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


        private static int CalulateSize(Size desiredSize, double dpi)
        {
            return (int) (desiredSize.Width*(dpi/DEFAULT_DPI) + 100) *
                   (int) (desiredSize.Height*(dpi/DEFAULT_DPI) + 100);
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
        public static void ExportToImage(GraphAreaBase surface, Uri path, ImageType itype, bool useZoomControlSurface = false, double imgdpi = DEFAULT_DPI, int imgQuality = 100)
        {
            if (!useZoomControlSurface)
                surface.SetPrintMode(true, true, 100);
            //Create a render bitmap and push the surface to it
            UIElement vis = surface;
            if (useZoomControlSurface)
            {
                var zoomControl = surface.Parent as IZoomControl;
                if (zoomControl != null)
                    vis = zoomControl.PresenterVisual;
                else
                {
                    var frameworkElement = surface.Parent as FrameworkElement;
                    if (frameworkElement != null && frameworkElement.Parent is IZoomControl)
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


        public static void PrintToFit(GraphAreaBase visual, string description, int margin = 0)
        {
            var pd = new PrintDialog();
            if (pd.ShowDialog() == true)
            {
                visual.SetPrintMode(true, true, margin);

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
                visual.SetPrintMode(false, true, margin);
            }
        }

        public static void PrintWithDPI(GraphAreaBase visual, string description, double dpi, int margin = 0)
        {
            var pd = new PrintDialog();
            if (pd.ShowDialog() == true)
            {
                visual.SetPrintMode(true, true, margin);
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
                visual.SetPrintMode(false, true, margin);
            }
        }

        #region OTHER

        /* /// <summary>
        /// experimental method, not working
        /// </summary>
        /// <param name="controlToPrint"></param>
        public void PrintExp(PocGraphLayout controlToPrint)
        {
            var fixedDoc = new FixedDocument();
            var pageContent = new PageContent();
            var fixedPage = new FixedPage();

            zoomControl.Content = null;

            //Create first page of document
            fixedPage.Children.Add(controlToPrint);
            FixedPage.SetLeft(controlToPrint, 0);
            FixedPage.SetTop(controlToPrint, 0);
            controlToPrint.InvalidateMeasure();
            controlToPrint.UpdateLayout();
            fixedPage.UpdateLayout();

            pageContent.Child = fixedPage;
            fixedDoc.Pages.Add(pageContent);
            var dlg = new SaveFileDialog
            {
                FileName = "print",
                DefaultExt = ".xps",
                Filter = "XPS Documents (.xps)|*.xps"
            };
            // Show save file dialog box
            bool? result = dlg.ShowDialog();

            // Process save file dialog box results
            if (result == true)
            {
                // Save document
                string filename = dlg.FileName;

                var xpsd = new XpsDocument(filename, FileAccess.ReadWrite);
                var xw = XpsDocument.CreateXpsDocumentWriter(xpsd);
                xw.Write(fixedDoc);
                xpsd.Close();
            }
            fixedPage.Children.Clear();
            zoomControl.Content = graphLayout;
        }
        */

        private static Bitmap RenderTargetBitmapToBitmap(RenderTargetBitmap source)
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                //Use png encoder for our data
                PngBitmapEncoder encoder = new PngBitmapEncoder();

                //Push the rendered bitmap to it
                encoder.Frames.Add(BitmapFrame.Create(source));

                //Save the data to the stream
                encoder.Save(outStream);
                return new Bitmap(outStream);
            }
        }

        private static RenderTargetBitmap RenderTargetBitmap(GraphAreaBase surface, bool useZoomControlSurface, double imgdpi)
        {
            UIElement vis = surface;
            if (useZoomControlSurface)
            {
                var zoomControl = surface.Parent as IZoomControl;
                if (zoomControl != null)
                    vis = zoomControl.PresenterVisual;
                else
                {
                    var frameworkElement = surface.Parent as FrameworkElement;
                    if (frameworkElement != null && frameworkElement.Parent is IZoomControl)
                        vis = ((IZoomControl) frameworkElement.Parent).PresenterVisual;
                }
            }
            var renderBitmap =
                new RenderTargetBitmap(
                //(int)surface.ActualWidth,
                //(int)surface.ActualHeight,
                    (int)(vis.DesiredSize.Width * (imgdpi / 96) + 100),
                    (int)(vis.DesiredSize.Height * (imgdpi / 96) + 100),
                    imgdpi,
                    imgdpi,
                    PixelFormat);

            vis.SetValue(Panel.BackgroundProperty, Brushes.White);
            //Render the graphlayout onto the bitmap.
            renderBitmap.Render(vis);

            return renderBitmap;

        }

        #endregion

    }
}
