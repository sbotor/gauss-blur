using System;
using System.Collections.Generic;
using System.Text;

namespace GaussBlur.Threading
{
    unsafe abstract class BlurThreadFactory
    {
        public float[] Kernel { get; protected set; }

        protected BlurTask task;
        protected byte* helperP;
        protected float* kernelP;

        public BlurThreadFactory(double kernelSD)
        {
            Kernel = createKernel(kernelSD);

            task = null;
            helperP = null;
            kernelP = null;
        }

        public abstract BlurThread Create(int start, int end);

        public virtual void Init(BlurTask task, byte* helperP, float* kernelP)
        {
            this.task = task;
            this.helperP = helperP;
            this.kernelP = kernelP;
        }

        public virtual float[] createKernel(double sd)
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
    }
}
