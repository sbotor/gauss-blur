using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace GaussBlur
{
    class BlurTask
    {
        public BlurThreadFactory Factory
        { get; private set; }

        public RGBArray Image
        { get; private set; }

        public GaussKernel Kernel
        { get; private set; }

        public BlurTask(BlurThreadFactory factory, RGBArray image, GaussKernel kernel)
        {
            Factory = factory;
            Image = image;
            Kernel = kernel;
        }

        public void Blur()
        {
            Thread[] threads = new Thread[Factory.ThreadCount];
            int[] slices = Image.Slice(Factory.ThreadCount);
            double[] helperArray = new double[Image.Length];
            
            unsafe
            {
                fixed (double* dataP = Image.Data,
                    helperP = helperArray,
                    kernelP = Kernel.Data)
                {
                    for (int i = 0; i < Factory.ThreadCount; i++)
                    {
                        BlurThread threadParam = Factory.CreateThread(
                            dataP,
                            helperP,
                            slices[i],
                            slices[i + 1],
                            Image.Stride,
                            Image.Height,
                            kernelP,
                            Kernel.Size);

                        threads[i] = new Thread(new ThreadStart(threadParam.Run));
                        threads[i].Start();
                    }

                    for (int i = 0; i < Factory.ThreadCount; i++)
                    {
                        threads[i].Join();
                    }
                }
            }
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
