using System;
using System.Collections.Generic;
using System.Text;

using GaussBlur.DLL;

namespace GaussBlur.Threading
{
    class AsmThreadFactory : IThreadFactory
    {
        private double[] _kernel;
        public double[] Kernel { get => _kernel; }

        private bool initialised;

        public AsmThreadFactory(double kernelSD)
        {
            initialised = false;

            _kernel = createKernel(kernelSD);
        }

        public unsafe BlurThread Create(BlurTask task, byte* helperP, double* kernelP, int start, int end)
        {
            if (!initialised)
            {
                AsmLib.Init(
                    task.Data.Data,
                    helperP,
                    task.Data.Stride,
                    task.Data.Height,
                    kernelP);

                initialised = true;
            }
            
            return new AsmThread(task, helperP, kernelP, start, end);
        }

        private static double[] createKernel(double sd)
        {
            double[] kernel = new double[12];

            double variance = sd * sd,
                constance = 1 / (Math.Sqrt(2.0 * Math.PI) * sd);

            kernel[8] = constance * Math.Exp(-2 / variance);
            kernel[4] = constance * Math.Exp(-0.5 / variance);
            kernel[0] = constance;

            double kernelSum = kernel[0] * 2 + kernel[4] * 2 + kernel[8];

            kernel[0] = kernel[1] = kernel[2] = kernel[3] =
                kernel[0] / kernelSum;
            
            kernel[4] = kernel[5] = kernel[6] = kernel[7] =
                kernel[4] / kernelSum;
            
            kernel[8] = kernel[9] = kernel[10] = kernel[11] =
                kernel[8] / kernelSum;

            return kernel;
        }
    }
}
