using System;
using System.Collections.Generic;

namespace GaussBlur
{
    class BlurTask : BlurThreadSynchronizer
    {
        public List<BlurThread> Threads { get; private set; }
        
        public double[] Kernel { get; private set; }

        public ImageData Data { get; private set; }

        public BlurTask(System.Drawing.Imaging.BitmapData data, int threadCount, double kernelSD, int repeats)
            : base(threadCount, repeats)
        {
            Data = new ImageData(data);
            Kernel = CreateKernel(kernelSD);

            Threads = new List<BlurThread>();
        }
        
        public void Clear()
        {
            Reset();
            Threads.Clear();
        }

        public static double[] CreateKernel(double sd)
        {
            double[] kernel = new double[5];

            double variance = sd * sd,
                constance = 1 / (Math.Sqrt(2.0 * Math.PI) * sd);

            kernel[0] = constance * Math.Exp(-2 / variance);
            kernel[1] = constance * Math.Exp(-0.5 / variance);
            kernel[2] = constance;

            double kernelSum = kernel[0] * 2 + kernel[1] * 2 + kernel[2];

            kernel[0] = kernel[4] = kernel[0] / kernelSum;
            kernel[1] = kernel[3] = kernel[1] / kernelSum;
            kernel[2] = kernel[2] / kernelSum;

            return kernel;
        }

        public void RunThreads(IThreadFactory factory)
        {
            unsafe
            {
                fixed(double* kernelP = Kernel)
                {
                    Clear();
                    int[] slices = Data.Slice(ThreadCount);
                    byte[] helper = new byte[Data.Length];

                    fixed (byte* helperP = helper)
                    {
                        for (int i = 0; i < ThreadCount; i++)
                        {
                            BlurThread thread = factory.Create(this, helperP, kernelP, slices[i], slices[i + 1]);
                            Threads.Add(thread);
                            thread.Start();
                        }

                        Threads.ForEach((t) => t.Join());
                    }
                }
            }
        }
    }
}
