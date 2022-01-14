using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Media.Imaging;

namespace GaussBlur.ImageProc
{
    class ImageContainer : IDisposable
    {
        public System.Drawing.Imaging.BitmapData? ImageData { get; private set; }

        public Bitmap? BoundImage { get; private set; }

        public MemoryStream? BoundStream { get; private set; }

        public ImageContainer()
        {
        }

        ~ImageContainer()
        {
            cleanup();
        }
        
        public void LoadImage(string inputDir)
        {
            try
            {
                if (File.Exists(inputDir))
                {
                    cleanup();

                    BoundStream = new MemoryStream();
                    using (FileStream fs = File.OpenRead(inputDir))
                    {
                        fs.CopyTo(BoundStream);
                    }

                    BoundImage = new Bitmap(BoundStream);
                    if (BoundImage.PixelFormat != System.Drawing.Imaging.PixelFormat.Format24bppRgb)
                    {
                        cleanup();
                        throw new ArgumentException("The image format is not supported");
                    }
                }
                else
                {
                    cleanup();
                    throw new FileNotFoundException();
                }
            }
            catch (ArgumentException e)
            {
                cleanup();
                throw e;
            }
            catch (Exception e)
            {
                cleanup();
                throw e;
            }
        }

        public BitmapSource LoadBitmapSource(string inputDir)
        {
            LoadImage(inputDir);
            
            LockImage();
            BitmapSource src = BitmapSource.Create(
                BoundImage.Width, ImageData.Height,
                BoundImage.HorizontalResolution, BoundImage.VerticalResolution,
                System.Windows.Media.PixelFormats.Bgr24, null,
                ImageData.Scan0, ImageData.Stride * ImageData.Height,
                ImageData.Stride);
            UnlockImage();

            return src;
        }

        public void Save(string fileDir)
        {
            using (FileStream outStream = File.Open(fileDir, FileMode.OpenOrCreate))
            {
                if (BoundImage != null)
                {
                    BoundImage.Save(outStream, BoundImage.RawFormat);
                }
            } 
        }

        private void cleanup()
        {
            if (BoundStream != null)
            {
                if (BoundStream.CanRead)
                {
                    BoundStream.Close();
                }
                BoundStream.Dispose();
                BoundStream = null;
            }

            UnlockImage();

            if (BoundImage != null)
            {
                BoundImage.Dispose();
                BoundImage = null;
            }
        }

        public void LockImage()
        {
            if (BoundImage != null)
            {
                ImageData = BoundImage.LockBits(
                                new Rectangle(0, 0, BoundImage.Width, BoundImage.Height),
                                System.Drawing.Imaging.ImageLockMode.ReadWrite,
                                BoundImage.PixelFormat);
            }
        }
        
        public void UnlockImage()
        {
            if (ImageData != null)
            {
                if (BoundImage != null)
                {
                    BoundImage.UnlockBits(ImageData);
                }
                ImageData = null;
            }
        }

        public static BitmapSource CreateBitmapSource(string fileDir)
        {
            if (File.Exists(fileDir))
            {
                using (FileStream stream = File.Open(fileDir, FileMode.Open))
                {
                    using (Bitmap image = new Bitmap(stream))
                    {
                        System.Drawing.Imaging.BitmapData data = image.LockBits(
                        new Rectangle(0, 0, image.Width, image.Height),
                        System.Drawing.Imaging.ImageLockMode.ReadOnly,
                        image.PixelFormat);

                        BitmapSource source = BitmapSource.Create(
                            data.Width, data.Height,
                            image.HorizontalResolution, image.VerticalResolution,
                            getPixelFormat(image.PixelFormat), null,
                            data.Scan0, data.Stride * data.Height,
                            data.Stride);

                        image.UnlockBits(data);
                        return source;
                    }
                }
            }
            else
            {
                throw new FileNotFoundException();
            }
        }

        private static System.Windows.Media.PixelFormat getPixelFormat(System.Drawing.Imaging.PixelFormat format)
        {
            switch(format)
            {
                case System.Drawing.Imaging.PixelFormat.Format24bppRgb:
                    return System.Windows.Media.PixelFormats.Bgr24;
                case System.Drawing.Imaging.PixelFormat.Format32bppArgb:
                    return System.Windows.Media.PixelFormats.Bgra32;
                default:
                    throw new ArgumentException();
            }
        }

        public void Dispose()
        {
            cleanup();
        }
    }
}
