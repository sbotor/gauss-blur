using System;
using System.Collections.Generic;
using System.Text;

namespace GaussBlur.Threading
{
    unsafe abstract class BlurThreadFactory
    {
        public float[] Kernel { get; protected set; }

        public int[] FixedPointKernel { get; protected set; }

        public BlurTask Task { get; set; }
        public byte* HelperP { get; set; }
        public float* KernelP { get; set; }
        public int* FixedPointKernelP { get; set; }

        public BlurThreadFactory(double kernelSD)
        {
            Kernel = CreateKernel(kernelSD);

            Task = null;
            HelperP = null;
            KernelP = null;
        }

        public abstract BlurThread Create(int start, int end);

        public virtual void Init(BlurTask task, byte* helperP, float* kernelP)
        {
            Task = task;
            HelperP = helperP;
            KernelP = kernelP;
        }

        public virtual void Init(BlurTask task, byte* helperP, int* fixedPointKernelP)
        {
            Task = task;
            HelperP = helperP;
            FixedPointKernelP = fixedPointKernelP;
        }

        public virtual float[] CreateKernel(double sd)
        {
            float[] kernel = new float[3];

            double variance = sd * sd,
                constance = 1 / (Math.Sqrt(2.0 * Math.PI) * sd);

            kernel[2] = (float)(constance * Math.Exp(-2 / variance));
            kernel[1] = (float)(constance * Math.Exp(-0.5 / variance));
            kernel[0] = (float)constance;

            float kernelSum = kernel[0] + kernel[1] * 2 + kernel[2] * 2;

            kernel[0] = kernel[0] / kernelSum;
            kernel[1] = kernel[1] / kernelSum;
            kernel[2] = kernel[2] / kernelSum;

            return kernel;
        }

        public int[] ObtainFixedPointKernel()
        {
            FixedPointKernel = new int[Kernel.Length];

            for (int i = 0; i < Kernel.Length; i++)
            {
                FixedPointKernel[i] = (int)(Kernel[i] * 256f + 0.5f);
            }

            return FixedPointKernel;
        }
    }
}
