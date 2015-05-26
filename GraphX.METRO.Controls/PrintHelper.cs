using System;
using GraphX.PCL.Common.Enums;

namespace GraphX.Controls
{
    internal static class PrintHelper
    {
        /// <summary>
        /// Default image resolution
        /// </summary>
        public const double DefaultDPI = 96d;

        //Set pixelformat of image.
       /////!!! private static PixelFormat pixelFormat = PixelFormats.Pbgra32;

        /// <summary>
        /// Method exports the GraphArea to an png image.
        /// </summary>
        /// <param name="surface">GraphArea control</param>
        /// <param name="path">Image destination path</param>
        /// <param name="imgdpi">Optional image DPI parameter</param>
        /// <param name="imgQuality">Optional image quality parameter (for some formats like JPEG)</param>
        public static void ExportToImage(GraphAreaBase surface, Uri path, ImageType itype, bool useZoomControlSurface = false, double imgdpi = DefaultDPI, int imgQuality = 100)
        {
            //TODO
            //Create a render bitmap and push the surface to it
            /*Visual vis = surface;
            if (useZoomControlSurface)
            {
                if (surface.Parent != null && surface.Parent is IZoomControl)
                    vis = (surface.Parent as IZoomControl).PresenterVisual;
                else if(surface.Parent!=null && surface.Parent is FrameworkElement && (surface.Parent as FrameworkElement).Parent is IZoomControl)
                    vis = ((surface.Parent as FrameworkElement).Parent as IZoomControl).PresenterVisual;
            }
            var renderBitmap =
                    new RenderTargetBitmap(
                                    //(int)surface.ActualWidth,
                                    //(int)surface.ActualHeight,
                    (int)((vis as UIElement).DesiredSize.Width * (imgdpi / DefaultDPI) + 100),
                    (int)((vis as UIElement).DesiredSize.Height * (imgdpi / DefaultDPI) + 100),
                    imgdpi,
                    imgdpi,
                    pixelFormat);

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
            }*/
        }


        public static void ShowPrintPreview(GraphAreaBase surface, string description = "")
        {
            //TODO
           /* try
            {
                var printDialog = new PrintDialog();
                if (printDialog.ShowDialog() == true)
                {
                    printDialog.PrintVisual(surface, description);
                }
            }
            catch (Exception ex)
            {
                new MessageDialog("Unexpected exception occured while trying to acces default printer. Please ensure that default printer is installed in your OS!").Show();
            }*/
        }

        /*public static Bitmap RenderTargetBitmapToBitmap(RenderTargetBitmap source)
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
        }*/

       /* public static RenderTargetBitmap RenderTargetBitmap(GraphAreaBase surface, bool useZoomControlSurface, double imgdpi)
        {
            Visual vis = surface;
            if (useZoomControlSurface)
            {
                if (surface.Parent != null && surface.Parent is IZoomControl)
                    vis = (surface.Parent as IZoomControl).PresenterVisual;
                else if (surface.Parent != null && surface.Parent is FrameworkElement &&
                         (surface.Parent as FrameworkElement).Parent is IZoomControl)
                    vis = ((surface.Parent as FrameworkElement).Parent as IZoomControl).PresenterVisual;
            }
            var renderBitmap =
                new RenderTargetBitmap(
                //(int)surface.ActualWidth,
                //(int)surface.ActualHeight,
                    (int)((vis as UIElement).DesiredSize.Width * (imgdpi / 96) + 100),
                    (int)((vis as UIElement).DesiredSize.Height * (imgdpi / 96) + 100),
                    imgdpi,
                    imgdpi,
                    pixelFormat);

            vis.SetValue(Panel.BackgroundProperty, System.Windows.Media.Brushes.White);
            //Render the graphlayout onto the bitmap.
            renderBitmap.Render(vis);

            return renderBitmap;

        }*/



    }
}
