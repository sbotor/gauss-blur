using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;

namespace GaussBlur
{
    class BlurTask
    {
        public BlurThreadFactory Factory
        { get; private set; }

        public Bitmap Image
        { get; private set; }

        public GaussKernel Kernel
        { get; private set; }

        public BlurTask(BlurThreadFactory factory, Bitmap image, GaussKernel kernel)
        {
            Factory = factory;
            Image = image;
            Kernel = kernel;
        }

        public void Blur()
        {
            System.Drawing.Rectangle rect = new System.Drawing.Rectangle(0, 0, Image.Width, Image.Height);
            System.Drawing.Imaging.BitmapData imageData =
                Image.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite,
                Image.PixelFormat);

            RGBArray rgbArray = new RGBArray(imageData);
            int[] slices = rgbArray.Slice(Factory.ThreadCount);
            double[] helperArray = new double[rgbArray.Length];
            
            unsafe
            {
                fixed (double* dataP = rgbArray.Data,
                    helperP = helperArray,
                    kernelP = Kernel.Data)
                {
                    for (int i = 0; i < Factory.ThreadCount; i++)
                    {
                        BlurThread thread = Factory.CreateThread(
                            dataP,
                            helperP,
                            slices[i],
                            slices[i + 1],
                            rgbArray.Stride,
                            rgbArray.Height,
                            kernelP,
                            Kernel.Size);

                        thread.Start();
                    }

                    for (int i = 0; i < Factory.ThreadCount; i++)
                    {
                        Factory.Items[i].Join();
                    }
                }
            }

            Marshal.Copy(rgbArray.ToByteArray(), 0, imageData.Scan0, rgbArray.Length);
            Image.UnlockBits(imageData);
        }

        public void Save(string filename)
        {
            if (File.Exists(filename))
            {
                // TODO
            }
        }
    }
}
