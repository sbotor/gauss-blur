using System;
using System.Collections.Generic;
using System.Text;

namespace GaussBlur.Threading
{
    unsafe abstract class BlurThreadFactory
    {
        public BlurTask Task { get; set; }
        public byte* HelperP { get; set; }

        public double KernelSD { get; set; }

        public BlurThreadFactory(double kernelSD)
        {
            Task = null;
            HelperP = null;
            KernelSD = kernelSD;
        }

        public abstract BlurThread Create(int start, int end);

        public virtual void Init(BlurTask task, byte* helperP, void* kernelP)
        {
            Task = task;
            HelperP = helperP;
        }

        public virtual float[] CreateFloatKernel()
        {
            float[] kernel = new float[3];

            double variance = KernelSD * KernelSD,
                constance = 1 / (Math.Sqrt(2.0 * Math.PI) * KernelSD);

            kernel[2] = (float)(constance * Math.Exp(-2 / variance));
            kernel[1] = (float)(constance * Math.Exp(-0.5 / variance));
            kernel[0] = (float)constance;

            float kernelSum = kernel[0] + kernel[1] * 2 + kernel[2] * 2;

            kernel[0] = kernel[0] / kernelSum;
            kernel[1] = kernel[1] / kernelSum;
            kernel[2] = kernel[2] / kernelSum;

            return kernel;
        }

        public int[] CreateFixedKernel()
        {
            return CreateFixedKernel(CreateFloatKernel());
        }

        public int[] CreateFixedKernel(float[] floatKernel)
        {
            int[] fixedKernel = new int[floatKernel.Length];

            for (int i = 0; i < floatKernel.Length; i++)
            {
                fixedKernel[i] = (int)(floatKernel[i] * Math.Pow(2, 24) + 0.5f);
            }

            return fixedKernel;
        }
    }
}
