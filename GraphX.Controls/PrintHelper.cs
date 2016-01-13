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

namespace GraphX.Controls
{
    internal static class PrintHelper
    {
        /// <summary>
        /// Default image resolution
        /// </summary>
        public const double DEFAULT_DPI = 96d;

        //Set pixelformat of image.
        private static readonly PixelFormat PixelFormat = PixelFormats.Pbgra32;

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
            if(!useZoomControlSurface)
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
                    if(frameworkElement != null && frameworkElement.Parent is IZoomControl)
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
            //due to mem leak in wpf :(
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            if (!useZoomControlSurface)
                surface.SetPrintMode(false, true, 100);
        }


        public static void ShowPrintPreview(Visual surface, string description = "")
        {
            try
            {
                var printDialog = new PrintDialog();
                if (printDialog.ShowDialog() == true)
                {
                    printDialog.PrintVisual(surface, description);
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Unexpected exception occured while trying to access default printer. Please ensure that default printer is installed in your OS!");
            }
        }


        public static void PrintExtended(GraphAreaBase visual, string description, int margin = 0, bool fitPage = false)
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
                var scale = Math.Min(capabilities.PageImageableArea.ExtentWidth / (visual.ActualWidth + margin), capabilities.PageImageableArea.ExtentHeight /
                               (visual.ActualHeight + margin));

                //Transform the Visual to scale
                var group = new TransformGroup();
                if (fitPage)
                    group.Children.Add(new ScaleTransform(scale, scale));
                visual.LayoutTransform = group;

                if (fitPage)
                {
                    //get the size of the printer page
                    var sz = new System.Windows.Size(capabilities.PageImageableArea.ExtentWidth, capabilities.PageImageableArea.ExtentHeight);
                    //update the layout of the visual to the printer page size.
                    visual.Measure(sz);
                    visual.Arrange(new Rect(new System.Windows.Point(capabilities.PageImageableArea.OriginWidth, capabilities.PageImageableArea.OriginHeight), sz));
                }
                //now print the visual to printer to fit on the one page.
                pd.PrintVisual(visual, description);

                //apply the original transform.
                visual.LayoutTransform = originalScale;
                visual.SetPrintMode(false, true, margin);
            }
        }

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

        public static Bitmap RenderTargetBitmapToBitmap(RenderTargetBitmap source)
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

        public static RenderTargetBitmap RenderTargetBitmap(GraphAreaBase surface, bool useZoomControlSurface, double imgdpi)
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



    }
}
