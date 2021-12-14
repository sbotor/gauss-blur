using System;
using System.Collections.Generic;
using System.Text;

using GaussBlur.DLL;

namespace GaussBlur.Threading
{
    unsafe class AsmThreadFactory : BlurThreadFactory
    {   
        public AsmThreadFactory(double kernelSD) : base(kernelSD)
        {
        }

        public override BlurThread Create(int start, int end)
        {   
            return new AsmThread(Task, start, end);
        }

        public override void Init(BlurTask task, byte* helperP, float* kernelP)
        {
            base.Init(task, helperP, kernelP);
            
            AsmLib.Init(task.Data.Data, helperP, task.Data.Stride,
                task.Data.Height, kernelP);
        }

        public override unsafe void Init(BlurTask task, byte* helperP, int* fixedPointKernelP)
        {
            base.Init(task, helperP, fixedPointKernelP);

            AsmLib.Init(task.Data.Data, helperP, task.Data.Stride,
                task.Data.Height, fixedPointKernelP);
        }

        public override float[] CreateKernel(double sd)
        {
            float[] kernel = new float[16];

            double variance = sd * sd,
                constance = 1 / (Math.Sqrt(2.0 * Math.PI) * sd);

            kernel[0] = (float)(constance * Math.Exp(-2 / variance));
            kernel[3] = (float)(constance * Math.Exp(-0.5 / variance));
            kernel[6] = (float)constance;

            float kernelSum = kernel[0] * 2 + kernel[3] * 2 + kernel[6];

            kernel[6] = kernel[7] = kernel[8] = kernel[6] / kernelSum;

            kernel[3] = kernel[4] = kernel[5] = 
                kernel[9] = kernel[10] = kernel[11] = kernel[3] / kernelSum;
            
            kernel[0] = kernel[1] = kernel[2] =
               kernel[12] = kernel[13] = kernel[14] = kernel[0] / kernelSum;

            return kernel;
        }
    }
}
