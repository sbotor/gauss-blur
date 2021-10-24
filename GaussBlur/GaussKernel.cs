using System;
using System.Collections.Generic;
using System.Text;

namespace GaussBlur
{
    class GaussKernel
    {
        public double[] Data
        {
            get;
            private set;
        }

        public double StdDev
        {
            get;
            private set;
        }

        public int Size
        {
            get => Data.Length;
        }

        public GaussKernel(int kernelSize, double sd)
        {
            if (kernelSize < 1 || kernelSize % 2 != 1)
            {
                throw new ArgumentException($"Kernel size of {kernelSize} is not valid.");
            }
            
            Data = new double[kernelSize];
            StdDev = sd;

            int maxCenterOffset = (kernelSize - 1) / 2;
            double variance = sd * sd,
                constance = 1 / Math.Sqrt(2.0 * Math.PI * variance),
                kernelSum = 0;

            for (int offset = -maxCenterOffset; offset <= maxCenterOffset; offset++)
            {
                double gaussValue = constance * Math.Exp(-(offset * offset) / (2 * variance));

                Data[offset + maxCenterOffset] = gaussValue;
                kernelSum += gaussValue;
            }

            for (int offset = -maxCenterOffset; offset <= maxCenterOffset; offset++)
            {
                Data[offset + maxCenterOffset] /= kernelSum;
            }
        }
    }
}
