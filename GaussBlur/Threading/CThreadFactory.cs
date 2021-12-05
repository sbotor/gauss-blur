using System;
using System.Collections.Generic;
using System.Text;

namespace GaussBlur.Threading
{
    unsafe class CThreadFactory : IThreadFactory
    {
        private double[] _kernel;
        public double[] Kernel { get => _kernel; }

        public CThreadFactory(double kernelSD)
        {
            _kernel = createKernel(kernelSD);
        }
        
        public BlurThread Create(BlurTask task, byte* helperP, double* kernelP, int start, int end)
        {
            return new CThread(task, helperP, kernelP, start, end);
        }

        private static double[] createKernel(double sd)
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
    }
}
