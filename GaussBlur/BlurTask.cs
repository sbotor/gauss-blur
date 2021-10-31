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
        public BlurThreadFactory Factory { get; private set; }

        public double[] Kernel { get; private set; }

        public System.Drawing.Imaging.BitmapData imageData { get; private set; }

        public BlurTask(System.Drawing.Imaging.BitmapData data, BlurThreadFactory factory, double kernelSD)
        {
            Factory = factory;
            imageData = data;
            createKernel(kernelSD);
        }

        private void createKernel(double sd)
        {
            Kernel = new double[5];

            double variance = sd * sd,
                constance = 1 / (Math.Sqrt(2.0 * Math.PI) * sd);

            Kernel[0] = constance * Math.Exp(-2 / variance);
            Kernel[1] = constance * Math.Exp(-0.5 / variance);
            Kernel[2] = constance;

            double kernelSum = Kernel[0] * 2 + Kernel[1] * 2 + Kernel[2];

            Kernel[0] = Kernel[4] = Kernel[0] / kernelSum;
            Kernel[1] = Kernel[3] = Kernel[1] / kernelSum;
            Kernel[2] = Kernel[2] / kernelSum;
        }

        private unsafe void blurInThreads(RGBArray rgbArray, byte* helperP, int[] slices, double* kernelP)
        {
            for (int i = 0; i < Factory.ThreadCount; i++)
            {
                BlurThread thread = Factory.CreateThread(
                    rgbArray.Data,
                    helperP,
                    slices[i],
                    slices[i + 1],
                    rgbArray.Stride,
                    rgbArray.Height,
                    kernelP);

                thread.Start();
            }

            for (int i = 0; i < Factory.ThreadCount; i++)
            {
                Factory.Items[i].Join();
            }
        }

        public void Blur(int repeat = 1)
        {
            RGBArray rgbArray = new RGBArray(imageData);
            int[] slices = rgbArray.Slice(Factory.ThreadCount);
            byte[] helperArray = new byte[rgbArray.Length];

            unsafe
            {
                fixed (byte* helperP = helperArray)
                {
                    fixed (double* kernelP = Kernel)
                    {
                        for (int i = 0; i < repeat; i++)
                        {
                            Factory.Clear();
                            blurInThreads(rgbArray, helperP, slices, kernelP);
                        }
                    }
                }
            }
        }
    }
}
